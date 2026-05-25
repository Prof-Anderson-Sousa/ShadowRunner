using UnityEngine;

public class CameraSeguirJogador : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.35f, -10f);
    [SerializeField] private float suavizacao = 0.2f;
    [SerializeField] private float antecipacaoHorizontal = 0.6f;
    [SerializeField] private float zoomOrtografico = 4.1f;
    [SerializeField] private float limiteInferior = 0.2f;
    [SerializeField] private float limiteSuperior = 1.4f;
    [SerializeField] private string nomeTilemapLimiteEsquerdo = "Cen\u00e1rio Tilemap";
    [SerializeField] private float margemVisualEsquerda = 2f;

    private Transform alvo;
    private Rigidbody2D alvoRb;
    private Vector3 velocidadeAtual;
    private Camera cameraPrincipal;
    private Renderer tilemapLimiteEsquerdoRenderer;

    void Awake()
    {
        cameraPrincipal = GetComponent<Camera>();

        if (cameraPrincipal != null && cameraPrincipal.orthographic)
            cameraPrincipal.orthographicSize = zoomOrtografico;

        TentarEncontrarTilemapLimiteEsquerdo();
    }

    void LateUpdate()
    {
        if (alvo == null)
        {
            TentarEncontrarJogador();
            if (alvo == null)
                return;
        }

        float deslocamentoHorizontal = 0f;

        if (alvoRb != null && Mathf.Abs(alvoRb.linearVelocity.x) > 0.05f)
            deslocamentoHorizontal = Mathf.Sign(alvoRb.linearVelocity.x) * antecipacaoHorizontal;

        Vector3 destino = alvo.position + offset + new Vector3(deslocamentoHorizontal, 0f, 0f);
        destino.x = Mathf.Max(destino.x, CalcularLimiteEsquerdoCamera());
        destino.y = Mathf.Clamp(destino.y, limiteInferior, limiteSuperior);
        destino.z = offset.z;
        transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocidadeAtual, suavizacao);
    }

    void TentarEncontrarJogador()
    {
        if (GameManager.Instance != null && GameManager.Instance.JogadorAtual != null)
        {
            alvo = GameManager.Instance.JogadorAtual;
        }
        else
        {
            GameObject jogador = GameObject.FindGameObjectWithTag("Player");
            if (jogador != null)
                alvo = jogador.transform;
        }

        if (alvo != null)
            alvoRb = alvo.GetComponent<Rigidbody2D>();
    }

    void TentarEncontrarTilemapLimiteEsquerdo()
    {
        GameObject tilemap = GameObject.Find(nomeTilemapLimiteEsquerdo);

        if (tilemap != null)
            tilemapLimiteEsquerdoRenderer = tilemap.GetComponent<Renderer>();
    }

    float CalcularLimiteEsquerdoCamera()
    {
        if (cameraPrincipal == null || !cameraPrincipal.orthographic)
            return float.NegativeInfinity;

        if (tilemapLimiteEsquerdoRenderer == null)
            TentarEncontrarTilemapLimiteEsquerdo();

        if (tilemapLimiteEsquerdoRenderer == null)
            return float.NegativeInfinity;

        float metadeLarguraCamera = cameraPrincipal.orthographicSize * cameraPrincipal.aspect;

        return tilemapLimiteEsquerdoRenderer.bounds.min.x
               + metadeLarguraCamera
               + margemVisualEsquerda;
    }
}
