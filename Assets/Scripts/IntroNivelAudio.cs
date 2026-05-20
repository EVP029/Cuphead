using UnityEngine;
using System.Collections;

public class IntroNivelAudio : MonoBehaviour
{
    [Header("Música de Fondo")]
    public AudioSource audioSourceMusica; 

    [Header("Audios de Introducción (Voz)")]
    public AudioSource audioSourceIntro;  
    public AudioClip[] variantesIntro;     

    [Header("Ajuste de Empalme (Crossfade)")]
    [Tooltip("A qué volumen de la voz quieres que entre la música. Un valor más alto (ej: 0.15) hará que la música entre MUCHO ANTES, cuando la voz aún suena fuerte.")]
    [Range(0.01f, 0.3f)] public float puntoDeEmpalme = 0.08f;

    private float[] samples = new float[256]; 

    void Start()
    {
        if (variantesIntro == null || variantesIntro.Length == 0)
        {
            ArrancarMusicaDirecto();
            return;
        }

        if (audioSourceIntro == null || audioSourceMusica == null) return;

        StartCoroutine(SecuenciaConEmpalme());
    }

    IEnumerator SecuenciaConEmpalme()
    {
        // 1. Elegimos y reproducimos la intro aleatoria
        int indiceAleatorio = Random.Range(0, variantesIntro.Length);
        AudioClip clipElegido = variantesIntro[indiceAleatorio];
        audioSourceIntro.clip = clipElegido;
        audioSourceIntro.Play();

        // 2. Esperamos un instante (0.4s) para dejar que la voz grite al principio con fuerza
        // sin que la música la interrumpa de golpe.
        yield return new WaitForSeconds(0.4f);

        bool musicaArrancada = false;

        // 3. BUCLE DE MONITOREO DE VOLUMEN
        while (audioSourceIntro.isPlaying)
        {
            audioSourceIntro.GetOutputData(samples, 0);
            float sumaVolumen = 0f;

            foreach (float sample in samples)
            {
                sumaVolumen += Mathf.Abs(sample);
            }
            float volumenPromedio = sumaVolumen / 256f;

            // ¡EL TRUCO DEL EMPALME!: En cuanto el volumen de la voz decae y cruza nuestro 'puntoDeEmpalme',
            // soltamos la música de inmediato en paralelo, mientras la voz termina de apagarse de forma natural.
            if (volumenPromedio < puntoDeEmpalme)
            {
                audioSourceMusica.Play();
                musicaArrancada = true;
                break; // Rompemos el bucle porque la música ya está corriendo
            }

            yield return null; 
        }

        // Seguridad: Si por alguna razón la voz era muy ruidosa y terminó de golpe sin cruzar el umbral,
        // nos aseguramos de encender la música de todos modos.
        if (!musicaArrancada)
        {
            audioSourceMusica.Play();
        }
    }

    void ArrancarMusicaDirecto()
    {
        if (audioSourceMusica != null) audioSourceMusica.Play();
    }
}