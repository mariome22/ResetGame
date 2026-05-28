using UnityEngine;

public class EnemyProjectilePlatformer : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 1;
    public float lifetime = 3f;
    [Tooltip("Capas con las que el proyectil chocará y se destruirá (Muros, Suelo, etc.)")]
    public LayerMask collisionLayers;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    public void Fire(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;
        transform.right = direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerPlatformerController player = collision.GetComponent<PlayerPlatformerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
