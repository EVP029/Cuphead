using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 1.5f;
    private float direction = 1f; // 1 derecha, -1 izquierda

    public void SetDirection(float dir)
    {
        direction = dir;

        // Volteamos el sprite de la bala si va a la izquierda
        if (dir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Movimiento corregido: se multiplica por la dirección
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca con el suelo (asegúrate de que el suelo tenga la Layer "Ground")
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
        
        if (collision.CompareTag("Enemy"))
        {
            // Aquí irá el daño al enemigo
            Destroy(gameObject);
        }
    }
}