using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject prefabProyectil; // Arrastra aqui el prefab de tu bala
    public float ritmoDeDisparo = 2f;
    public float rangoDeVision = 7f;

    private Transform jugador;
    private float proximoDisparoTiempo;

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
        if (jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        // Si esta en rango y ha pasado el tiempo de recarga
        if (distancia <= rangoDeVision && Time.time >= proximoDisparoTiempo)
        {
            Disparar();
            proximoDisparoTiempo = Time.time + ritmoDeDisparo;
        }
    }

    private void Disparar()
    {
        Vector2 direccion = (jugador.position - transform.position).normalized;
        GameObject bala = Instantiate(prefabProyectil, transform.position, Quaternion.identity);

        // Llama al script del proyectil para darle la direccion
        EnemyProjectile scriptBala = bala.GetComponent<EnemyProjectile>();
        if (scriptBala != null)
        {
            scriptBala.Disparar(direccion);
        }
    }

    // Dibuja el area de vision en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeVision);
    }
}