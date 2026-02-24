using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int cantidadDano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Buscamos el componente de vida en el jugador
            PlayerHealth saludJugador = other.GetComponent<PlayerHealth>();

            // Si lo encontramos, le hacemos daño
            if (saludJugador != null)
            {
                saludJugador.RecibirDano(cantidadDano);
            }
        }
    }
}