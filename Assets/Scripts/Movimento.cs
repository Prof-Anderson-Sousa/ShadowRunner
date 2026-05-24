using UnityEngine;

public class Movimento : MonoBehaviour
{
    private float horizontalInput;
    private Rigidbody2D rb;
    [SerializeField] private int velocidade = 5;
    [SerializeField] private Transform peDoPersonagem;
    [SerializeField] private LayerMask chaoLayer;
    [SerializeField] private float limiteEsquerdo = -8f;
    [SerializeField] private float limiteDireito = 8f;
    private bool estaNoChao;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerCombat playerCombat;

    [Header("Acao Especial")]
    [SerializeField] private KeyCode teclaAcaoEspecial = KeyCode.E;
    [SerializeField] private int custoAcaoEspecial = 3;
    [SerializeField] private float raioAcaoEspecial = 2.4f;
    [SerializeField] private float impulsoAcaoEspecial = 8f;
    [SerializeField] private float cooldownAcaoEspecial = 1.2f;

    private int movendoHash = Animator.StringToHash("movendo");
    private int saltandoHash = Animator.StringToHash("saltando");
    private float proximaAcaoEspecialLiberada;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        if (playerCombat != null && playerCombat.IsDead)
            return;

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
        animator.SetBool(movendoHash, horizontalInput != 0);
        animator.SetBool(saltandoHash, !estaNoChao);

        if (horizontalInput > 0)
            spriteRenderer.flipX = false;
        else if (horizontalInput < 0)
            spriteRenderer.flipX = true;

        float x = Mathf.Clamp(transform.position.x, limiteEsquerdo, limiteDireito);
        transform.position = new Vector2(x, transform.position.y);
    }

    private void FixedUpdate()
    {
        if (playerCombat != null && playerCombat.IsDead)
            return;

        rb.linearVelocity = new Vector2(horizontalInput * velocidade, rb.linearVelocity.y);
    }

    public bool EstaCaindo()
    {
        return rb.linearVelocity.y < -0.1f;
    }

    public float AlturaDoPe()
    {
        return peDoPersonagem.position.y;
    }

    public void QuicarAposPisar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
    }

    void TentarUsarAcaoEspecial()
    {
        if (Time.timeScale == 0f || Time.time < proximaAcaoEspecialLiberada)
            return;

        if (GameManager.Instance == null || !GameManager.Instance.GastarEssencia(custoAcaoEspecial))
            return;

        proximaAcaoEspecialLiberada = Time.time + cooldownAcaoEspecial;
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
}
