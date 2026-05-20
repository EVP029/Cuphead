using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    [Range(0.1f, 0.9f)]
    public float controllerDeadzone = 0.2f;

    [Header("Jump Reworked")]
    public float jumpForce = 12f; 
    public float gravityMultiplierAscending = 2.0f;   // Reducido ligeramente para una subida más fluida
    public float gravityMultiplierDescending = 4.0f;  // Caída rápida y firme
    public float gravityMultiplierLowJump = 3.0f;     // Gravedad base de salto corto (amortiguada por código)

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

    [Header("Ground Check (BoxCast)")]
    public LayerMask groundLayer;
    public LayerMask platformLayer; 

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

    [Header("Health & Combat Reworked")]
    public int health = 3;
    public float damageJumpForce = 5f; 
    public float hitStunTime = 0.3f;
    public float fallBounceForce = 28f;
    public float invincibilityTime = 1.5f;

    [Header("Death")]
    public float deathDelay = 3f;
    public float ghostRiseSpeed = 5f; 
    
    [Header("Intro")]
    public bool isIntroPlaying = true; 

    [Header("Efectos de Audio (Player SFX)")]
    private AudioSource audioSource;
    public AudioClip sonidoHit;       // Sonido cuando el jugador recibe daño
    public AudioClip sonidoDash;      // Sonido cuando el jugador hace un dash
    public AudioClip sonidoMuerte;    // Sonido cuando la salud llega a cero
    [Range(0f, 1f)] public float volumenSFX = 0.8f;

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
    private bool isDroppingThroughPlatform = false; 

    private Coroutine currentDashCoroutine;
    private float defaultGravity;
    
    private float groundedTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;

        defaultGravity = rb.gravityScale;

        // CONFIGURACIÓN AUTOMÁTICA DEL AUDIOSOURCE
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 

        if (isIntroPlaying)
        {
            rb.simulated = false;
            SetCollider(introSize, introOffset);
        }
    }

    void Update()
    {
        if (isDead || isGettingHit || isIntroPlaying || isUsingSpecial) return;

        bool rawGrounded = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer | platformLayer) && rb.linearVelocity.y <= 0.05f;
        
        if (rawGrounded)
        {
            groundedTimer = 0.06f; 
            isGrounded = true;
        }
        else
        {
            groundedTimer -= Time.deltaTime;
            if (groundedTimer <= 0f)
            {
                isGrounded = false; 
            }
        }

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
            if (isDucking && !isDashing)
            {
                StartCoroutine(DropDownRoutine());
            }
            else if (isGrounded && !isDucking && !isDashing)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false; 
                groundedTimer = 0f;
            }
            else if (!isGrounded)
            {
                TryParry();
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (currentCards > 0 && !isUsingSpecial) 
            {
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
        FlipSpriteSprite();
    }

    void FixedUpdate()
    {
        if (isDead || isDashing || isGettingHit || isUsingSpecial) return;
        
        float targetSpeed = isDucking || (isLookingUp && isShooting && isGrounded) ? 0 : moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        if (!isGrounded)
        {
            if (isDroppingThroughPlatform || rb.linearVelocity.y < 0)
            {
                rb.gravityScale = defaultGravity * gravityMultiplierDescending;
            }
            else if (rb.linearVelocity.y > 0 && !(Input.GetKey(KeyCode.Space) || Input.GetButton("Jump")))
            {
                rb.gravityScale = defaultGravity * gravityMultiplierAscending;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.85f);
            }
            else
            {
                rb.gravityScale = defaultGravity * gravityMultiplierAscending;
            }
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }
    }

    IEnumerator LaunchSpecialRoutine()
    {
        isUsingSpecial = true;
        currentCards--;
        animator.SetBool("SpecialAttack", true);
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
        
        // Audio del Dash
        if (audioSource != null && sonidoDash != null) audioSource.PlayOneShot(sonidoDash, volumenSFX);

        rb.gravityScale = 0; 
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0); 
        yield return new WaitForSeconds(dashTime); 
        
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

        bool deVerdadEstaPisando = isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f;

        animator.SetFloat("Speed", isDucking ? 0 : (deVerdadEstaPisando ? Mathf.Abs(moveInput) : 0f)); 
        animator.SetBool("IsJumping", !deVerdadEstaPisando); 
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

    void FlipSpriteSprite() 
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
        
        // Audio del Daño
        if (audioSource != null && sonidoHit != null) audioSource.PlayOneShot(sonidoHit, volumenSFX);

        rb.linearVelocity = Vector2.zero; 
        
        if (applyKnockback) 
        {
            rb.AddForce(Vector2.up * damageJumpForce, ForceMode2D.Impulse); 
        }
        
        yield return new WaitForSeconds(hitStunTime); 
        isGettingHit = false; 
    }

    void Die() 
    { 
        if (isDead) return; 
        isDead = true; 

        // Audio de Muerte
        if (audioSource != null && sonidoMuerte != null) audioSource.PlayOneShot(sonidoMuerte, volumenSFX);

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

    IEnumerator DropDownRoutine()
    {
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int contactCount = playerCollider.GetContacts(contacts);

        Collider2D platformToIgnore = null;

        for (int i = 0; i < contactCount; i++)
        {
            if (contacts[i].collider.GetComponent<PlatformEffector2D>() != null || 
                contacts[i].collider.GetComponentInParent<PlatformEffector2D>() != null)
            {
                platformToIgnore = contacts[i].collider;
                break;
            }
        }

        if (platformToIgnore != null)
        {
            isDroppingThroughPlatform = true;
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -10f); 

            Physics2D.IgnoreCollision(playerCollider, platformToIgnore, true);

            yield return new WaitForSeconds(0.15f);

            Physics2D.IgnoreCollision(playerCollider, platformToIgnore, false);
            isDroppingThroughPlatform = false;
        }
    }
}