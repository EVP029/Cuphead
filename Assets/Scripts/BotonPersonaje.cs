using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class BotonPersonaje : MonoBehaviour, ISelectHandler, IDeselectHandler {
    public Animator animator;

    public Image fondoSeleccionado;

    public StartGame_02 transitionManager;

    public void OnSelect(BaseEventData eventData) {
        animator.SetBool("Selected", true);
        fondoSeleccionado.enabled = true;
    }

    public void OnDeselect(BaseEventData eventData) {
        animator.SetBool("Selected", false);
        fondoSeleccionado.enabled = false;
    }

    public void Presionar() {
        StartCoroutine(SecuenciaInicio());
    }

    IEnumerator SecuenciaInicio() {
        yield return new WaitForSeconds(0.1f);

        transitionManager.EmpezarJuego();
    }
}