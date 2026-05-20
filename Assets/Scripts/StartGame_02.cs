using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame_02 : MonoBehaviour
{
    // public Animator transitionAnimation;

    public string nombreNivel;

    public float tiempoEspera = 1.5f;

    private bool yaEmpezo = false;

    public void EmpezarJuego() {
        if (!yaEmpezo) {
            yaEmpezo = true;

            // StartCoroutine(StartJueguito());

            SceneManager.LoadScene(nombreNivel);
        }
    }

    //System.Collections.IEnumerator StartJueguito() {
    //    // Activar animación
    //    transitionAnimation.SetTrigger("Start");

    //    // Esperar
    //    yield return new WaitForSeconds(tiempoEspera);

    //    // Cambiar escena
    //    SceneManager.LoadScene(nombreNivel);
    //}
}