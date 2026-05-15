using UnityEngine; 

public class Bullet : MonoBehaviour 
{ 
    public float speed = 20f; 
    public float lifeTime = 1.5f; 
    private Vector3 moveDirection; 

    [Header("Cuphead System")]
    public bool isSpecial = false; 
    private PlayerMovement player;

    public void SetDirection(float dir) 
    { 
        // 1. Detectar orientación del firePoint
        moveDirection = transform.right; 

        // 2. Lógica de movimiento horizontal vs vertical
        if (Mathf.Abs(moveDirection.y) < 0.5f) 
        { 
            moveDirection = new Vector3(dir, 0, 0); 
            
            // Volteamos el sprite/objeto según dirección
            transform.localScale = new Vector3(dir, 1, 1); 
        } 
        else 
        { 
            moveDirection = Vector3.up; 
        }
    } 

    void Start() 
    { 
        Destroy(gameObject, lifeTime); 
        player = FindObjectOfType<PlayerMovement>();
        
        // NOTA: Si al disparar la bola de fuego sigue quieta, 
        // es porque el script PlayerMovement no está llamando a SetDirection.
    } 

    void Update() 
    { 
        // Se mueve en la dirección calculada
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World); 
    } 

    private void OnTriggerEnter2D(Collider2D collision) 
    { 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) 
        { 
            Destroy(gameObject); 
        } 

        if (collision.CompareTag("Enemy")) 
        { 
            // Si es bala normal, cargamos energía
            if (!isSpecial && player != null)
            {
                player.AddEnergy(0.1f);
            }

            // Si es especial, podrías hacer que atraviese enemigos no destruyéndola
            if (!isSpecial) 
            {
                Destroy(gameObject); 
            }
            else 
            {
                // Opcional: Destruir especial solo si choca con algo muy pesado
                // Destroy(gameObject); 
            }
        } 
    } 
}