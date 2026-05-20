using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    [Header("Configuración del Movimiento")]
    public float amplitude = 0.2f; 
    public float speed = 1.5f;     

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        // El script AHORA SOLO SE ENCARGA DE MOVERLA, sin tocar al jugador
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}