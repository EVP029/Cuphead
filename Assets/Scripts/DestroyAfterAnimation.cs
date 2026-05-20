using UnityEngine;
using System.Collections;

public class DestroyEffect : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(DestroyAfterAnimationRoutine());
    }

    IEnumerator DestroyAfterAnimationRoutine()
    {
        // 1. Esperamos un frame para que el Animator cargue el clip correctamente en memoria
        yield return null;

        if (anim != null)
        {
            // 2. Obtenemos la información del estado actual de la animación
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            
            // 3. Esperamos exactamente lo que dura el clip de animación
            yield return new WaitForSeconds(stateInfo.length);
        }
        else
        {
            // Si por alguna razón no hay Animator, lo destruimos en 0.5 segundos para que no se quede trabado
            yield return new WaitForSeconds(0.5f);
        }

        // 4. Borramos el efecto visual por completo
        Destroy(gameObject);
    }
}