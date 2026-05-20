using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float puntoSalePantalla = -14.719f;
    [SerializeField] private float puntoReinicia = 22.58f;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x <= puntoSalePantalla)
        {
            transform.position = new Vector3(puntoReinicia, transform.position.y, transform.position.z);
        }
    }
}
