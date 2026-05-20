using UnityEngine;

public class VidaJugador : MonoBehaviour
{
    [Header("Estadísticas")]
    public int vidasMaximas = 3;
    private int vidasActuales;

    [Header("Referencia a la UI")]
    public ControladorVidaUI scriptVidaUI; // Arrastra aquí el objeto con el script de la UI

    private bool esInvencible = false;
    public float tiempoInvencibilidad = 1.5f;

    void Start()
    {
        vidasActuales = vidasMaximas;
        // Inicializamos la UI para que muestre la vida completa al empezar
        if (scriptVidaUI != null)
        {
            scriptVidaUI.ActualizarVidasUI(vidasActuales);
        }
    }

    // Esta función se ejecuta cuando Cuphead choca con algo
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Asegúrate de que el jefe o sus ataques tengan la etiqueta (Tag) "Enemy"
        if (collision.CompareTag("Enemy") && !esInvencible)
        {
            RecibirDanio();
        }
    }

    void RecibirDanio()
    {
        vidasActuales--;
        
        // Avisamos a la UI que actualice el sprite
        if (scriptVidaUI != null)
        {
            scriptVidaUI.ActualizarVidasUI(vidasActuales);
        }

        if (vidasActuales <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(TiempoDeEsperaDanio());
        }
    }

    System.Collections.IEnumerator TiempoDeEsperaDanio()
    {
        esInvencible = true;
        // Aquí puedes hacer que el sprite de Cuphead parpadee visualmente si quieres
        yield return new WaitForSeconds(tiempoInvencibilidad);
        esInvencible = false;
    }

    void Morir()
    {
        Debug.Log("Cuphead ha muerto. ¡Knockout!");
        // Aquí pondrías la pantalla de Game Over
    }
}