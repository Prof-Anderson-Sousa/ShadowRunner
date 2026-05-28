using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimentação Base")]
    public float speed = 8f;
    private float moveInput;
    private float lastDirection = 1f;

    [Header("Mecânica de Pulo")]
    public float jumpForce = 14f;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    public float checkRadius = 0.2f;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Detecção de Parede")]
    public float wallCheckDistance = 0.15f;

    [Header("Mecânica de Dash")]
    public float dashPower = 20f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("Efeitos Visuais")]
    public GameObject dustPrefab;
    public GameObject jumpDustPrefab; 
    public Vector3 dustOffset; 

    [Header("Mecânica de Combate")]
    public GameObject[] weaponPrefabs;
    public int currentWeaponIndex = 0;
    public Transform firePoint;
    public float throwCooldown = 0.5f;
    private float nextThrowTime = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (isDashing) return;

        // Movimentação horizontal
        moveInput = Input.GetAxisRaw("Horizontal");
        //rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (moveInput != 0) 
        {
            lastDirection = moveInput;
        }

        // Atualização do Animator do Player
        anim.SetFloat("DirectionX", lastDirection);
        anim.SetBool("IsMoving", moveInput != 0);
        
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("VelocityY", rb.linearVelocity.y);

        // Comando de pulo
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (jumpDustPrefab != null)
            {
                Instantiate(jumpDustPrefab, groundCheck.position + dustOffset, Quaternion.identity);
            }
        }

        // Pouso
        if (!wasGrounded && isGrounded)
        {
            if (jumpDustPrefab != null)
            {
                Instantiate(jumpDustPrefab, groundCheck.position + dustOffset, Quaternion.identity);
            }
        }
        wasGrounded = isGrounded;

        // Comando de Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(DashImpulse());
        }

        // Troca de Arma (Tecla Q)
        if (Input.GetKeyDown(KeyCode.Q) && weaponPrefabs.Length > 0)
        {
            currentWeaponIndex++;
            
            if (currentWeaponIndex >= weaponPrefabs.Length)
            {
                currentWeaponIndex = 0;
            }
            Debug.Log("Arma equipada: " + currentWeaponIndex);
        }

        // Arremesso (Tecla Z)
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextThrowTime)
        {
            if (weaponPrefabs.Length > 0 && weaponPrefabs[currentWeaponIndex] != null && firePoint != null)
            {
                anim.SetTrigger("ThrowTrigger");
                nextThrowTime = Time.time + throwCooldown;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        float velX = moveInput * speed;

        if (!isGrounded && velX != 0f)
        {
            Vector2 direcao = velX > 0 ? Vector2.right : Vector2.left;
            float meiaLargura = col != null ? col.bounds.extents.x : 0.2f;
            bool paredeNaFrente = Physics2D.Raycast(transform.position, direcao, meiaLargura + wallCheckDistance, whatIsGround);
            if (paredeNaFrente)
                velX = 0f;
        }

        rb.linearVelocity = new Vector2(velX, rb.linearVelocity.y);
    }

    // Função chamada pelo Animation Event no frame exato do arremesso
    public void PerformThrowInstance()
    {
        if (weaponPrefabs.Length > 0 && weaponPrefabs[currentWeaponIndex] != null && firePoint != null)
        {
            GameObject weapon = Instantiate(weaponPrefabs[currentWeaponIndex], firePoint.position, Quaternion.identity);
            
            ThrownWeapon script = weapon.GetComponent<ThrownWeapon>();
            if (script != null)
            {
                script.Initialize(lastDirection);
            }
        }
    }

    private IEnumerator DashImpulse()
    {
        canDash = false;
        isDashing = true;
        
        float gravityBefore = rb.gravityScale;
        rb.gravityScale = 0f;

        anim.SetFloat("DirectionX", lastDirection);
        anim.SetTrigger("DashTrigger");

        rb.linearVelocity = new Vector2(dashPower * lastDirection, 0f);

        if (dustPrefab != null)
        {
            GameObject dust = Instantiate(dustPrefab, groundCheck.position + dustOffset, Quaternion.identity);
            Animator dustAnim = dust.GetComponent<Animator>();
            
            if (dustAnim != null)
            {
                dustAnim.SetFloat("DirectionX", lastDirection);
            }
        }

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = gravityBefore;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}