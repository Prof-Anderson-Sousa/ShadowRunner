using UnityEngine;

public class CameraSeguirJogador : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.35f, -10f);
    [SerializeField] private float suavizacao = 0.2f;
    [SerializeField] private float antecipacaoHorizontal = 0.6f;
    [SerializeField] private float zoomOrtografico = 4.1f;
    [SerializeField] private float limiteEsquerdo = -5.8f;
    [SerializeField] private float limiteDireito = 5.8f;
    [SerializeField] private float limiteInferior = 0.2f;
    [SerializeField] private float limiteSuperior = 1.4f;

    private Transform alvo;
    private Rigidbody2D alvoRb;
    private Vector3 velocidadeAtual;
    private Camera cameraPrincipal;

    void Awake()
    {
        cameraPrincipal = GetComponent<Camera>();

        if (cameraPrincipal != null && cameraPrincipal.orthographic)
            cameraPrincipal.orthographicSize = zoomOrtografico;
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
        destino.x = Mathf.Clamp(destino.x, limiteEsquerdo, limiteDireito);
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
}
