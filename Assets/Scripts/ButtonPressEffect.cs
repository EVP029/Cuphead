using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressEffect : MonoBehaviour, ISubmitHandler
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log("Submit detectado");
        StartCoroutine(PressEffect());
    }

    IEnumerator PressEffect()
    {
        transform.localScale = originalScale * 0.95f;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = originalScale;
    }
}