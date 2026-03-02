using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Estadisticas")]
    public int vidaMaxima = 3;
    private int vidaActual;

    private void Start()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;
        Debug.Log("Dano recibido en " + gameObject.name + ". Vida: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Debug.Log("Muere " + gameObject.name);
        Destroy(gameObject);
    }
}