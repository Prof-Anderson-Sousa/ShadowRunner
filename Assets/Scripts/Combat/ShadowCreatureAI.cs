using System.Collections;
using UnityEngine;

public class ShadowCreatureAI : MonoBehaviour
{
    private enum CreatureState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;

    [Header("Movimento")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float patrolDistance = 3f;

    [Header("Detecção e Ataque")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float telegraphDuration = 0.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private string deathAnimationTrigger = "Die";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;
    private Color originalColor;

    private CreatureState currentState = CreatureState.Patrol;
    private int patrolDirection = 1;
    private float patrolOriginX;
    private int currentHealth;
    private float nextAttackTime;
    private bool isDead;
    private bool isAttacking;
    private Coroutine attackRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        patrolOriginX = transform.position.x;
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        CachePlayerReference();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null)
            CachePlayerReference();

        if (isAttacking)
            return;

        switch (currentState)
        {
            case CreatureState.Patrol:
                UpdatePatrolState();
                break;
            case CreatureState.Chase:
                UpdateChaseState();
                break;
            case CreatureState.Attack:
                UpdateAttackState();
                break;
        }

        UpdateSpriteFacing();
        UpdateAnimationStates(); 
    }

    private void FixedUpdate()
    {
        if (isDead || isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float speed = currentState == CreatureState.Chase ? chaseSpeed : patrolSpeed;
        float direction = currentState == CreatureState.Chase ? GetDirectionToPlayer() : patrolDirection;

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    private void CachePlayerReference()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void UpdatePatrolState()
    {
        float offsetFromOrigin = transform.position.x - patrolOriginX;

        if (offsetFromOrigin >= patrolDistance)
            patrolDirection = -1;
        else if (offsetFromOrigin <= -patrolDistance)
            patrolDirection = 1;

        if (player != null && GetDistanceToPlayer() <= detectionRadius)
            currentState = CreatureState.Chase;
    }

    private void UpdateChaseState()
    {
        if (player == null)
        {
            currentState = CreatureState.Patrol;
            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();

        if (distanceToPlayer > detectionRadius)
        {
            currentState = CreatureState.Patrol;
            return;
        }

        if (distanceToPlayer <= attackRadius && Time.time >= nextAttackTime)
            attackRoutine = StartCoroutine(AttackRoutine());
    }

    private void UpdateAttackState()
    {
        if (!isAttacking)
            currentState = player != null && GetDistanceToPlayer() <= detectionRadius
                ? CreatureState.Chase
                : CreatureState.Patrol;
    }

    private void UpdateAnimationStates()
    {
        if (animator != null)
        {
            // Se o inimigo estiver a mover-se horizontalmente, ativa a animação de Walk
            bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            animator.SetBool("IsWalking", isMoving);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = CreatureState.Attack;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // DISPARA O TRIGGER DE ATAQUE NO ANIMATOR
        if (animator != null)
            animator.SetTrigger("Attack");

        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(telegraphDuration);

        if (!isDead && player != null && GetDistanceToPlayer() <= attackRadius)
            ApplyDamageToPlayer();

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
        attackRoutine = null;

        currentState = player != null && GetDistanceToPlayer() <= detectionRadius
            ? CreatureState.Chase
            : CreatureState.Patrol;
    }

    private void ApplyDamageToPlayer()
    {
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();

        if (playerCombat == null)
            playerCombat = player.GetComponentInParent<PlayerCombat>();

        if (playerCombat != null)
            playerCombat.TakeDamage(attackDamage);
    }

    private float GetDistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    private int GetDirectionToPlayer()
    {
        return player.position.x >= transform.position.x ? 1 : -1;
    }

    private void UpdateSpriteFacing()
    {
        if (spriteRenderer == null || isAttacking)
            return;

        if (currentState == CreatureState.Chase && player != null)
            spriteRenderer.flipX = player.position.x < transform.position.x;
        else
            spriteRenderer.flipX = patrolDirection < 0;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
        currentState = CreatureState.Patrol;

        foreach (Collider2D collider in GetComponents<Collider2D>())
            collider.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (animator != null && !string.IsNullOrEmpty(deathAnimationTrigger))
            animator.SetTrigger(deathAnimationTrigger);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}