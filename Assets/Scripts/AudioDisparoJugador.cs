using UnityEngine;

public class AudioDisparoJugador : MonoBehaviour
{
    [Header("Efectos de Sonido (Audio)")]
    private AudioSource audioSource;     // El componente que reproducirá el sonido
    public AudioClip sonidoDisparoLoop;  // El archivo de audio del disparo
    [Range(0f, 1f)] 
    public float volumenDisparo = 0.6f;  // Volumen del disparo

    void Start()
    {
        // Buscamos o añadimos el componente AudioSource en Cuphead
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configuración inicial limpia para el bucle de audio
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Audio 2D puro
        audioSource.clip = sonidoDisparoLoop;
        audioSource.volume = volumenDisparo;
        audioSource.loop = true; // El sonido se repetirá mientras se mantenga presionado
    }

    void Update()
    {
        // Al presionar la tecla X por primera vez:
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (audioSource != null && sonidoDisparoLoop != null && !audioSource.isPlaying)
            {
                audioSource.Play(); // Inicia la ráfaga de sonido
            }
        }

        // En el instante en que se suelta la tecla X:
        if (Input.GetKeyUp(KeyCode.X))
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop(); // Detiene el sonido inmediatamente
            }
        }
    }
}