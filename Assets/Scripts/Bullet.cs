using UnityEngine; 

public class Bullet : MonoBehaviour 
{ 
    public float speed = 20f; 
    public float lifeTime = 1.5f; 
    private Vector3 moveDirection; 

    [Header("Cuphead System")]
    public bool isSpecial = false; 
    public int damageValue = 1; // Ajusta cuánto daño hace cada disparo normal en Unity
    public int specialDamageValue = 5; // NUEVO: Ajusta cuánto daño hace el ataque especial en Unity
    private PlayerMovement player;

    [Header("Efectos de Calidad de Vida")]
    public GameObject impactEffectPrefab; // NUEVO: Arrastra aquí tu Prefab del impacto visual

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
        // NUEVO: Si es una bala especial, le asignamos el daño pesado antes de que golpee
        if (isSpecial)
        {
            damageValue = specialDamageValue;
        }

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
        // MODIFICACIÓN: Si choca con un proyectil del enemigo, lo ignora olímpicamente
        if (collision.CompareTag("Projectile") || collision.CompareTag("EnemyProjectile"))
        {
            return; 
        }

        // NUEVO: Intentamos hacer daño a cualquier objeto compatible (Jefe o Plantita)
        IDamageable damageableTarget = collision.GetComponent<IDamageable>();
        if (damageableTarget != null)
        {
            damageableTarget.TakeDamage(damageValue);
        }

        // Modificado para que explote al tocar el suelo
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) 
        { 
            InstantiateImpact(); 
        } 

        if (collision.CompareTag("Enemy")) 
        { 
            // Si es bala normal, cargamos energía
            if (!isSpecial && player != null)
            {
                player.AddEnergy(0.1f);
            }

            // Si es bala normal, explota e impacta inmediatamente
            if (!isSpecial) 
            {
                // Modificado para que explote al tocar al enemigo
                InstantiateImpact(); 
            }
            else 
            {
                // MODIFICADO: Si es ESPECIAL, solo spawnea el efecto visual del golpe en el aire,
                // ¡pero NO destruye la bala! Así logra atravesar enemigos.
                if (impactEffectPrefab != null)
                {
                    Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
                }
            }
        } 
    } 

    // NUEVO: Método centralizado para spawnear el efecto y autodestruirse
    void InstantiateImpact()
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
}