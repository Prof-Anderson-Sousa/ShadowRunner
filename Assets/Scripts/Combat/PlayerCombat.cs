using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(50)]
public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque Melee")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private KeyCode attackKey = KeyCode.J;
    [SerializeField] private bool usarAtaqueMelee = false;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1.5f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invulnerabilityDuration = 0.8f;
    [SerializeField] private float deathDelay = 1.2f;
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private float damageFlashInterval = 0.1f;

    [Header("Animator (Blind Huntress)")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string hurtTriggerName = "Hit";
    [SerializeField] private string deathTriggerName = "Die";

    private Rigidbody2D rb;
    private Animator animator;
    private AnimacaoPersonagemPorCodigo animacaoPorCodigo;
    private Color originalSpriteColor;
    private int currentHealth;
    private int facingDirection = 1;
    private float nextAttackTime;
    private float nextDashTime;
    private float invulnerableUntil;
    private float dashEndTime;
    private Coroutine invulnerabilityRoutine;

    private int attackTriggerHash;
    private int hurtTriggerHash;
    private int deathTriggerHash;

    public bool isDashing { get; private set; }
    public bool IsDead { get; private set; }
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    public bool IsInvulnerable => !IsDead && (isDashing || Time.time < invulnerableUntil);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animacaoPorCodigo = GetComponent<AnimacaoPersonagemPorCodigo>();
        CacheAttackPoint();
        CacheVisualRenderer();
        CacheAnimatorHashes();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    private void Update()
    {
        if (IsDead)
            return;

        UpdateFacingDirection();
        TryMeleeAttack();
        TryStartDash();

        if (isDashing && Time.time >= dashEndTime)
            isDashing = false;
    }

    private void FixedUpdate()
    {
        if (!isDashing || IsDead)
            return;

        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, rb.linearVelocity.y);
    }

    private void CacheVisualRenderer()
    {
        if (visualRenderer == null)
            visualRenderer = GetComponent<SpriteRenderer>();

        if (visualRenderer == null)
            visualRenderer = GetComponentInChildren<SpriteRenderer>();

        if (visualRenderer != null)
            originalSpriteColor = visualRenderer.color;
    }

    private void CacheAnimatorHashes()
    {
        attackTriggerHash = Animator.StringToHash(attackTriggerName);
        hurtTriggerHash = Animator.StringToHash(hurtTriggerName);
        deathTriggerHash = Animator.StringToHash(deathTriggerName);
    }

    private void CacheAttackPoint()
    {
        if (attackPoint != null)
            return;

        Transform pontoEncontrado = transform.Find("AttackPoint");

        if (pontoEncontrado != null)
            attackPoint = pontoEncontrado;
    }

    private void UpdateFacingDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal > 0f)
            facingDirection = 1;
        else if (horizontal < 0f)
            facingDirection = -1;
        else if (visualRenderer != null)
            facingDirection = visualRenderer.flipX ? -1 : 1;
    }

    private void TryMeleeAttack()
    {
        if (!usarAtaqueMelee)
            return;

        if (Time.time < nextAttackTime)
            return;

        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(attackKey))
            return;

        nextAttackTime = Time.time + attackCooldown;
        TriggerAnimator(attackTriggerHash, attackTriggerName);

        if (attackPoint == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            ShadowCreatureAI enemy = hit.GetComponent<ShadowCreatureAI>();

            if (enemy == null)
                enemy = hit.GetComponentInParent<ShadowCreatureAI>();

            if (enemy != null)
                enemy.TakeDamage(attackDamage);
        }
    }

    private void TryStartDash()
    {
        if (isDashing || Time.time < nextDashTime)
            return;

        if (!Input.GetKeyDown(dashKey))
            return;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, rb.linearVelocity.y);
    }

    public void TakeDamage(int damage)
    {
        if (IsInvulnerable || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        invulnerableUntil = Time.time + invulnerabilityDuration;

        if (animacaoPorCodigo != null)
            animacaoPorCodigo.TocarHit();
        else
            TriggerAnimator(hurtTriggerHash);
        StartInvulnerabilityFeedback();
        NotifyHealthChanged();

        if (currentHealth <= 0)
            BeginDeathSequence();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        NotifyHealthChanged();
    }

    private void BeginDeathSequence()
    {
        if (IsDead)
            return;

        IsDead = true;
        isDashing = false;

        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(invulnerabilityRoutine);
            invulnerabilityRoutine = null;
        }

        RestoreSpriteColor();
        if (animacaoPorCodigo != null)
            animacaoPorCodigo.TocarMorte();
        else
            TriggerAnimator(deathTriggerHash);

        foreach (Collider2D collider in GetComponents<Collider2D>())
            collider.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathDelay);

        if (GameManager.Instance != null)
            GameManager.Instance.EncerrarJogo();
    }

    private void StartInvulnerabilityFeedback()
    {
        if (invulnerabilityRoutine != null)
            StopCoroutine(invulnerabilityRoutine);

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityFeedbackRoutine());
    }

    private IEnumerator InvulnerabilityFeedbackRoutine()
    {
        if (visualRenderer == null)
            yield break;

        while (Time.time < invulnerableUntil && !IsDead)
        {
            visualRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashInterval);
            visualRenderer.color = originalSpriteColor;
            yield return new WaitForSeconds(damageFlashInterval);
        }

        invulnerabilityRoutine = null;
        RestoreSpriteColor();
    }

    private void RestoreSpriteColor()
    {
        if (visualRenderer != null)
            visualRenderer.color = originalSpriteColor;
    }

    private void TriggerAnimator(int triggerHash, string fallbackStateName = "")
    {
        if (animator == null)
            return;

        if (HasAnimatorParameter(triggerHash))
        {
            animator.SetTrigger(triggerHash);
            return;
        }

        TryCrossFadeState(fallbackStateName, 0f);
    }

    private bool HasAnimatorParameter(int parameterHash)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == parameterHash)
                return true;
        }

        return false;
    }

    private bool TryCrossFadeState(string stateName, float transitionDuration)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return false;

        int stateHash = Animator.StringToHash(stateName);
        int baseLayerStateHash = Animator.StringToHash("Base Layer." + stateName);

        if (animator.HasState(0, stateHash))
        {
            animator.CrossFade(stateHash, transitionDuration, 0);
            return true;
        }

        if (animator.HasState(0, baseLayerStateHash))
        {
            animator.CrossFade(baseLayerStateHash, transitionDuration, 0);
            return true;
        }

        return false;
    }

    private void NotifyHealthChanged()
    {
        GameManager.Instance?.AtualizarVidaJogador(currentHealth, maxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
