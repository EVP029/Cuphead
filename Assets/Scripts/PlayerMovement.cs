using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    [Range(0.1f, 0.9f)]
    public float controllerDeadzone = 0.2f;

    [Header("Jump")]
    public float jumpForce = 11f; 
    public float fallMultiplier = 4f; 
    public float lowJumpMultiplier = 2.2f; 

    [Header("Dash")]
    public float dashSpeed = 18f;     
    public float dashTime = 0.5f;      
    private bool canDash = true;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.15f;
    private float nextFireTime = 0f;
    public Vector3 shootOffsetStanding = new Vector3(0.8f, 1.3f, 0f);
    public Vector3 shootOffsetDucking = new Vector3(0.9f, 0.6f, 0f);
    public Vector3 shootOffsetUp = new Vector3(0.2f, 2.0f, 0f); 
    public Vector3 shootOffsetRunning = new Vector3(0.8f, 1.1f, 0f); 

    [Header("Energy & Special")]
    public GameObject specialBulletPrefab;
    public int currentCards = 0;
    public int maxCards = 5;
    public float specialFreezeTime = 0.5f; 
    public float specialShotDelay = 0.15f; 
    private float energyProgress = 0f;
    private bool isUsingSpecial = false; 

    [Header("Parry System")]
    public float parryRadius = 0.6f;
    public LayerMask parryLayer;
    public float parryBounceForce = 12f;
    private bool canParry = true;

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
    public Vector2 introSize = new Vector2(0.8f, 1.8f); 
    public Vector2 introOffset = new Vector2(0f, 0.9f); 

    [Header("Health & Combat")]
    public int health = 3;
    public float damageBounceForce = 10f;
    public float hitStunTime = 0.4f;
    public float fallBounceForce = 28f;
    public float invincibilityTime = 1.5f;

    [Header("Death")]
    public float deathDelay = 3f;
    public float ghostRiseSpeed = 5f; 
    
    [Header("Intro")]
    public bool isIntroPlaying = true; 

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D playerCollider;

    private float moveInput;
    private bool isGrounded;
    private bool isDashing;
    private bool isDucking;
    private bool isLookingUp; 
    private bool isShooting; 
    private bool isInvincible;
    private bool isDead;
    private bool isGettingHit;

    // --- NUEVAS VARIABLES PARA ARREGLAR LA GRAVEDAD DEL DASH ---
    private Coroutine currentDashCoroutine;
    private float defaultGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;

        // GUARDAMOS LA GRAVEDAD BASE AL INICIAR
        defaultGravity = rb.gravityScale;

        if (isIntroPlaying)
        {
            rb.simulated = false;
            SetCollider(introSize, introOffset);
        }
    }

    void Update()
    {
        if (isDead || isGettingHit || isIntroPlaying || isUsingSpecial) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && !isDashing)
        {
            canDash = true;
            canParry = true; 
        }

        float h = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1;
        
        float joyH = Input.GetAxisRaw("Horizontal"); 
        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(joyH) > controllerDeadzone) h = joyH;
        moveInput = h;

        float rtValue = 0;
        try { rtValue = Input.GetAxisRaw("Axis 6"); } catch { }

        isShooting = rtValue > 0.3f || Input.GetKey(KeyCode.X);

        float v = Input.GetAxisRaw("Vertical");
        isDucking = (v < -controllerDeadzone || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && isGrounded;
        
        bool pressingUp = (v > controllerDeadzone || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));

        if (Mathf.Abs(moveInput) > 0.1f && isShooting) isLookingUp = false;
        else isLookingUp = pressingUp && !isDucking;

        UpdateColliderState();
        UpdateFirePointPosition();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 0"))
        {
            if (isGrounded && !isDucking && !isDashing)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            else if (!isGrounded)
                TryParry();
        }

        // --- BOTÓN B CON DASH-CANCEL ---
        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (currentCards > 0 && !isUsingSpecial) 
            {
                // Si estábamos dasheando, lo cancelamos
                if (isDashing && currentDashCoroutine != null)
                {
                    StopCoroutine(currentDashCoroutine);
                    isDashing = false;
                }
                StartCoroutine(LaunchSpecialRoutine());
            }
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown("joystick button 4")) && !isDashing && canDash)
        {
            // Guardamos la referencia de la corrutina al iniciarla
            currentDashCoroutine = StartCoroutine(DashRoutine());
        }

        if (isShooting) 
        {
            Shoot();
            animator.SetBool("IsShooting", true);
        } 
        else 
        {
            animator.SetBool("IsShooting", false);
            animator.ResetTrigger("ShootTrigger"); 
        }

        if (!isDashing) UpdateAnimations();
        FlipSprite();
    }

    void FixedUpdate()
    {
        if (isDead || isDashing || isGettingHit || isUsingSpecial) return;
        
        bool lockMovement = isDucking || (isLookingUp && isShooting && isGrounded);
        float targetSpeed = lockMovement ? 0 : moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    // --- CORRUTINA PARA ATAQUE ESPECIAL ---
    IEnumerator LaunchSpecialRoutine()
    {
        isUsingSpecial = true;
        currentCards--;
        
        animator.SetBool("SpecialAttack", true);
        
        // Usamos la gravedad original sin crear una variable local
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero; 

        animator.Play("SpecialAttack", 0, 0f); 

        yield return new WaitForSeconds(specialShotDelay);

        if (specialBulletPrefab != null)
        {
            GameObject specialObj = Instantiate(specialBulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletScript = specialObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.isSpecial = true;
                bulletScript.SetDirection(transform.localScale.x);
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0, specialFreezeTime - specialShotDelay));

        animator.SetBool("SpecialAttack", false);

        // Restauramos usando la gravedad por defecto siempre
        rb.gravityScale = defaultGravity;
        isUsingSpecial = false;

        if (!isGrounded) animator.Play("Jump", 0, 0f);
    }

    public void AddEnergy(float amount)
    {
        if (currentCards >= maxCards) return;
        energyProgress += amount;
        if (energyProgress >= 1f)
        {
            energyProgress -= 1f;
            currentCards++;
        }
    }

    void TryParry()
    {
        if (!canParry) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, parryRadius, parryLayer);
        if (hit != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, parryBounceForce);
            animator.SetTrigger("ParryTrigger");
            if (currentCards < maxCards) currentCards++;
            canParry = false;
        }
    }

    public void FinishIntro()
    {
        isIntroPlaying = false;
        rb.simulated = true;
        SetCollider(standingSize, standingOffset);
    }

    IEnumerator DashRoutine() 
    { 
        isDashing = true; 
        if (!isGrounded) canDash = false; 
        animator.SetBool("IsJumping", false); 
        animator.Play("Dash", 0, 0f); 
        
        // Aplicamos gravedad cero directo
        rb.gravityScale = 0; 
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0); 
        yield return new WaitForSeconds(dashTime); 
        
        // Restauramos a la gravedad por defecto guardada al inicio
        rb.gravityScale = defaultGravity; 
        isDashing = false; 
        if (isDead || isGettingHit) yield break; 
        if (!isGrounded) animator.Play("Jump", 0, 0f); 
    }

    void UpdateFirePointPosition() 
    { 
        if (isDucking) 
        { 
            firePoint.localPosition = shootOffsetDucking; 
            firePoint.localEulerAngles = Vector3.zero; 
        } 
        else if (isLookingUp) 
        { 
            firePoint.localPosition = shootOffsetUp; 
            float rotationZ = 90f * transform.localScale.x;
            firePoint.localEulerAngles = new Vector3(0, 0, rotationZ); 
        } 
        else if (Mathf.Abs(moveInput) > 0.1f && isGrounded)
        {
            firePoint.localPosition = shootOffsetRunning;
            firePoint.localEulerAngles = Vector3.zero;
        }
        else 
        { 
            firePoint.localPosition = shootOffsetStanding; 
            firePoint.localEulerAngles = Vector3.zero; 
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
        animator.SetFloat("Speed", isDucking ? 0 : Mathf.Abs(moveInput)); 
        animator.SetBool("IsJumping", !isGrounded); 
        animator.SetBool("IsDucking", isDucking); 
        animator.SetBool("IsLookingUp", isLookingUp); 
    }
    
    void UpdateColliderState()
    {
        if (isIntroPlaying) return; 
        if (isDashing) SetCollider(dashingSize, dashingOffset);
        else if (isDucking) SetCollider(duckingSize, duckingOffset);
        else if (!isGrounded) SetCollider(jumpingSize, jumpingOffset);
        else SetCollider(standingSize, standingOffset);
    }

    void SetCollider(Vector2 size, Vector2 offset) { playerCollider.size = size; playerCollider.offset = offset; }

    void FlipSprite() 
    { 
        if (isDashing || isDead || isGettingHit) return; 
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1); 
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1); 
    }

    public void TakeDamage(bool applyKnockback = true) 
    { 
        if (isInvincible || isDead || isGettingHit) return; 
        health--; 
        if (health <= 0) Die(); 
        else { StartCoroutine(HitRoutine(applyKnockback)); StartCoroutine(InvincibilityRoutine()); } 
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