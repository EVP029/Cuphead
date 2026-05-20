using UnityEngine;
using System.Collections; // <--- Esta línea es la que le enseña a Unity qué es un 'IEnumerator'

public class BossCagney : MonoBehaviour
{
    // MODIFICADO: Se añade el estado PlatformVinesAttack al enum
    public enum BossState { Intro, Idle, LungeAttack, GatlingAttack, FloatingAttack, PlatformVinesAttack, Transition, Phase2, Dead }
    [Header("Exit Management")]
public LevelExitManager levelExitManager; // Arrastra aquí el objeto vacío que creamos
    [Header("Health & Phases")]
    public int health = 1000;
    public int phase2Threshold = 400; 
    
    [Header("State")]
    public BossState currentState;
    private bool isAttacking = false;
    private int lastAttackChoice = -1; 

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine; 

    [Header("Configuración del Parpadeo de Daño")]
    public float duracionParpadeo = 0.04f; 
    public Color colorParpadeo = Color.red; 

    [Header("Phase 2 Settings")]
    public GameObject sporePrefab; 
    public Transform sporeSpitPoint; 
    public float phase2AttackDelay = 2f; 
    public Animator vinesAnimator; 
    public Collider2D floorVinesCollider; 
    private bool isPhase2Active = false;

    [Header("Lunge Attack Settings")]
    public float lungeTotalDuration = 2.5f; 
    public GameObject lungeHighHitbox; 
    public GameObject lungeLowHitbox;  
    public GameObject cabezaWeakSpot;   
    private bool isLungeHigh = false;

    private Collider2D lungeHighCollider;
    private Collider2D lungeLowCollider;
    private Collider2D cabezaCollider;

    [Header("Platform Vines Attack Settings")]
    [Tooltip("Arrastra aquí los scripts VinesPlatformAttack asignados a tus plataformas")]
    public VinesPlatformAttack[] vainasPlataformas; 
    public float tiempoAlertaVaina = 1.0f; 
    public float tiempoDanoVaina = 1.5f;   
    public AudioClip sonidoAlertaVainas;   

    [Header("Efectos de Sonido (Cagney SFX)")]
    private AudioSource audioSource;
    public AudioClip sonidoIntro;         
    public AudioClip sonidoLunge;       
    public AudioClip sonidoGatling;     
    public AudioClip sonidoSporePhase2; 
    public AudioClip sonidoCambioFase;    
    public AudioClip sonidoKnockout;     
    public AudioClip sonidoMuerteLlanto;  
    [Range(0f, 1f)] public float volumenSFX = 0.8f;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 
        
        if (lungeHighHitbox != null) lungeHighCollider = lungeHighHitbox.GetComponent<Collider2D>();
        if (lungeLowHitbox != null) lungeLowCollider = lungeLowHitbox.GetComponent<Collider2D>();
        if (cabezaWeakSpot != null) cabezaCollider = cabezaWeakSpot.GetComponent<Collider2D>();

        if (lungeHighCollider != null) lungeHighCollider.enabled = false;
        if (lungeLowCollider != null) lungeLowCollider.enabled = false;
        if (cabezaCollider != null) cabezaCollider.enabled = true;

        if (floorVinesCollider != null) floorVinesCollider.enabled = false;

