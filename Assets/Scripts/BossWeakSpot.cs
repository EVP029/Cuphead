using UnityEngine;
using System.Collections;

public class BossWeakSpot : MonoBehaviour, IDamageable
{
    private BossCagney mainBoss;
    private SpriteRenderer bossSpriteRenderer;
    private bool isFlashing = false;

    void Start()
    {
        // Buscamos el script principal en el objeto padre
        mainBoss = GetComponentInParent<BossCagney>();
        
        // Buscamos el SpriteRenderer en el objeto padre (Cagney)
        if (mainBoss != null)
        {
            bossSpriteRenderer = mainBoss.GetComponent<SpriteRenderer>();
        }
    }

    public void TakeDamage(int damage)
    {
        if (mainBoss != null)
        {
            // 1. Le restamos la vida al jefe principal
            mainBoss.health -= damage;
            
            // Si la vida llega a 0 o menos, avisamos al jefe que muera
            if (mainBoss.health <= 0)
            {
                mainBoss.TakeDamage(0); // Esto activará su método Die() original
                return;
            }

            // 2. Hacemos que todo el cuerpo de Cagney parpadee desde aquí
            if (!isFlashing && gameObject.activeInHierarchy)
            {
                StartCoroutine(FlashBossSprite());
            }
        }
    }

    // Corrutina local para asegurar el parpadeo continuo en el SpriteRenderer del padre
// Corrutina modificada para parpadear en blanco puro estilo Cuphead
IEnumerator FlashBossSprite()
    {
        if (bossSpriteRenderer == null) yield break;
        
        isFlashing = true;

        // Guardamos el color original del jefe (que normalmente es blanco puro con Alfa 1)
        Color originalColor = bossSpriteRenderer.color;
        
        // Creamos un color blanco con un 40% de intensidad/transparencia (0.4f)
        // Esto hará que se mezcle con el sprite original, haciéndolo ver iluminado pero no cegador
        Color flashColor = new Color(1f, 1f, 1f, 0.4f); 

        // Parpadeo 1
        bossSpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.04f);
        bossSpriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.04f);
        
        // Parpadeo 2
        bossSpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.04f);
        bossSpriteRenderer.color = originalColor;

        isFlashing = false;
    }
}