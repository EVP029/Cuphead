using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
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
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    [Header("Health")]
    public int health = 3;
    public float fallBounceForce = 28f;
    public float invincibilityTime = 1.5f;
    [Header("Death")]
    public float deathDelay = 3f;
    public float ghostRiseSpeed = 8f; // Lo subí a 8 por defecto

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float moveInput;
    private bool isGrounded;
    private bool isDashing;
    private bool isDucking;
    private bool isInvincible;
    private bool isDead;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
    }

    void Update() {
        if (isDead) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && !isDashing) canDash = true;
        moveInput = Input.GetAxisRaw("Horizontal");
        isDucking = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && isGrounded;
        if (Input.GetButtonDown("Jump") && isGrounded && !isDucking && !isDashing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && canDash)
            StartCoroutine(DashRoutine());
        if (!isDashing) UpdateAnimations();
        FlipSprite();
    }

    void FixedUpdate() {
        if (isDead || isDashing) return;
        float targetSpeed = isDucking ? 0 : moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    void UpdateAnimations() {
        animator.SetFloat("Speed", isDucking ? 0 : Mathf.Abs(moveInput));
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsDucking", isDucking);
    }

    void FlipSprite() {
        if (isDashing || isDead) return; 
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    IEnumerator DashRoutine() {
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
        if (!isGrounded) {
            animator.SetBool("IsJumping", true);
            animator.Play("Jump", 0, 0f); 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("FallZone")) HandleFall();
    }

    void HandleFall() {
        if (isDead || isInvincible) return;
        TakeDamage();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * fallBounceForce, ForceMode2D.Impulse);
    }

    public void TakeDamage() {
        if (isInvincible || isDead) return;
        health--;
        if (health <= 0) Die();
        else {
            animator.SetTrigger("IsHit");
            StartCoroutine(InvincibilityRoutine());
        }
    }

    void Die() {
        if (isDead) return;
        isDead = true;
        
        // Limpiar físicas
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; 

        // Forzar animación Death
        animator.SetBool("IsJumping", false);
        animator.Play("Death", 0, 0f); 

        spriteRenderer.sortingOrder = 100;
        StartCoroutine(GhostRise());
    }

    IEnumerator GhostRise() {
        yield return new WaitForSeconds(0.1f);
        animator.Play("Death", 0, 0f); // Re-forzado
        float timer = 0;
        while (timer < deathDelay) {
            transform.position += Vector3.up * ghostRiseSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator InvincibilityRoutine() {
        isInvincible = true;
        float elapsed = 0;
        Color originalColor = spriteRenderer.color;
        Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        while (elapsed < invincibilityTime) {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }
        spriteRenderer.color = originalColor;
        isInvincible = false;
    }
}