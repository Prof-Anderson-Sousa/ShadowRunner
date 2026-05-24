using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject personagemPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("HUD")]
    [SerializeField] private TMP_Text pontuacaoTexto;
    [SerializeField] private TMP_Text essenciaTexto;
    [SerializeField] private TMP_Text ondasTexto;
    [SerializeField] private TMP_Text vidaTexto;
    [SerializeField] private TMP_Text resumoGameOverTexto;

    public float TempoDejogo { get; private set; }
    public int Pontuacao { get; private set; }
    public int Essencia { get; private set; }
    public int MorcegosDerrotados { get; private set; }
    public int OndaAtual { get; private set; } = 1;
    public Transform JogadorAtual { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SpawnarPersonagem();
        AtualizarHUD();
    }

    void Update()
    {
        TempoDejogo += Time.deltaTime;
    }

    void SpawnarPersonagem()
    {
        GameObject personagemInstanciado = Instantiate(personagemPrefab, spawnPoint.position, Quaternion.identity);
        JogadorAtual = personagemInstanciado.transform;
    }

    public void ReiniciarJogo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void EncerrarJogo()
    {
        AtualizarResumoGameOver();
        MenuController.Instance?.MostrarGameOver();
    }

    public void RegistrarMorcegoDerrotado(int pontosRecebidos, int essenciaRecebida)
    {
        MorcegosDerrotados++;
        Pontuacao += pontosRecebidos;
        Essencia += essenciaRecebida;
        AtualizarHUD();
    }

    public void RegistrarNovaOnda(int novaOnda)
    {
        OndaAtual = Mathf.Max(1, novaOnda);

        if (OndaAtual > 1)
        {
            Pontuacao += 150;
            Essencia += 1;
        }

        AtualizarHUD();
    }

    public bool GastarEssencia(int custo)
    {
        if (Essencia < custo)
            return false;

        Essencia -= custo;
        AtualizarHUD();
        return true;
    }

    public void AtualizarVidaJogador(int vidaAtual, int vidaMaxima)
    {
        if (vidaTexto == null)
            return;

        vidaTexto.text = $"Vida: {vidaAtual}/{vidaMaxima}";
    }

    void AtualizarHUD()
    {
        if (pontuacaoTexto != null)
            pontuacaoTexto.text = $"Pontos: {Pontuacao}";

        if (essenciaTexto != null)
            essenciaTexto.text = $"Essencia: {Essencia}";

        if (ondasTexto != null)
            ondasTexto.text = $"Onda: {OndaAtual}";

        PlayerCombat playerCombat = JogadorAtual != null
            ? JogadorAtual.GetComponent<PlayerCombat>()
            : null;

        if (playerCombat != null)
            AtualizarVidaJogador(playerCombat.CurrentHealth, playerCombat.MaxHealth);
    }

    void AtualizarResumoGameOver()
    {
        if (resumoGameOverTexto == null)
            return;

        resumoGameOverTexto.text =
            $"Morcegos derrotados: {MorcegosDerrotados}\n" +
            $"Tempo sobrevivido: {TempoDejogo:F1}s";
    }
}
