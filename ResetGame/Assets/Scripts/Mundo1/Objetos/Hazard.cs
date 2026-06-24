using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Configuracion de Dano")]
    public int cantidadDano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth saludJugador = other.GetComponent<PlayerHealth>();

            if (saludJugador != null)
            {
                saludJugador.RecibirDano(cantidadDano, transform.position);
            }
        }
    }
}