        currentState = BossState.Intro;
        StartCoroutine(BossLogicRoutine());
    }

    IEnumerator BossLogicRoutine()
    {
        currentState = BossState.Intro;
        animator.Play("Intro"); 

        if (audioSource != null && sonidoIntro != null) audioSource.PlayOneShot(sonidoIntro, volumenSFX);

        yield return null;

        float introDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(introDuration); 

        currentState = BossState.Idle;

        while (health > phase2Threshold && currentState != BossState.Dead)
        {
            if (!isAttacking)
            {
                yield return StartCoroutine(ChooseRandomAttack());
            }
            yield return null;
        }

        if (health > 0 && currentState != BossState.Dead && !isPhase2Active)
        {
            yield return StartCoroutine(TransitionToPhase2Routine());
        }
    }

    IEnumerator TransitionToPhase2Routine()
    {
        isPhase2Active = true;
        isAttacking = true;
        currentState = BossState.Transition;

        DesactivarHitboxLunge();
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);

        if (audioSource != null && sonidoCambioFase != null) audioSource.PlayOneShot(sonidoCambioFase, volumenSFX);

        if (vinesAnimator != null)
        {
            vinesAnimator.SetTrigger("Appear");
        }

        if (floorVinesCollider != null)
        {
            floorVinesCollider.enabled = true;
        }

        animator.Play("IntroFinal"); 
        yield return null;

        float transitionDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(transitionDuration);

        currentState = BossState.Phase2;
        isAttacking = false;

        StartCoroutine(Phase2LogicRoutine());
    }

    IEnumerator Phase2LogicRoutine()
    {
        animator.Play("IdleFinal");

        while (currentState == BossState.Phase2 && health > 0)
        {
            yield return new WaitForSeconds(phase2AttackDelay);

            if (currentState == BossState.Dead) yield break;

            isAttacking = true;
            animator.Play("FiringPollen"); 

            if (audioSource != null && sonidoSporePhase2 != null) audioSource.PlayOneShot(sonidoSporePhase2, volumenSFX);

            yield return null;

            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength * 0.3f); 

            if (sporePrefab != null && sporeSpitPoint != null)
            {
                Instantiate(sporePrefab, sporeSpitPoint.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(animationLength * 0.7f);

            if (currentState == BossState.Phase2 && health > 0)
            {
                animator.Play("IdleFinal");
            }

            isAttacking = false;
        }
    }

    IEnumerator ChooseRandomAttack()
    {
        isAttacking = true;
        animator.Play("Idle"); 
        yield return new WaitForSeconds(Random.Range(1f, 2.5f));

        int attackChoice;
        do
        {
            // MODIFICADO: Rango aumentado a 4 opciones de ataque aleatorio
            attackChoice = Random.Range(0, 4);
        } 
        while (attackChoice == lastAttackChoice);

        lastAttackChoice = attackChoice;

        if (attackChoice == 0)
        {
            currentState = BossState.LungeAttack;
            yield return StartCoroutine(LungeAttackRoutine());
        }
        else if (attackChoice == 1)
        {
            currentState = BossState.GatlingAttack;
            yield return StartCoroutine(GatlingAttackRoutine());
        }
        else if (attackChoice == 2)
        {
            currentState = BossState.FloatingAttack;
            yield return StartCoroutine(FloatingAttackRoutine());
        }
        else
        {
            // MODIFICADO: Activación del nuevo estado ofensivo
            currentState = BossState.PlatformVinesAttack;
            yield return StartCoroutine(PlatformVinesAttackRoutine());
        }

        currentState = BossState.Idle;
        isAttacking = false;
    }

    IEnumerator LungeAttackRoutine()
    {
        isLungeHigh = Random.value > 0.5f;
        animator.SetBool("IsLungeHigh", isLungeHigh);
        animator.SetTrigger("LungeTrigger");

        if (audioSource != null && sonidoLunge != null) audioSource.PlayOneShot(sonidoLunge, volumenSFX);

        yield return new WaitForSeconds(lungeTotalDuration);
        DesactivarHitboxLunge();
    }

    public void ActivarHitboxLunge()
    {
        if (cabezaCollider != null) cabezaCollider.enabled = false;

        if (isLungeHigh && lungeHighCollider != null) lungeHighCollider.enabled = true;
        else if (!isLungeHigh && lungeLowCollider != null) lungeLowCollider.enabled = true;
    }

    public void DesactivarHitboxLunge()
    {
        if (lungeHighCollider != null) lungeHighCollider.enabled = false;
        if (lungeLowCollider != null) lungeLowCollider.enabled = false;
        if (cabezaCollider != null) cabezaCollider.enabled = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    [Header("Seed Gatling Settings")]
    public GameObject seedPrefab;        
    public Transform spitPoint;          
    public GameObject muzzleFlashObject; 
    public float gatlingDuration = 2.5f;  
    public int totalSeedsToSpawn = 3;    
    public float delayBetweenSeeds = 0.4f; 

    [Header("Configuración de Lluvia desde el Cielo")]
    public float spawnHeightY = 7f;   
    public float minXRange = -8f;     
    public float maxXRange = 4f;      

    IEnumerator GatlingAttackRoutine()
    {
        animator.SetTrigger("GatlingTrigger");

        if (audioSource != null && sonidoGatling != null) 
        {
            audioSource.clip = sonidoGatling;
            audioSource.volume = volumenSFX;
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(0.4f); 
        
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(true);

        for (int i = 0; i < totalSeedsToSpawn; i++)
        {
            if (seedPrefab != null)
            {
                float randomX = Random.Range(minXRange, maxXRange);
                Vector3 spawnPosition = new Vector3(randomX, spawnHeightY, transform.position.z);
                Instantiate(seedPrefab, spawnPosition, Quaternion.identity);
            }
            yield return new WaitForSeconds(delayBetweenSeeds);
        }
        
        float timeSpawning = totalSeedsToSpawn * delayBetweenSeeds;
        float remainingTime = gatlingDuration - 0.4f - timeSpawning;
        
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    [Header("Floating Attack Settings")]
    public GameObject projectilePrefab;     
    public Transform handsCenterPoint;       
    public float floatingAttackDuration = 3f;
    public int totalProjectiles = 3;         
    public float timeBetweenProjectiles = 0.6f;

 IEnumerator FloatingAttackRoutine()
{
    // 1. Encendemos el disparador de la animación
    animator.SetTrigger("FloatingAttackTrigger");
    
    // 2. Esperamos un frame para que el Animator procese el cambio
    yield return null; 

    // 3. El tiempo de espera exacto que tenías configurado antes de lanzar
    float tiempoAntesDeSpawnear = 0.9f; 
    yield return new WaitForSeconds(tiempoAntesDeSpawnear);

    // 4. Soltamos las ráfagas de proyectiles
    for (int i = 0; i < totalProjectiles; i++)
    {
        if (projectilePrefab != null && handsCenterPoint != null)
        {
            Vector3 spawnPos = handsCenterPoint.position;
            Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(timeBetweenProjectiles);
    }

    // 5. Calculamos el tiempo restante de la animación para que no se corte
    float timeSpawning = totalProjectiles * timeBetweenProjectiles;
    float remainingTime = floatingAttackDuration - tiempoAntesDeSpawnear - timeSpawning;

    if (remainingTime > 0)
    {
        yield return new WaitForSeconds(remainingTime);
    }
}

// Corrutina auxiliar para soltar las 3 ráfagas seguidas sin depender del frame exacto del jefe
IEnumerator SpawnProjectilesQueue()
{
    for (int i = 0; i < totalProjectiles; i++)
    {
        if (projectilePrefab != null && handsCenterPoint != null)
        {
            Vector3 spawnPos = handsCenterPoint.position;
            Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(timeBetweenProjectiles);
    }
}

    // NUEVO: Corrutina para procesar de manera aleatoria el brote de vainas en las plataformas
    IEnumerator PlatformVinesAttackRoutine()
    {
        animator.SetTrigger("FloatingAttackTrigger"); 

        if (vainasPlataformas != null && vainasPlataformas.Length > 0)
        {
            int plataformaObjetivo = Random.Range(0, vainasPlataformas.Length);

            if (vainasPlataformas[plataformaObjetivo] != null)
            {
                if (audioSource != null && sonidoAlertaVainas != null)
                {
                    audioSource.PlayOneShot(sonidoAlertaVainas, volumenSFX);
                }

                yield return StartCoroutine(vainasPlataformas[plataformaObjetivo].ActivarAtaque(tiempoAlertaVaina, tiempoDanoVaina));
            }
        }
        else
        {
            yield return new WaitForSeconds(1.0f); 
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead) return;
        
        health -= damage;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashWhite());
        
        if (health <= 0) Die();
    }

    IEnumerator FlashWhite()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.white; 
        yield return new WaitForSeconds(duracionParpadeo);
        
        spriteRenderer.color = colorParpadeo; 
        yield return new WaitForSeconds(duracionParpadeo);
        
        spriteRenderer.color = Color.white;
        flashCoroutine = null; 
    }

void Die()
    {
        if (currentState == BossState.Dead) return;
        currentState = BossState.Dead;
        
        StopAllCoroutines(); 

        IntroNivelAudio scriptMusica = Object.FindFirstObjectByType<IntroNivelAudio>();
        if (scriptMusica != null && scriptMusica.audioSourceMusica != null)
        {
            scriptMusica.audioSourceMusica.Stop();
        }

        if (audioSource != null)
        {
            audioSource.Stop(); 
            if (sonidoKnockout != null) audioSource.PlayOneShot(sonidoKnockout, volumenSFX);
        }

        if (cabezaCollider != null) cabezaCollider.enabled = false;
        if (lungeHighCollider != null) lungeHighCollider.enabled = false;
        if (lungeLowCollider != null) lungeLowCollider.enabled = false;
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        if (animator != null)
        {
            animator.Play("Knockout", 0, 0f);
            animator.Update(0f); 
        }

        // --- ALERTA DE DIAGNÓSTICO ---
        if (levelExitManager != null)
        {
            Debug.Log("[CAGNEY] ¡He muerto! Avisando con éxito a LevelExitManager para cambiar de escena.");
            levelExitManager.IniciarRetornoAlTitulo();
        }
        else
        {
            Debug.LogError("🚨 [ERROR CRÍTICO] Cagney no puede cambiar de escena porque la casilla 'Level Exit Manager' en su Inspector está VACÍA (None).");
        }
    }

    IEnumerator FadeOutAudioMuerteRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        float duracionFade = 2.5f; 
        float tiempoInicio = Time.time;

        while (Time.time < tiempoInicio + duracionFade)
        {
            float porcentajeCompletado = (Time.time - tiempoInicio) / duracionFade;
            audioSource.volume = Mathf.Lerp(volumenSFX, 0f, porcentajeCompletado);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.loop = false;
    }
}