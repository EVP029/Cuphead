using UnityEngine;
using System.Collections;

// Modificado: Se implementa la interfaz IDamageable para recibir daño de la bala
public class ToothyPlant : MonoBehaviour, IDamageable
{
    [Header("Ajustes de Caída")]
    public float fallSpeed = 7f;

    [Header("Ajustes de Planta")]
    public float health = 8f;
    public float lifetime = 5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;
    private SpriteRenderer spriteRenderer; // NUEVO: Para poder hacer el parpadeo de color
    private bool hasHitGround = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // NUEVO: Inicialización del SpriteRenderer

        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        
        // Cae recto desde donde el jefe la creó
        rb.linearVelocity = new Vector2(0, -fallSpeed);
    }

    void Update()
    {
        if (hasHitGround)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Daño al entrar en contacto
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0 && !hasHitGround)
        {
            PlantItself();
        }

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerMovement>()?.TakeDamage();
        }
    }

    // NUEVO: Daño si el jugador se queda parado encima de la planta
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerMovement>()?.TakeDamage();
        }
    }

    void PlantItself()
    {
        hasHitGround = true;
        
        // En lugar de rb.simulated = false, hacemos esto:
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Sigue activo para detectar a Cuphead, pero no le afecta la gravedad

        anim.SetTrigger("HitGround"); 
        
        // Iniciamos el conteo de vida por tiempo
        StartCoroutine(LifetimeRoutine());
    }

    // Modificado: Cambiado a int para cumplir con la interfaz IDamageable
    public void TakeDamage(int damage)
    {
        if (!hasHitGround || isDead) return;

        health -= damage;
        
        // NUEVO: Activa el parpadeo rojo al recibir el golpe
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FlashWhite());
        }

        if (health <= 0) 
        {
            StartDeathSequence();
        }
    }

    // NUEVO: Corrutina de intermitencia idéntica a la de Cagney
// Corrutina modificada en la planta para parpadear en blanco puro
IEnumerator FlashWhite()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        Color flashColor = new Color(1f, 1f, 1f, 0.4f); // Blanco suavizado al 40%

        // Parpadeo 1
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.04f);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.04f);
        
        // Parpadeo 2
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.04f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        if (!isDead)
        {
            StartDeathSequence();
        }
    }

    // NUEVO: Manejo de la muerte con animación
    void StartDeathSequence()
    {
        isDead = true;
        
        // Desactivamos el collider para que ya no dañe al jugador ni reciba más balas mientras muere
        if (col != null) col.enabled = false;

        // Activamos la animación de muerte en el Animator
        anim.SetTrigger("DieTrigger");

        // Buscamos cuánto dura la animación de muerte actual para destruir el objeto justo al terminar
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        Destroy(gameObject, stateInfo.length);
    }
}