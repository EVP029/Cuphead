using UnityEngine;

public class PlayerOverworld : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool lookingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Evita que el personaje salga volando si choca con esquinas
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Obtener Input (Raw para respuesta instantánea tipo arcade)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 2. Normalizar para que no corra más rápido en diagonal
        if (moveInput.magnitude > 1)
        {
            moveInput.Normalize();
        }

        // 3. Animaciones
        // Usamos valores absolutos para las animaciones de caminar si tu Animator es simple
        // O mandamos los valores directos para un Blend Tree
        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.y);
        anim.SetFloat("Speed", moveInput.magnitude);

        // 4. Lógica de Giro (Flip)
        if (moveInput.x > 0 && !lookingRight)
        {
            TurnAround();
        }
        else if (moveInput.x < 0 && lookingRight)
        {
            TurnAround();
        }
    }

    void FixedUpdate()
    {
        // Movimiento consistente usando la velocidad del Rigidbody
        rb.linearVelocity = moveInput * speed;
    }

    void TurnAround()
    {
        lookingRight = !lookingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
