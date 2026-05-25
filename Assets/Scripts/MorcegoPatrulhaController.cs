using UnityEngine;

public class MorcegoPatrulhaController : MonoBehaviour
{
    [Header("Movimento Horizontal")]
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private int direcaoInicial = -1;

    [Header("Deteccao de Parede")]
    [SerializeField] private Transform detectorParede;
    [SerializeField] private float distanciaDeteccaoParede = 0.25f;
    [SerializeField] private LayerMask camadaParedeOuTilemap;

    [Header("Pontuacao")]
    [SerializeField] private int pontosAoEncostarNoPlayer = 25;
    [SerializeField] private int pontosPerdidosAoBaterSemMatar = 40;
    [SerializeField] private int pontosBaseAoMatar = 100;
    [SerializeField] private int multiplicadorPontosAoMatar = 3;
    [SerializeField] private int essenciaAoMatar = 1;

    [Header("Empurrao no Player")]
    [SerializeField] private float forcaEmpurraoHorizontal = 7f;
    [SerializeField] private float forcaEmpurraoVertical = 4f;
    [SerializeField] private float cooldownDanoPlayer = 0.8f;

    [Header("Morte")]
    [SerializeField] private float tempoParaDestruir = 0.25f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private int direcaoAtual;
    private bool morto;
    private float proximoDanoLiberado;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        direcaoAtual = direcaoInicial >= 0 ? 1 : -1;

        GarantirDetectorParede();
        AtualizarVisual();
    }

    private void FixedUpdate()
    {
        if (morto)
            return;

        Patrulhar();
        VerificarParede();
    }

    private void Patrulhar()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(direcaoAtual * velocidade, rb.linearVelocity.y);
    }

    private void VerificarParede()
    {
        if (detectorParede == null)
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            detectorParede.position,
            Vector2.right * direcaoAtual,
            distanciaDeteccaoParede,
            camadaParedeOuTilemap
        );

        if (hit.collider != null)
        {
            Virar();
        }
    }

    private void Virar()
    {
        direcaoAtual *= -1;
        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = direcaoAtual < 0;

        if (detectorParede != null)
        {
            float x = Mathf.Abs(detectorParede.localPosition.x) * direcaoAtual;
            detectorParede.localPosition = new Vector3(x, detectorParede.localPosition.y, 0f);
        }
    }

    private void GarantirDetectorParede()
    {
        if (detectorParede != null)
            return;

        Transform encontrado = transform.Find("DetectorParede");

        if (encontrado != null)
        {
            detectorParede = encontrado;
            return;
        }

        GameObject novoDetector = new GameObject("DetectorParede");
        novoDetector.transform.SetParent(transform);
        novoDetector.transform.localPosition = new Vector3(0.45f, 0f, 0f);
        detectorParede = novoDetector.transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (morto)
            return;

        if (collision.collider.CompareTag("Player"))
        {
            BaterNoPlayer(collision.collider);
            return;
        }

        if (EstaNaLayerSelecionada(collision.collider.gameObject.layer, camadaParedeOuTilemap))
        {
            Virar();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (morto)
            return;

        if (other.CompareTag("Player"))
        {
            BaterNoPlayer(other);
            return;
        }

        if (EstaNaLayerSelecionada(other.gameObject.layer, camadaParedeOuTilemap))
        {
            Virar();
        }
    }

    private bool EstaNaLayerSelecionada(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }

    private void BaterNoPlayer(Collider2D playerCollider)
    {
        if (Time.time < proximoDanoLiberado)
            return;

        proximoDanoLiberado = Time.time + cooldownDanoPlayer;

        GameManager.Instance?.RemoverPontos(pontosAoEncostarNoPlayer);

        EmpurrarPlayer(playerCollider);
    }

    public void ReceberAtaqueEspada()
    {
        if (morto)
            return;

        int pontosFinais = pontosBaseAoMatar * multiplicadorPontosAoMatar;

        GameManager.Instance?.RegistrarMorcegoDerrotado(pontosFinais, essenciaAoMatar);

        Morrer();
    }

    public void BaterSemMatar(Collider2D playerCollider)
    {
        if (morto)
            return;

        GameManager.Instance?.RemoverPontos(pontosPerdidosAoBaterSemMatar);

        EmpurrarPlayer(playerCollider);
    }

    private void EmpurrarPlayer(Collider2D playerCollider)
    {
        if (playerCollider == null)
            return;

        Movimento movimento = playerCollider.GetComponent<Movimento>() ?? playerCollider.GetComponentInParent<Movimento>();

        if (movimento == null)
            return;

        Vector2 direcaoEmpurrao = playerCollider.transform.position - transform.position;

        if (Mathf.Abs(direcaoEmpurrao.x) < 0.01f)
            direcaoEmpurrao.x = -direcaoAtual;

        movimento.ReceberEmpurrao(
            direcaoEmpurrao,
            forcaEmpurraoHorizontal,
            forcaEmpurraoVertical
        );
    }

    private void Morrer()
    {
        morto = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (col != null)
            col.enabled = false;

        Destroy(gameObject, tempoParaDestruir);
    }

    private void OnDrawGizmosSelected()
    {
        GarantirDetectorParede();

        if (detectorParede == null)
            return;

        int direcaoGizmo = direcaoAtual == 0
            ? (direcaoInicial >= 0 ? 1 : -1)
            : direcaoAtual;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            detectorParede.position,
            detectorParede.position + Vector3.right * direcaoGizmo * distanciaDeteccaoParede
        );
    }
}