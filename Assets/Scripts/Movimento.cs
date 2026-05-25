using UnityEngine;

public class Movimento : MonoBehaviour
{
    private float horizontalInput;
    private Rigidbody2D rb;

    [SerializeField] private int velocidade = 5;
    [SerializeField] private Transform peDoPersonagem;
    [SerializeField] private LayerMask chaoLayer;
    [SerializeField] private string nomeTilemapLimiteEsquerdo = "Cenário Tilemap";
    [SerializeField] private float margemLimiteEsquerdo = 0.2f;

    private bool estaNoChao;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerCombat playerCombat;
    private Renderer tilemapLimiteEsquerdoRenderer;
    private Collider2D personagemCollider;

    [Header("Acao Especial")]
    [SerializeField] private KeyCode teclaAcaoEspecial = KeyCode.E;
    [SerializeField] private int custoAcaoEspecial = 3;
    [SerializeField] private float raioAcaoEspecial = 2.4f;
    [SerializeField] private float impulsoAcaoEspecial = 8f;
    [SerializeField] private float cooldownAcaoEspecial = 1.2f;

    private int movendoHash = Animator.StringToHash("movendo");
    private int saltandoHash = Animator.StringToHash("saltando");
    private float proximaAcaoEspecialLiberada;

    private bool controlesBloqueados;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCombat = GetComponent<PlayerCombat>();
        personagemCollider = GetComponent<Collider2D>();

        GarantirPeDoPersonagem();
        TentarEncontrarTilemapLimiteEsquerdo();
    }

    void Update()
    {
        if (playerCombat != null && playerCombat.IsDead)
            return;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetControlesBloqueados(false);
            Debug.Log("Controles do jogador desbloqueados manualmente.");
        }

        GarantirPeDoPersonagem();

        if (controlesBloqueados)
        {
            horizontalInput = 0f;

            estaNoChao = Physics2D.OverlapCircle(peDoPersonagem.position, 0.2f, chaoLayer);

            if (animator != null)
            {
                animator.SetBool(movendoHash, horizontalInput != 0);
                animator.SetBool(saltandoHash, !estaNoChao);
            }

            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && estaNoChao)
        {
            rb.AddForce(Vector2.up * 600);
        }

        if (Input.GetKeyDown(teclaAcaoEspecial))
        {
            TentarUsarAcaoEspecial();
        }

        estaNoChao = Physics2D.OverlapCircle(peDoPersonagem.position, 0.2f, chaoLayer);

        if (animator != null)
        {
            animator.SetBool(movendoHash, horizontalInput != 0);
            animator.SetBool(saltandoHash, !estaNoChao);
        }

        if (spriteRenderer != null)
        {
            if (horizontalInput > 0)
                spriteRenderer.flipX = false;
            else if (horizontalInput < 0)
                spriteRenderer.flipX = true;
        }
    }

    public void ReceberEmpurrao(Vector2 direcao, float forcaHorizontal, float forcaVertical)
    {
        if (rb == null)
            return;

        direcao.Normalize();

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            new Vector2(direcao.x * forcaHorizontal, forcaVertical),
            ForceMode2D.Impulse
        );
    }

    private void FixedUpdate()
    {
        if (playerCombat != null && playerCombat.IsDead)
            return;

        if (controlesBloqueados)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            TravarLimiteEsquerdoDoMapa();
            return;
        }

        if (rb != null)
            rb.linearVelocity = new Vector2(horizontalInput * velocidade, rb.linearVelocity.y);

        TravarLimiteEsquerdoDoMapa();
    }

    public void SetControlesBloqueados(bool bloquear)
    {
        controlesBloqueados = bloquear;

        if (bloquear)
        {
            horizontalInput = 0f;

            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (animator != null)
                animator.SetBool(movendoHash, false);
        }
    }

    private void GarantirPeDoPersonagem()
    {
        if (peDoPersonagem != null)
            return;

        Transform peEncontrado = transform.Find("PeDoPersonagem");

        if (peEncontrado != null)
        {
            peDoPersonagem = peEncontrado;
            return;
        }

        GameObject novoPe = new GameObject("PeDoPersonagem");
        novoPe.transform.SetParent(transform);
        novoPe.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        peDoPersonagem = novoPe.transform;

        Debug.LogWarning("PeDoPersonagem não estava atribuído. Um ponto de pé foi criado automaticamente em " + gameObject.name);
    }

    public bool EstaCaindo()
    {
        if (rb == null)
            return false;

        return rb.linearVelocity.y < -0.1f;
    }

    public float AlturaDoPe()
    {
        GarantirPeDoPersonagem();
        return peDoPersonagem.position.y;
    }

    public void QuicarAposPisar()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
    }

    void TentarUsarAcaoEspecial()
    {
        if (Time.timeScale == 0f || Time.time < proximaAcaoEspecialLiberada)
            return;

        if (GameManager.Instance == null || !GameManager.Instance.GastarEssencia(custoAcaoEspecial))
            return;

        proximaAcaoEspecialLiberada = Time.time + cooldownAcaoEspecial;

        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, impulsoAcaoEspecial);

        Collider2D[] alvos = Physics2D.OverlapCircleAll(transform.position, raioAcaoEspecial);

        foreach (Collider2D alvo in alvos)
        {
            MorcegoController morcego = alvo.GetComponent<MorcegoController>();

            if (morcego == null)
                morcego = alvo.GetComponentInParent<MorcegoController>();

            if (morcego != null)
                morcego.DerrotarPorAcaoEspecial();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioAcaoEspecial);
    }

    void TentarEncontrarTilemapLimiteEsquerdo()
    {
        GameObject tilemap = GameObject.Find(nomeTilemapLimiteEsquerdo);

        if (tilemap != null)
            tilemapLimiteEsquerdoRenderer = tilemap.GetComponent<Renderer>();
    }

    void TravarLimiteEsquerdoDoMapa()
    {
        if (tilemapLimiteEsquerdoRenderer == null)
            TentarEncontrarTilemapLimiteEsquerdo();

        if (tilemapLimiteEsquerdoRenderer == null)
            return;

        float metadeLarguraPersonagem = personagemCollider != null ? personagemCollider.bounds.extents.x : 0f;
        float limiteEsquerdo = tilemapLimiteEsquerdoRenderer.bounds.min.x + metadeLarguraPersonagem + margemLimiteEsquerdo;

        if (transform.position.x >= limiteEsquerdo)
            return;

        transform.position = new Vector2(limiteEsquerdo, transform.position.y);

        if (rb != null && rb.linearVelocity.x < 0f)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}
