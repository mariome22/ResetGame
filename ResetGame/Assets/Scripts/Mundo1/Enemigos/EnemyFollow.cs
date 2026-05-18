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

    [Header("Comportamiento Zombie")]
    public bool esZombie = false;
    
    [Header("Fase 1: Tambaleo")]
    [Tooltip("Magnitud del tambaleo lateral")]
    public float amplitudTambaleo = 0.5f;
    [Tooltip("Velocidad a la que oscila el tambaleo")]
    public float velocidadTambaleo = 2f;

    [Header("Fase 2: Pasos Rápidos")]
    [Tooltip("Probabilidad por segundo de dar un paso rápido")]
    public float probabilidadPasoRapido = 0.2f;
    public float multiplicadorPasoRapido = 2f;
    public float duracionPasoRapido = 0.5f;

    [Header("Fase 3: Frenesí")]
    public float distanciaFrenesi = 3f;
    public float multiplicadorFrenesi = 1.5f;

    private float tiempoPasoRapidoRestante = 0f;

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
            float velocidadActual = velocidad;
            Vector2 direccionFinal = mejorDireccion;

            if (esZombie)
            {
                // Fase 3: Frenesí
                float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
                if (distanciaAlJugador <= distanciaFrenesi)
                {
                    velocidadActual *= multiplicadorFrenesi;
                }
                else
                {
                    // Fase 2: Pasos Rápidos
                    if (tiempoPasoRapidoRestante > 0)
                    {
                        tiempoPasoRapidoRestante -= Time.fixedDeltaTime;
                        velocidadActual *= multiplicadorPasoRapido;
                    }
                    else if (Random.value < probabilidadPasoRapido * Time.fixedDeltaTime)
                    {
                        tiempoPasoRapidoRestante = duracionPasoRapido;
                    }
                }

                // Fase 1: Tambaleo
                Vector2 perpendicular = new Vector2(-mejorDireccion.y, mejorDireccion.x);
                float factorTambaleo = Mathf.Sin(Time.time * velocidadTambaleo) * amplitudTambaleo;
                
                direccionFinal = (mejorDireccion + perpendicular * factorTambaleo).normalized;
            }

            Debug.DrawRay(transform.position, direccionFinal * distanciaRaycast, Color.green);
            rb.linearVelocity = direccionFinal * velocidadActual;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
