using UnityEngine;
using System.Collections;

public class VinesPlatformAttack : MonoBehaviour
{
    private Animator animator;
    private Collider2D damageCollider;

    void Awake()
    {
        animator = GetComponent<Animator>();
        damageCollider = GetComponent<Collider2D>();
        
        // Empezamos con el collider apagado para no hacer daño invisible
        if (damageCollider != null) damageCollider.enabled = false;
    }

    // Este método lo llamará Cagney desde su rutina
    public IEnumerator ActivarAtaque(float duracionAlerta, float duracionAtaque)
    {
        // 1. Fase de Alerta: Avisa al jugador (puedes poner una animación de vibración o señal)
        animator.Play("Vines_Alert"); // Si no tienes animación de alerta, usa la misma o déjala quieta
        yield return new WaitForSeconds(duracionAlerta);

        // 2. Fase de Daño: Brotan las vainas
        animator.Play("Vines_Attack"); // Reemplaza con el nombre exacto de tu animación
        if (damageCollider != null) damageCollider.enabled = true;

        yield return new WaitForSeconds(duracionAtaque);

        // 3. Fase de Retirada: Apagamos el peligro
        if (damageCollider != null) damageCollider.enabled = false;
        animator.Play("New State"); // Vuelve a su estado vacío u oculto
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerMovement>()?.TakeDamage();
        }
    }
}