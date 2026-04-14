using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Better Jump")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.35f;
    public LayerMask groundLayer;

    [Header("Damage")]
    public int health = 3;
    public float damageBounceForce = 18f;
    public float fallBounceForce = 25f;
    public float invincibilityTime = 1f;

    [Header("Death")]
    public float deathDelay = 3f;
    public float ghostRiseSpeed = 0.6f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool isGrounded;
    private bool isDashing;
    private bool isDucking;
    private bool isInvincible;
    private bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
    }
    void Update()
    {
        if (isDead) return;

        // Ground check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Duck
        isDucking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        animator.SetBool("IsDucking", isDucking);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isDucking && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && !isDucking)
        {
            StartCoroutine(Dash());
        }

        // Animator
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsJumping", !isGrounded);

        // Flip
        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isDashing && !isDucking)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        // Better jump
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                           (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                           (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        animator.SetTrigger("IsDashing");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float dir = transform.localScale.x;
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FallZone"))
        {
            FallDamage();
        }
    }

    void FallDamage()
    {
        TakeDamage();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * fallBounceForce, ForceMode2D.Impulse);
    }

    void TakeDamage()
    {
        if (isInvincible || isDead) return;

        health--;

        animator.SetTrigger("IsHit");

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * damageBounceForce, ForceMode2D.Impulse);

        StartCoroutine(Invincibility());

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator Invincibility()
    {
        isInvincible = true;

        float timer = 0;

        while (timer < invincibilityTime)
        {
            Color c = spriteRenderer.color;
            c.a = 0.6f;
            spriteRenderer.color = c;

            yield return new WaitForSeconds(0.1f);

            c.a = 1f;
            spriteRenderer.color = c;

            yield return new WaitForSeconds(0.1f);

            timer += 0.2f;
        }

        Color reset = spriteRenderer.color;
        reset.a = 1f;
        spriteRenderer.color = reset;

        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        animator.SetTrigger("IsDead");

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        StartCoroutine(GhostRise());
    }

    IEnumerator GhostRise()
    {
        float timer = 0;

        while (timer < deathDelay)
        {
            transform.position += Vector3.up * ghostRiseSpeed * Time.deltaTime;

            timer += Time.deltaTime;
            yield return null;
        }

        EndLevel();
    }

    void EndLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}