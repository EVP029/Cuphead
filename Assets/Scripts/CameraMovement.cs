using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform seguirPersonaje;
    public float suavidad = 3.5f;

    //  Límites del mapa
    public float minX, maxX;
    public float minY, maxY;

    private void LateUpdate()
    {
        float x = Mathf.Lerp(transform.position.x, seguirPersonaje.position.x, suavidad * Time.deltaTime);

        float y = Mathf.Lerp(transform.position.y, seguirPersonaje.position.y, suavidad * Time.deltaTime);

        //  Limitar cámara
        x = Mathf.Clamp(x, minX, maxX);
        y = Mathf.Clamp(y, minY, maxY);

        transform.position = new Vector3(x, y, transform.position.z);
    }
}

