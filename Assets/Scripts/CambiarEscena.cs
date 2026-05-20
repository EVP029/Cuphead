using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // OBLIGATORIO para usar Corrutinas

public class CambiarEscena : MonoBehaviour {
    public string nombreEscena;
    
    // Referencia al Animator que controlará el fondo negro
    public Animator transicionAnimator; 
    // Cuánto tiempo quieres que se quede la pantalla en negro antes de cargar
    public float tiempoEnNegro = 1.5f; 

    public void Cambiar() {
        Time.timeScale = 1f;

        // Si tenemos un animator asignado, hacemos la transición pro.
        // Si no, cambia instantáneamente para evitar que se rompa el juego.
        if (transicionAnimator != null)
        {
            StartCoroutine(SecuenciaTransicion());
        }
        else
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }

    IEnumerator SecuenciaTransicion()
{
    // Esta línea es la que "despierta" a la animación cuando tú quieres
    transicionAnimator.SetTrigger("IniciarFade");

    // Esperamos a que la pantalla esté negra antes de saltar al MAPA
    yield return new WaitForSeconds(tiempoEnNegro);

    SceneManager.LoadScene(nombreEscena);
}
}