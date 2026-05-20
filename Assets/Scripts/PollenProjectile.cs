using UnityEngine;

public class PollenProjectile : MonoBehaviour
{
    [Header("Movimiento Base")]
    public float speed = 5f;            // Velocidad de avance hacia adelante (izquierda)
    public bool moveLeft = true;        // Por defecto va a la izquierda. Si cambia de lado, desactívalo.

    [Header("Ajustes de la Onda (Seno)")]
    public float waveFrequency = 3f;    // Qué tan rápido hace la ondita (número de oscilaciones)
    public float waveAmplitude = 0.5f;  // Qué tan marcada o sutil es la onda (altura de la ondita)

    [Header("Configuración de Daño")]
    public int damageToPlayer = 1;      // Cuánto quita al jugador

    private float lifetime = 0f;
    private Vector3 startPosition;

    void Start()
    {
        // Guardamos la posición inicial para calcular la onda sobre este eje limpio
        startPosition = transform.position;
        
        // Destrucción automática si sale de la pantalla para no saturar la memoria (6 segundos)
        Destroy(gameObject, 6f);
    }

    void Update()
    {
        // 1. Calculamos el tiempo de vida para la fórmula matemática
        lifetime += Time.deltaTime;

        // 2. Movimiento horizontal recto
        float direction = moveLeft ? -1f : 1f;
        float currentX = startPosition.x + (lifetime * speed * direction);

        // 3. El truco mágico: Movimiento vertical usando Mathf.Sin para crear la onda sutil
        // Multiplicamos por la amplitud para que no sea una onda gigante de arriba a abajo.
        float currentY = startPosition.y + Mathf.Sin(lifetime * waveFrequency) * waveAmplitude;

        // 4. Aplicamos la nueva posición combinada al objeto
        transform.position = new Vector3(currentX, currentY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectamos si choca con el jugador
        if (collision.CompareTag("Player"))
        {
            // Aquí llamas al método de daño de tu script de jugador. Ejemplo:
            // collision.GetComponent<PlayerHealth>().TakeDamage(damageToPlayer);
            
            Destroy(gameObject); // El polen se deshace al impactar al jugador
        }
    }
}