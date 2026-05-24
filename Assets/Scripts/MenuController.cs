using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance { get; private set; }

    [Header("Paineis")]
    [SerializeField] private GameObject menuInicial;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuGameOver;

    [Header("Configuracao")]
    [SerializeField] private KeyCode teclaPause = KeyCode.Escape;
    [SerializeField] private bool abrirMenuInicialAoIniciar = true;

    public bool JogoIniciado { get; private set; }
    public bool JogoPausado { get; private set; }
    public bool JogoTerminou { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FecharTodosMenus();

        if (abrirMenuInicialAoIniciar)
        {
            AbrirMenuInicial();
        }
        else
        {
            ContinuarJogo();
        }
    }

    private void Update()
    {
        if (!JogoIniciado || JogoTerminou)
        {
            return;
        }

        if (Input.GetKeyDown(teclaPause))
        {
            TogglePause();
        }
    }

    public void IniciarJogo()
    {
        JogoIniciado = true;
        JogoTerminou = false;
        menuInicial?.SetActive(false);
        menuGameOver?.SetActive(false);
        ContinuarJogo();
    }

    public void TogglePause()
    {
        if (JogoPausado)
        {
            ContinuarJogo();
            return;
        }

        PausarJogo();
    }

    public void PausarJogo()
    {
        if (JogoTerminou || !JogoIniciado)
        {
            return;
        }

        JogoPausado = true;
        menuPause?.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ContinuarJogo()
    {
        JogoPausado = false;
        menuPause?.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MostrarGameOver()
    {
        if (JogoTerminou)
        {
            return;
        }

        JogoTerminou = true;
        JogoPausado = false;
        menuPause?.SetActive(false);
        menuInicial?.SetActive(false);
        menuGameOver?.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ReiniciarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SairDoJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void AbrirMenuInicial()
    {
        JogoIniciado = false;
        JogoPausado = true;
        JogoTerminou = false;
        menuInicial?.SetActive(true);
        menuPause?.SetActive(false);
        menuGameOver?.SetActive(false);
        Time.timeScale = 0f;
    }

    private void FecharTodosMenus()
    {
        menuInicial?.SetActive(false);
        menuPause?.SetActive(false);
        menuGameOver?.SetActive(false);
    }
}
