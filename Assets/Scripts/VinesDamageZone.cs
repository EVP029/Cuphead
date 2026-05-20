using UnityEngine;

public class VinesDamageZone : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damageToPlayer = 1;

    [Header("Configuración de la Fuerza de Empuje (Anti-Dashes)")]
    [Tooltip("Intenta un valor entre 10 y 13 para el salto vertical.")]
    public float fuerzaImpulsoVertical = 11f; 

    private float tiempoProximoRebote = 0f;
    private float cooldownRebote = 0.2f; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.time < tiempoProximoRebote) return;
            tiempoProximoRebote = Time.time + cooldownRebote;

            // 1. Aplicar daño
            collision.SendMessage("TakeDamage", damageToPlayer, SendMessageOptions.DontRequireReceiver);
            
            // 2. Control total de físicas con Fuerza de Impulso
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // INTENTO DE FRENAR SCRIPTS INTERNOS:
                // Si tu script de Dash usa Corrutinas o funciones para detenerse, 
                // esto intentará llamar a una función que cancele el dash si existe.
                collision.SendMessage("StopDash", SendMessageOptions.DontRequireReceiver);
                collision.SendMessage("CancelDash", SendMessageOptions.DontRequireReceiver);

                // TRUCO MAESTRO: Forzamos al Rigidbody a pasar a modo Kinematic un milisegundo 
                // y luego volver a Dynamic. Esto borra CUALQUIER fuerza, inercia, dash o empuje
                // horizontal que el jugador estuviera haciendo en ese frame exacto.
                playerRb.isKinematic = true;
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.isKinematic = false;
                
                // Ahora que el jugador está 100% "limpio" en el aire, aplicamos el impulso vertical puro
                playerRb.AddForce(Vector2.up * fuerzaImpulsoVertical, ForceMode2D.Impulse);
                
                Debug.Log("¡Fuerzas horizontales y verticales reseteadas! Rebote vertical limpio.");
            }
        }
    }
}