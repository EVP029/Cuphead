using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/*
    Controla el menú de la tacita tirada
        + Menú principal
        + Menú partidas
        + Menú personajes 

    Versión para Teclado (Sin Mouse)
*/

public class MenuPrincipal : MonoBehaviour {
    public GameObject panelPrincipal;
    public GameObject panelTarjetitas;
    public GameObject panelPersonajes01;
    public GameObject panelPersonajes02;
    public GameObject panelPersonajes03;

    public GameObject botonInicialMenu;
    public GameObject botonInicialTarjetitas;
    public GameObject botonInicialPersonajes01;
    public GameObject botonInicialPersonajes02;
    public GameObject botonInicialPersonajes03;

    private enum EstadoMenu {
        Principal,
        Tarjetitas,
        Personajes
    }

    private EstadoMenu estadoActual = EstadoMenu.Principal;

    void Update() {
        bool presionoB = false;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            presionoB = true;

        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            presionoB = true;

        if (presionoB) {
            ManejarRegreso();
        }
    }

    public void IrATarjetitas() {
        panelPrincipal.SetActive(false);
        panelTarjetitas.SetActive(true);

        estadoActual = EstadoMenu.Tarjetitas;

        EventSystem.current.SetSelectedGameObject(botonInicialTarjetitas);
    }

    public void IrAPersonajes01() {
        panelTarjetitas.SetActive(false);
        panelPersonajes01.SetActive(true);

        estadoActual = EstadoMenu.Personajes;

        EventSystem.current.SetSelectedGameObject(botonInicialPersonajes01);
    }

    public void IrAPersonajes02() {
        panelTarjetitas.SetActive(false);
        panelPersonajes02.SetActive(true);

        estadoActual = EstadoMenu.Personajes;

        EventSystem.current.SetSelectedGameObject(botonInicialPersonajes02);
    }

    public void IrAPersonajes03() {
        panelTarjetitas.SetActive(false);
        panelPersonajes03.SetActive(true);

        estadoActual = EstadoMenu.Personajes;

        EventSystem.current.SetSelectedGameObject(botonInicialPersonajes03);
    }

    void ManejarRegreso() {

        // SI ESTÁ EN PERSONAJES
        if (panelPersonajes01.activeSelf ||
            panelPersonajes02.activeSelf ||
            panelPersonajes03.activeSelf) {

            panelPersonajes01.SetActive(false);
            panelPersonajes02.SetActive(false);
            panelPersonajes03.SetActive(false);

            panelTarjetitas.SetActive(true);

            EventSystem.current.SetSelectedGameObject(botonInicialTarjetitas);

            return;
        }

        // SI ESTÁ EN TARJETITAS
        if (panelTarjetitas.activeSelf) {

            panelTarjetitas.SetActive(false);

            panelPrincipal.SetActive(true);

            EventSystem.current.SetSelectedGameObject(botonInicialMenu);

            return;
        }
    }
}