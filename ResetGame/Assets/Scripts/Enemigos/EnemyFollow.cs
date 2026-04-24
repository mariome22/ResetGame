using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Context Steering")]
    public LayerMask obstacleLayer;
    public float distanciaRaycast = 1f;
    [Tooltip("El radio del grosor del enemigo para no atascarse en las esquinas")]
    public float radioObstaculos = 0.4f;

    [Header("Movimiento")]
    public float velocidad = 2f;

    private Transform jugador;
    private Rigidbody2D rb;
    private Vector2[] direcciones = new Vector2[]
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
    };

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (jugador != null)
        {
            MoverConContextSteering();
        }
        else
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    private void MoverConContextSteering()
    {
        if (rb == null) return;

        Vector2 direccionAlObjetivo = (jugador.position - transform.position).normalized;
        Vector2 mejorDireccion = Vector2.zero;
        float mejorDot = -Mathf.Infinity;

        for (int i = 0; i < 8; i++)
        {
            Vector2 dir = direcciones[i];
            
            // CircleCast simula el grosor del enemigo
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radioObstaculos, dir, distanciaRaycast, obstacleLayer);

            if (hit.collider != null)
            {
                // Direccion bloqueada
                Debug.DrawRay(transform.position, dir * distanciaRaycast, Color.red);
            }
            else
            {
                // Direccion libre
                float dot = Vector2.Dot(dir, direccionAlObjetivo);
                if (dot > mejorDot)
                {
                    mejorDot = dot;
                    mejorDireccion = dir;
                }
            }
        }

        if (mejorDireccion != Vector2.zero)
        {
            Debug.DrawRay(transform.position, mejorDireccion * distanciaRaycast, Color.green);
            rb.linearVelocity = mejorDireccion * velocidad;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
