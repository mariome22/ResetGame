using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    [Tooltip("Tiempo en segundos antes de que el proyectil se destruya automáticamente si no choca contra nada")]
    public float tiempoDeVida = 3f;

    public void Inicializar(Vector2 direccion, float velocidad)
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Asignamos la velocidad en la dirección dada
        rb.linearVelocity = direccion.normalized * velocidad;

        // Opcional: rotar el proyectil para que mire en la dirección del movimiento
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);

        // Iniciar la destrucción automática por si se pierde en el vacío
        StartCoroutine(DestruirPorTiempo());
    }

    private IEnumerator DestruirPorTiempo()
    {
        yield return new WaitForSeconds(tiempoDeVida);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Evitamos chocar con el propio jugador
        if (collider.CompareTag("Player")) return;

        // Comprobamos si hemos chocado con un enemigo
        if (collider.CompareTag("Enemy"))
        {
            EnemyBase scriptEnemigo = collider.GetComponent<EnemyBase>();
            if (scriptEnemigo != null)
            {
                scriptEnemigo.RecibirDano(1);
            }
        }
        else
        {
            // Comprobamos si hemos chocado con un objeto rompible
            ObjetoRompible caja = collider.GetComponent<ObjetoRompible>();
            if (caja != null)
            {
                caja.RecibirDano(1);
            }
        }

        // Destruimos el proyectil al chocar con CUALQUIER otra cosa que no sea el player
        Destroy(gameObject);
    }
}
