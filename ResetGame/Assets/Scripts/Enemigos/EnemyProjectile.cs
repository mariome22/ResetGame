using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float velocidad = 7f;
    public int dano = 1;
    public float tiempoDeVida = 3f;

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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Impacto contra el jugador. Pierde " + dano + " vida.");
            collision.GetComponent<PlayerHealth>().RecibirDano(dano);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}