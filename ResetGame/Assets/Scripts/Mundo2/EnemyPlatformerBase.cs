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
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Die"); // Lanza la animación de muerte
            
            // Congelamos al enemigo y desactivamos sus colisiones
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false; 
            
            // Desactivamos el script para que deje de patrullar o atacar
            this.enabled = false;

            // Destruimos el objeto después de 0.6 segundos (ajustable)
            Destroy(gameObject, 0.8f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public virtual void TurnAround()
    {
        // Para ser sobrescrito por las clases hijas
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Rebotar si chocamos contra otro enemigo de plataformas
        if (collision.gameObject.GetComponent<EnemyPlatformerBase>() != null)
        {
            TurnAround();
            return;
        }

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
