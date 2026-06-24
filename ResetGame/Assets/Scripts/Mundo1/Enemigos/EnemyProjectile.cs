using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float velocidad = 7f;
    public int dano = 1;
    public float tiempoDeVida = 3f;
    [Tooltip("Capas con las que el proyectil chocará y se destruirá (Muros, Mesas, etc.)")]
    public LayerMask capasColision;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, tiempoDeVida);
    }

    public void Disparar(Vector2 direccion)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direccion.normalized * velocidad;
        
        // Orientar el sprite hacia la dirección en la que viaja
        // (Asume que el sprite de la bala está dibujado mirando hacia la derecha)
        transform.right = direccion;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Impacto contra el jugador. Pierde " + dano + " vida.");
            collision.GetComponent<PlayerHealth>().RecibirDano(dano, transform.position);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall") || ((capasColision.value & (1 << collision.gameObject.layer)) > 0))
        {
            // Se destruye si tiene la etiqueta Wall O si pertenece a una capa marcada en capasColision
            Destroy(gameObject);
        }
    }
}