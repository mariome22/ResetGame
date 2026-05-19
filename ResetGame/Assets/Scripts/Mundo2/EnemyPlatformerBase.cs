using UnityEngine;

public class EnemyPlatformerBase : MonoBehaviour
{
    [Header("Ajustes Base")]
    public int maxHealth = 1;
    protected int currentHealth;
    public int damageToPlayer = 1;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Aquí puedes añadir más adelante partículas o un sonido de "plop" al morir
        Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerPlatformerController player = collision.gameObject.GetComponent<PlayerPlatformerController>();
            if (player != null)
            {
                // Obtenemos el punto de contacto para saber desde dónde chocó el jugador
                ContactPoint2D contact = collision.GetContact(0);
                
                // La "normal" es la dirección perpendicular a la superficie de choque.
                // Si la normal.y es negativa (apunta hacia abajo), el jugador chocó desde arriba.
                if (contact.normal.y <= -0.5f)
                {
                    // El jugador pisó la cabeza del enemigo
                    player.Bounce();
                    TakeDamage(maxHealth); // El enemigo muere
                }
                else
                {
                    // Choque lateral o desde abajo
                    player.TakeDamage(damageToPlayer);
                }
            }
        }
    }
}
