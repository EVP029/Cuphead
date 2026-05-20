using UnityEngine;

public class AnunciadorPantalla : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Buscamos el componente Animator en el objeto de la UI
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("¡Falta el componente Animator en este objeto! Asegúrate de que tiene asignado el WALLOP_Controller.");
        }
    }

    // Función que manda a llamar el script de audio al iniciar el nivel
    public void JugarAnimacionIntro()
    {
        if (animator != null)
        {
            // Forzamos al Animator a reproducir el estado llamado "WALLOP"
            animator.Play("WALLOP"); 
        }
    }
}