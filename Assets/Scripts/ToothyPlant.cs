using UnityEngine;
using System.Collections;

public class ToothyPlant : MonoBehaviour
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
    private bool hasHitGround = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

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

    public void TakeDamage(float damage)
    {
        if (!hasHitGround || isDead) return;

        health -= damage;
        if (health <= 0) 
        {
            StartDeathSequence();
        }
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