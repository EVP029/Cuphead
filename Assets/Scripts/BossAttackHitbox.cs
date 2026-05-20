using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [Header("Configuración de Daño al Jugador")]
    public int damageToPlayer = 1;

    private BossCagney bossPrincipal;

    void Awake()
    {
        bossPrincipal = GetComponentInParent<BossCagney>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.SendMessage("TakeDamage", damageToPlayer, SendMessageOptions.DontRequireReceiver);
            Debug.Log("¡Cagney dañó al jugador!");
        }

        if (collision.CompareTag("Bullet")) 
        {
            if (bossPrincipal != null)
            {
                bossPrincipal.TakeDamage(10); 
            }
        }
    }
}