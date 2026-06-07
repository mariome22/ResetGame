using UnityEngine;

public class HazardPlatformer : MonoBehaviour
{
    [Header("Ajustes de Daño")]
    [Tooltip("Cantidad de vidas que restará al jugador (por defecto 1).")]
    public int damage = 1;

    [Tooltip("Si se activa, el jugador morirá instantáneamente al entrar en contacto (ignora vidas restantes).")]
    public bool instantKill = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcesarDaño(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarDaño(collision.gameObject);
    }

    private void ProcesarDaño(GameObject objeto)
    {
        // Verificar si es el jugador
        PlayerPlatformerController player = objeto.GetComponent<PlayerPlatformerController>();
        if (player != null)
        {
            if (instantKill)
            {
                // Para matar instantáneamente, infligimos daño equivalente a su salud máxima o vidas
                player.TakeDamage(999);
            }
            else
            {
                player.TakeDamage(damage);
            }
        }
    }
}
