using UnityEngine;
using System.Collections;

public class BossCagney : MonoBehaviour
{
    public enum BossState { Intro, Idle, LungeAttack, GatlingAttack, Transition, Phase2, Dead }
    
    [Header("Health & Phases")]
    public int health = 1000;
    public int phase2Threshold = 400; 
    
    [Header("State")]
    public BossState currentState;
    private bool isAttacking = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentState = BossState.Intro;
        StartCoroutine(BossLogicRoutine());
    }

    IEnumerator BossLogicRoutine()
    {
        yield return new WaitForSeconds(2f); 
        currentState = BossState.Idle;

        while (health > 0 && currentState != BossState.Phase2)
        {
            if (!isAttacking)
            {
                yield return StartCoroutine(ChooseRandomAttack());
            }
            yield return null;
        }
    }

    IEnumerator ChooseRandomAttack()
    {
        isAttacking = true;
        animator.Play("Idle"); 
        yield return new WaitForSeconds(Random.Range(1f, 2.5f));

        int attackChoice = Random.Range(0, 2);

        if (attackChoice == 0)
        {
            currentState = BossState.LungeAttack;
            yield return StartCoroutine(LungeAttackRoutine());
        }
        else
        {
            currentState = BossState.GatlingAttack;
            yield return StartCoroutine(GatlingAttackRoutine());
        }

        currentState = BossState.Idle;
        isAttacking = false;
    }

    [Header("Lunge Attack Settings")]
    public float lungeTotalDuration = 2.5f; 
    public GameObject lungeHighHitbox;
    public GameObject lungeLowHitbox;
    private bool isLungeHigh = false;

    IEnumerator LungeAttackRoutine()
    {
        isLungeHigh = Random.value > 0.5f;
        animator.SetBool("IsLungeHigh", isLungeHigh);
        animator.SetTrigger("LungeTrigger");
        yield return new WaitForSeconds(lungeTotalDuration);
        DesactivarHitboxLunge();
    }

    public void ActivarHitboxLunge()
    {
        if (isLungeHigh && lungeHighHitbox != null) lungeHighHitbox.SetActive(true);
        else if (!isLungeHigh && lungeLowHitbox != null) lungeLowHitbox.SetActive(true);
    }

    public void DesactivarHitboxLunge()
    {
        if (lungeHighHitbox != null) lungeHighHitbox.SetActive(false);
        if (lungeLowHitbox != null) lungeLowHitbox.SetActive(false);
    }

    // ========================================================================
    // --- EDICIÓN PARA SPAWNEAR DESDE EL CIELO DIRECTAMENTE ---
    // ========================================================================
    [Header("Seed Gatling Settings")]
    public GameObject seedPrefab;        
    public Transform spitPoint;          
    public GameObject muzzleFlashObject; 
    public float gatlingDuration = 2.5f;  
    public int totalSeedsToSpawn = 3;    
    public float delayBetweenSeeds = 0.4f; 

    [Header("Configuración de Lluvia desde el Cielo")]
    public float spawnHeightY = 7f;   // Altura en el cielo (fuera de cámara) para crear la semilla
    public float minXRange = -8f;     // Límite izquierdo del escenario
    public float maxXRange = 4f;      // Límite derecho del escenario

    IEnumerator GatlingAttackRoutine()
    {
        // 1. Encendemos el Trigger para que inicie el clip completo "GatlingAttack"
        animator.SetTrigger("GatlingTrigger");
        
        // Esperamos un momento breve en lo que levanta la cabeza antes de sacar las partículas
        yield return new WaitForSeconds(0.4f); 
        
        // 2. ACTIVACIÓN: Encendemos la animación de la boca
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(true);

        // 3. RÁFAGA DIRECTA AL CIELO:
        for (int i = 0; i < totalSeedsToSpawn; i++)
        {
            if (seedPrefab != null)
            {
                // Calculamos la posición aleatoria arriba del mapa en este instante
                float randomX = Random.Range(minXRange, maxXRange);
                Vector3 spawnPosition = new Vector3(randomX, spawnHeightY, transform.position.z);

                // Instanciamos la semilla directamente allá arriba
                Instantiate(seedPrefab, spawnPosition, Quaternion.identity);
            }
            yield return new WaitForSeconds(delayBetweenSeeds);
        }
        
        // 4. ESPERA: Calculamos el tiempo que le queda a la animación principal para terminar por completo
        float timeSpawning = totalSeedsToSpawn * delayBetweenSeeds;
        float remainingTime = gatlingDuration - 0.4f - timeSpawning;
        
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // 5. DESACTIVACIÓN: Apagamos el efecto de la boca JUSTO al salir del estado de ataque
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);
    }
    // ========================================================================

    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead) return;
        health -= damage;
        StartCoroutine(FlashWhite());
        if (health <= 0) Die();
    }

    IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.red; 
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        currentState = BossState.Dead;
        StopAllCoroutines();
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);
        animator.Play("Knockout"); 
    }
}