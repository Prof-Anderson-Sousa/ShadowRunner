using UnityEngine;

public class MorcegoController : MonoBehaviour
{
    [SerializeField] private float velocidade = 4f;
    [SerializeField] private float amplitudeOnda = 0.8f;
    [SerializeField] private float frequenciaOnda = 2f;
    [SerializeField] private float distanciaRepulsao = 1.5f;
    [SerializeField] private float forcaRepulsao = 2f;
    [SerializeField] private int recompensaPontos = 100;
    [SerializeField] private int recompensaEssencia = 1;
    [SerializeField] private int recompensaPontosAcaoEspecial = 60;

    private Rigidbody2D rb;
    private Transform jogador;
    private SpriteRenderer spriteRenderer;
    private float yInicial;
    private bool morto = false;
    private float direcaoX = -1f;

    private float limiteEsquerdo = -10f;
    private float limiteDireito = 12f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        yInicial = transform.position.y;

        if (GameManager.Instance != null)
        {
            float tempo = GameManager.Instance.TempoDejogo;
            velocidade += tempo * 0.03f;
        }
    }

    void Update()
    {
        if (morto) return;

        if (jogador == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) jogador = obj.transform;
            return;
        }

        if (transform.position.x <= limiteEsquerdo)
        {
            direcaoX = 1f;
            yInicial = transform.position.y;
        }
        else if (transform.position.x >= limiteDireito)
        {
            direcaoX = -1f;
            yInicial = transform.position.y;
        }

        Vector2 repulsao = CalcularRepulsao();
        float novoX = transform.position.x + (direcaoX * velocidade + repulsao.x) * Time.deltaTime;
        float novoY = yInicial + Mathf.Sin(Time.time * frequenciaOnda) * amplitudeOnda + repulsao.y * Time.deltaTime;

        rb.MovePosition(new Vector2(novoX, novoY));
        spriteRenderer.flipX = direcaoX < 0;
    }

    Vector2 CalcularRepulsao()
    {
        Vector2 forca = Vector2.zero;
        GameObject[] inimigos = GameObject.FindGameObjectsWithTag("Inimigo");

        foreach (GameObject outro in inimigos)
        {
            if (outro == gameObject) continue;

            float distancia = Vector2.Distance(transform.position, outro.transform.position);

            if (distancia < distanciaRepulsao && distancia > 0.01f)
            {
                Vector2 direcaoAfastamento = (transform.position - outro.transform.position).normalized;
                float intensidade = (distanciaRepulsao - distancia) / distanciaRepulsao;
                forca += direcaoAfastamento * intensidade * forcaRepulsao;
            }
        }

        return forca;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Projetil"))
        {
            Destroy(col.gameObject);
            Derrotar(recompensaPontos, recompensaEssencia);
            return;
        }

        if (col.CompareTag("Player"))
        {
            Movimento movimentoPlayer = col.GetComponent<Movimento>();

            if (movimentoPlayer == null)
                movimentoPlayer = col.GetComponentInParent<Movimento>();

            bool veioDeCima = false;
            bool estaCaindo = false;

            if (movimentoPlayer != null)
            {
                veioDeCima = movimentoPlayer.AlturaDoPe() > transform.position.y + 0.1f;
                estaCaindo = movimentoPlayer.EstaCaindo();
            }

            if (movimentoPlayer != null && veioDeCima && estaCaindo)
            {
                Derrotar(recompensaPontos, recompensaEssencia);
                movimentoPlayer.QuicarAposPisar();
                return;
            }

            GameManager.Instance.EncerrarJogo();
        }
    }

    public void DerrotarPorAcaoEspecial()
    {
        Derrotar(recompensaPontosAcaoEspecial, 0);
    }

    void Derrotar(int pontosRecebidos, int essenciaRecebida)
    {
        if (morto)
            return;

        GameManager.Instance?.RegistrarMorcegoDerrotado(pontosRecebidos, essenciaRecebida);
        Morrer();
    }

    void Morrer()
    {
        morto = true;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 0.4f);
    }
}
