using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseManager_02 : MonoBehaviour {

    public GameObject pausePanel;

    public GameObject botonResume;

    private bool pausado = false;

    void Update() {

        bool presionoPause = false;

        // ESC = abrir pausa
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame) {

            presionoPause = true;
        }

        // START control = abrir pausa
        if (Gamepad.current != null &&
            Gamepad.current.startButton.wasPressedThisFrame) {

            presionoPause = true;
        }

        // ABRIR PAUSA
        if (presionoPause && !pausado) {

            Pause();
        }
    }

    public void Pause() {

        pausePanel.SetActive(true);

        Time.timeScale = 0f;

        pausado = true;

        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(botonResume);
    }

    public void Resume() {

        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        pausado = false;
    }
}