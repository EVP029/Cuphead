using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject interactIcon;

    private void Start()
    {
        interactIcon.SetActive(false);
    }

    public void ShowIcon()
    {
        interactIcon.SetActive(true);
    }

    public void HideIcon()
    {
        interactIcon.SetActive(false);
    }
}