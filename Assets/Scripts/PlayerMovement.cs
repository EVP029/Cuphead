using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // ... (Variables de Movimiento, Salto, Dash se mantienen igual)
    [Header("Movement")]
    public float moveSpeed = 8f;
    [Header("Jump")]
    public float jumpForce = 11f; 
    public float fallMultiplier = 4f; 
    public float lowJumpMultiplier = 2.2f; 
    [Header("Dash")]
    public float dashSpeed = 22f;
    public float dashTime = 0.25f;
    private bool canDash = true;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.15f;
    private float nextFireTime = 0f;
    [Tooltip("Posición del disparo cuando está de pie")]
    public Vector3 shootOffsetStanding = new Vector3(0.8f, 1.3f, 0f);
    [Tooltip("Posición del disparo cuando está agachado")]
    public Vector3 shootOffsetDucking = new Vector3(0.9f, 0.6f, 0f);

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Collider Profiles")]
    public Vector2 standingSize = new Vector2(0.8f, 1.8f);
    public Vector2 standingOffset = new Vector2(0f, 0.9f);
    public Vector2 duckingSize = new Vector2(0.8f, 1.0f);
    public Vector2 duckingOffset = new Vector2(0f, 0.5f);
    public Vector2 jumpingSize = new Vector2(0.7f, 1.4f);
    public Vector2 jumpingOffset = new Vector2(0f, 1.1f);
    public Vector2 dashingSize = new Vector2(1.5f, 0.6f);
    public Vector2 dashingOffset = new Vector2(0f, 0.8f);

    [Header("Health & Combat")]
    public int health = 3;
    public float damageBounceForce = 10f;
    public float hitStunTime = 0.4f;
    public float fallBounceForce = 28f;
    public float invincibilityTime = 1.5f;

    [Header("Death")]
    public float deathDelay = 3f;
    public float ghostRiseSpeed = 5f; 

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D playerCollider;

    private float moveInput;
    private bool isGrounded;
    private bool isDashing;
    private bool isDucking;
    private bool isInvincible;
    private bool isDead;
    private bool isGettingHit;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (isDead || isGettingHit) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && !isDashing) canDash = true;

        moveInput = Input.GetAxisRaw("Horizontal");
        isDucking = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && isGrounded;

        UpdateColliderState();
        UpdateFirePointPosition(); // <--- REFINAMIENTO: Mueve el cañón según la pose

        if (Input.GetButtonDown("Jump") && isGrounded && !isDucking && !isDashing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && canDash)
            StartCoroutine(DashRoutine());

        if (Input.GetKey(KeyCode.X)) 
        {
            Shoot();
            animator.SetBool("IsShooting", true);
        } 
        else 
        {
            animator.SetBool("IsShooting", false);
        }

        if (!isDashing) UpdateAnimations();

        FlipSprite();
    }

    void FixedUpdate()
    {
        if (isDead || isDashing || isGettingHit) return;
        float targetSpeed = isDucking ? 0 : moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    // Nueva función para mover el punto de disparo
    void UpdateFirePointPosition()
    {
        if (isDucking)
        {
            firePoint.localPosition = shootOffsetDucking;
        }
        else
        {
            firePoint.localPosition = shootOffsetStanding;
        }
    }

    void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Physics2D.IgnoreCollision(bulletObj.GetComponent<Collider2D>(), playerCollider);
            
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.SetDirection(transform.localScale.x);
            
            animator.SetTrigger("ShootTrigger"); 
            nextFireTime = Time.time + fireRate;
        }
    }

    void UpdateAnimations()
    {
        if (isDead || isGettingHit || isDashing) return;

        float animSpeed = isDucking ? 0 : Mathf.Abs(moveInput);
        animator.SetFloat("Speed", animSpeed);
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsDucking", isDucking);
    }

    // ... (Mantén UpdateColliderState, FlipSprite, DashRoutine, TakeDamage, HitRoutine, Die, GhostRise, InvincibilityRoutine, OnTriggerEnter2D y HandleFall igual que antes)

    void UpdateColliderState()
    {
        if (isDashing) SetCollider(dashingSize, dashingOffset);
        else if (isDucking) SetCollider(duckingSize, duckingOffset);
        else if (!isGrounded) SetCollider(jumpingSize, jumpingOffset);
        else SetCollider(standingSize, standingOffset);
    }

    void SetCollider(Vector2 size, Vector2 offset)
    {
        playerCollider.size = size;
        playerCollider.offset = offset;
    }

    void FlipSprite()
    {
        if (isDashing || isDead || isGettingHit) return; 
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        if (!isGrounded) canDash = false;
        animator.SetBool("IsJumping", false);
        animator.Play("Dash", 0, 0f); 
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0; 
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        if (isDead || isGettingHit) yield break;
        if (!isGrounded) animator.Play("Jump", 0, 0f); 
    }

    public void TakeDamage(bool applyKnockback = true)
    {
        if (isInvincible || isDead || isGettingHit) return; 
        health--;
        if (health <= 0) Die();
        else 
        {
            StartCoroutine(HitRoutine(applyKnockback));
            StartCoroutine(InvincibilityRoutine());
        }
    }

    IEnumerator HitRoutine(bool applyKnockback)
    {
        isGettingHit = true;
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsDucking", false);
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("IsHit"); 
        rb.linearVelocity = Vector2.zero;
        if (applyKnockback) rb.AddForce(new Vector2(-transform.localScale.x * 5f, damageBounceForce), ForceMode2D.Impulse);
        yield return new WaitForSeconds(hitStunTime); 
        isGettingHit = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsDucking", false);
        animator.SetFloat("Speed", 0);
        animator.Play("Death", 0, 0f); 
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; 
        spriteRenderer.sortingOrder = 100;
        StartCoroutine(GhostRise());
    }

    IEnumerator GhostRise()
    {
        yield return new WaitForSeconds(0.1f);
        animator.Play("Death", 0, 0f); 
        float timer = 0;
        while (timer < deathDelay)
        {
            transform.position += Vector3.up * ghostRiseSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0;
        Color originalColor = spriteRenderer.color;
        while (elapsed < invincibilityTime)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
            yield return new WaitForSeconds(0.08f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.08f);
            elapsed += 0.16f;
        }
        spriteRenderer.color = originalColor;
        isInvincible = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FallZone")) HandleFall();
        if (collision.CompareTag("Enemy")) TakeDamage();
    }

    void HandleFall()
    {
        if (isDead || isGettingHit || isInvincible) return;
        TakeDamage(false); 
        if (!isDead) rb.linearVelocity = new Vector2(0, fallBounceForce); 
    }
}