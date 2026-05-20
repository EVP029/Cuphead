using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour {
    public GameObject pausePanel;

    public GameObject botonResume;

    private bool pausado = false;

    void Update() {
        bool presionoPause = false;

        bool presionoResume = false;

        // P = abrir/cerrar pausa
        if (Keyboard.current != null &&
            Keyboard.current.pKey.wasPressedThisFrame) {
            presionoPause = true;
        }

        // SPACE = solo cerrar pausa
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame) {
            presionoResume = true;
        }

        // START control = abrir/cerrar pausa
        if (Gamepad.current != null &&
            Gamepad.current.startButton.wasPressedThisFrame) {
            presionoPause = true;
        }

        // BOTÓN B = cerrar pausa
        if (Gamepad.current != null &&
            Gamepad.current.buttonEast.wasPressedThisFrame) {
            presionoResume = true;
        }

        // ABRIR / CERRAR
        if (presionoPause) {
            if (pausado)
                Resume();
            else
                Pause();
        }

        // SOLO CERRAR
        if (presionoResume && pausado) {
            Resume();
        }
    }
    public void Pause() {
        pausePanel.SetActive(true);

        Time.timeScale = 0f;

        pausado = true;

        EventSystem.current.SetSelectedGameObject(botonResume);
    }

    public void Resume() {
        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        pausado = false;
    }
}