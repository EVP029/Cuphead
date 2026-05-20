using UnityEngine;
using UnityEngine.UI;

public class SelectorCharacterBG : MonoBehaviour
{
    public Image fondoSeleccion;

    public Sprite fondoCuphead;
    public Sprite fondoMugman;

    public void SeleccionarCuphead() {
        fondoSeleccion.sprite = fondoCuphead;
    }

    public void SeleccionarMugman() {
        fondoSeleccion.sprite = fondoMugman;
    }
}