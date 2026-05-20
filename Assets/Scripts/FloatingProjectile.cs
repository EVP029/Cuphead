using UnityEngine;
using System.Collections;

public class FloatingProjectile : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 8f; 
    public bool moveRight = false; 
    public float waveFrequency = 3f;  
    public float waveMagnitude = 0.5f; 

    [Header("Mecánica de Relevo")]
    public bool spawnReturnOnExit = false; 
    public GameObject returnProjectilePrefab; 

    [Header("Efectos de Audio (Proyectil)")]
    private AudioSource audioSource;
    public AudioClip sonidoAparicion; 
    [Range(0f, 1f)] public float volumenSFX = 0.6f;
    public float tiempoAntesDelFade = 1.2f;
    public float duracionFade = 0.4f;

    private float screenLimitX;
    private Vector3 startPosition;
    private float animationTimer;
    private bool hasTriggeredExit = false;

    void Start()
    {
        Debug.Log($"[PROYECTIL] ¡He nacido! Mi nombre es {gameObject.name} y mi posición X es {transform.position.x}");

        startPosition = transform.position;
        animationTimer = Random.Range(0f, 10f); 
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 

        if (sonidoAparicion != null)
        {
            audioSource.clip = sonidoAparicion;
            audioSource.volume = volumenSFX;
            audioSource.loop = true;
            audioSource.Play();
            StartCoroutine(FadeOutAudioProyectilRoutine());
        }

        if (Camera.main != null)
        {
            float camEdge = Camera.main.ScreenToWorldPoint(new Vector3(moveRight ? Screen.width : 0, 0, 0)).x;
            screenLimitX = moveRight ? camEdge + 2f : camEdge - 2f;
            Debug.Log($"[PROYECTIL] Cámara detectada. Mi límite screenLimitX es: {screenLimitX}");
        }
        else
        {
            screenLimitX = moveRight ? 12f : -12f;
            Debug.LogWarning($"[PROYECTIL] ¡Ojo! Camera.main es NULL. Usando límite por defecto: {screenLimitX}");
        }

        // REVISIÓN DE DESTRUCCIÓN INMEDIATA
        if (!moveRight && transform.position.x < screenLimitX)
        {
            Debug.LogError($"[PROYECTIL CRÍTICO] ¡Me voy a morir en el Start()! Mi X ({transform.position.x}) ya es menor que el límite ({screenLimitX})");
        }

        Destroy(gameObject, 10f); 
    }

    void Update()
    {
        Vector3 direction = moveRight ? Vector3.right : Vector3.left;
        startPosition += direction * speed * Time.deltaTime;

        animationTimer += Time.deltaTime * waveFrequency;
        float posY = startPosition.y + Mathf.Sin(animationTimer) * waveMagnitude;
        transform.position = new Vector3(startPosition.x, posY, transform.position.z);

        if (!moveRight) 
        {
            if (transform.position.x < screenLimitX && !hasTriggeredExit)
            {
                Debug.Log($"[PROYECTIL] Crucé el límite izquierdo ({transform.position.x} < {screenLimitX}). Activando relevo.");
                hasTriggeredExit = true;
                CheckExitLogic();
            }
        }
        else 
        {
            if (transform.position.x > screenLimitX)
            {
                Debug.Log($"[PROYECTIL] Regresé al límite derecho ({transform.position.x} > {screenLimitX}). Destruyendo.");
                Destroy(gameObject);
            }
        }
    }

    void CheckExitLogic()
    {
        if (spawnReturnOnExit && returnProjectilePrefab != null)
        {
            GameObject returnPoint = GameObject.Find("ReturnSpawnPoint");
            
            if (returnPoint != null)
            {
                GameObject newProj = Instantiate(returnProjectilePrefab, returnPoint.transform.position, Quaternion.identity);
                FloatingProjectile projScript = newProj.GetComponent<FloatingProjectile>();
                if (projScript != null)
                {
                    projScript.moveRight = true;
                    projScript.spawnReturnOnExit = false; 
                    
                    if (Camera.main != null)
                    {
                        float camEdge = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
                        projScript.screenLimitX = camEdge + 2f;
                    }
                    else
                    {
                        projScript.screenLimitX = 12f;
                    }
                    newProj.transform.localScale = new Vector3(-Mathf.Abs(newProj.transform.localScale.x), newProj.transform.localScale.y, newProj.transform.localScale.z);
                }
            }
            else
            {
                Debug.LogError("[PROYECTIL] Error: No encontré ningún objeto llamado 'ReturnSpawnPoint' en la escena.");
            }
        }

        Destroy(gameObject); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[PROYECTIL] He chocado con algo: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerMovement>()?.TakeDamage();
            Debug.Log("[PROYECTIL] Le di al jugador. Destruyéndome.");
            Destroy(gameObject); 
        }
    }

    IEnumerator FadeOutAudioProyectilRoutine()
    {
        yield return new WaitForSeconds(tiempoAntesDelFade);
        float tiempoInicio = Time.time;
        while (Time.time < tiempoInicio + duracionFade)
        {
            if (audioSource == null) yield break;
            float porcentajeCompletado = (Time.time - tiempoInicio) / duracionFade;
            audioSource.volume = Mathf.Lerp(volumenSFX, 0f, porcentajeCompletado);
            yield return null;
        }
        if (audioSource != null)
        {
            audioSource.volume = 0f;
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
}