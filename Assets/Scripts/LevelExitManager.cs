using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Requerido para cambiar de escena

public class LevelExitManager : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre exacto de la escena del menú principal.")]
    public string nombreEscenaTitulo = "MainMenu";

    [Header("Tiempos de Espera")]
    [Tooltip("Tiempo en segundos que esperará el juego tras la muerte del jefe.")]
    public float tiempoEsperaTrasMuerte = 2.0f; // <--- Cambiado a 2 segundos

    // Esta es la función que llamará Cagney en cuanto su vida llegue a 0
    public void IniciarRetornoAlTitulo()
    {
        StartCoroutine(CambiarEscenaRoutine());
    }

    private IEnumerator CambiarEscenaRoutine()
    {
        // Espera los 2 segundos configurados
        yield return new WaitForSeconds(tiempoEsperaTrasMuerte);

        // Carga la escena del menú
        SceneManager.LoadScene(nombreEscenaTitulo);
    }
}