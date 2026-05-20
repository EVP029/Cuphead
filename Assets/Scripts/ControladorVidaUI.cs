using UnityEngine;
using UnityEngine.UI;

public class ControladorVidaUI : MonoBehaviour
{
    [Header("Componentes de UI")]
    public Image imagenHP;         // El objeto HP_Display
    public Animator animatorHP;    // El Animator de HP_Display

    [Header("Sprites de Vida")]
    public Sprite spriteVida3;
    public Sprite spriteVida2;
    public Sprite spriteVida1;     
    public Sprite spriteMuerto;

    void Start()
    {
        // Al empezar el juego, nos aseguramos de que el Animator esté apagado
        // para que no bloquee el cambio de sprites del script
        if (animatorHP != null)
        {
            animatorHP.enabled = false;
        }
    }

    public void ActualizarVidasUI(int vidasActuales)
    {
        switch (vidasActuales)
        {
            case 3:
                if (animatorHP != null) animatorHP.enabled = false; // Apagado
                imagenHP.sprite = spriteVida3;
                break;

            case 2:
                if (animatorHP != null) animatorHP.enabled = false; // Apagado
                imagenHP.sprite = spriteVida2;
                break;

            case 1:
                // 1. Ponemos primero el sprite base de 1 vida
                imagenHP.sprite = spriteVida1;
                
                // 2. ¡ENCENDEMOS EL ANIMATOR! Ahora sí tiene permiso de controlar los sprites
                if (animatorHP != null)
                {
                    animatorHP.enabled = true;
                    animatorHP.SetBool("EnPeligro", true); // Activa el parpadeo
                }
                break;

            default:
                if (animatorHP != null) animatorHP.enabled = false; // Apagado
                imagenHP.sprite = spriteMuerto;
                break;
        }
    }
}
