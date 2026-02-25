using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public int vidaMaxima = 3;
    private int vidaActual;

    [Header("Movimiento")]
    public float velocidad = 2f;
    private Transform objetivo; // El jugador al que va a perseguir

    private void Start()
    {
        vidaActual = vidaMaxima;

        // Al nacer, busca automáticamente a Null usando su Tag
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            objetivo = jugador.transform;
        }
    }

    private void Update()
    {
        // IA súper simple: Si el jugador existe, camina hacia él
        if (objetivo != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, objetivo.position, velocidad * Time.deltaTime);
        }
    }

    // Esta función la llamará la espada de Null
    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;
        Debug.Log("¡" + gameObject.name + " recibió " + cantidadDano + " de daño! Vida: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Debug.Log(gameObject.name + " ha muerto.");
        // Destruimos el objeto para que desaparezca de la escena
        Destroy(gameObject);
    }
}