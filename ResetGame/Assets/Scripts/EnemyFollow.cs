using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;

    private Transform jugador;

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
        }
    }

    private void Update()
    {
        if (jugador != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, jugador.position, velocidad * Time.deltaTime);
        }
    }
}