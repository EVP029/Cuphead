using UnityEngine;
using UnityEngine.SceneManagement;

/*
    Hace el cambio de escena del Menú principal al Menú de la tacita, esperando a que termine la animación de la transición 
    Versión para Teclado (Sin Mouse)
*/


public class StartGame : MonoBehaviour {

    public Animator transitionAnimation;
    public string nombreNivel; 

    public float tiempoEspera = 1.5f;
    private bool yaEmpezo = false;

    void Update() {
        if (!yaEmpezo && Input.anyKeyDown) {
            yaEmpezo = true; 
            StartCoroutine(StartJueguito());
        }
    }

    System.Collections.IEnumerator StartJueguito() {

        transitionAnimation.SetTrigger("Start"); // Activar animacion - iris

        yield return new WaitForSeconds(tiempoEspera); // Esperar a que termine

        SceneManager.LoadScene(nombreNivel);
    }
}