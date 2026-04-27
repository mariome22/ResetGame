using UnityEngine;
using System.Collections;

public class EnemyMelee : MonoBehaviour
{
    [Header("Movimiento Base (Slime)")]
    public float velocidadNormal = 2f;
    public float rangoDeteccion = 10f;

    [Header("Ajustes de Embestida")]
    public bool haceEmbestidas = false;
    public float rangoEmbestida = 4f;
    public float velocidadEmbestida = 8f;
    public float tiempoPreparacion = 0.5f;
    public float tiempoRecargaEmbestida = 2f;
    [Tooltip("Color que tomara para avisar del ataque")]
    public Color colorAvisoEmbestida = Color.red;

    [Header("Ajustes de Explosion")]
    public bool explota = false;
    public float rangoActivacionExplosion = 1.5f;
    public float tiempoParaExplotar = 1f;
    public int danoExplosion = 2;
    [Tooltip("Fuerza con la que vibra antes de explotar")]
    public float intensidadTemblor = 0.05f;

    [Header("Dano de Contacto")]
    public int danoPorContacto = 1;

    [Header("Comportamiento Zombie")]
    public bool esZombie = false;
    
    [Header("Fase 1: Tambaleo")]
    [Tooltip("Magnitud del tambaleo lateral")]
    public float amplitudTambaleo = 0.5f;
    [Tooltip("Velocidad a la que oscila el tambaleo")]
    public float velocidadTambaleo = 10f;

    [Header("Fase 2: Pasos Rápidos")]
    [Tooltip("Probabilidad por segundo de dar un paso rápido")]
    public float probabilidadPasoRapido = 0.2f;
    public float multiplicadorPasoRapido = 2f;
    public float duracionPasoRapido = 0.5f;

    [Header("Fase 3: Frenesí")]
    public float distanciaFrenesi = 3f;
    public float multiplicadorFrenesi = 1.5f;

    private float tiempoPasoRapidoRestante = 0f;

    [Header("Context Steering")]
    public LayerMask obstacleLayer;
    public float distanciaRaycast = 1f;
    [Tooltip("El radio del grosor del enemigo para no atascarse en las esquinas")]
    public float radioObstaculos = 0.4f;

    private Transform jugador;
    private bool estaOcupado = false;
    private bool puedeEmbestir = true;
    private bool estaEmbistiendo = false;
    private bool explosionIniciada = false;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
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
        if (objJugador != null) jugador = objJugador.transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (jugador == null || estaOcupado) 
        {
            if (rb != null && !estaEmbistiendo) rb.linearVelocity = Vector2.zero;
            return;
        }

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia <= rangoDeteccion)
        {
            if (explota && distancia <= rangoActivacionExplosion && !explosionIniciada)
            {
                explosionIniciada = true;
                StartCoroutine(RutinaExplosion());
            }
            
            if (haceEmbestidas && distancia <= rangoEmbestida && puedeEmbestir && !explosionIniciada)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                StartCoroutine(RutinaEmbestida());
            }
            else
            {
                MoverConContextSteering();
            }
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
            
            // Usamos CircleCast en lugar de Raycast para tener en cuenta el ancho del enemigo
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radioObstaculos, dir, distanciaRaycast, obstacleLayer);

            if (hit.collider != null)
            {
                Debug.DrawRay(transform.position, dir * distanciaRaycast, Color.red);
            }
            else
            {
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
            float velocidadActual = velocidadNormal;
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

    private IEnumerator RutinaEmbestida()
    {
        estaOcupado = true;
        puedeEmbestir = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null) spriteRenderer.color = colorAvisoEmbestida;
        yield return new WaitForSeconds(tiempoPreparacion);

        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
        Vector2 posicionObjetivo = jugador.position;
        estaEmbistiendo = true;

        while (Vector2.Distance(transform.position, posicionObjetivo) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, posicionObjetivo, velocidadEmbestida * Time.deltaTime);
            yield return null;
        }
        estaEmbistiendo = false;
        estaOcupado = false;

        yield return new WaitForSeconds(tiempoRecargaEmbestida);
        puedeEmbestir = true;
    }

    private IEnumerator RutinaExplosion()
    {
        // Ya no ponemos estaOcupado = true, para que MoverConContextSteering se siga llamando
        float tiempoPasado = 0f;

        // Parpadeo rojo en lugar de temblor físico para no interrumpir el movimiento
        while (tiempoPasado < tiempoParaExplotar)
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            tiempoPasado += 0.2f;
        }

        if (jugador != null)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(2.5f);
            float distanciaFinal = Vector2.Distance(transform.position, jugador.position);
            // Ampliamos un poco el rango real de explosión para ser justos si el enemigo sigue andando
            if (distanciaFinal <= rangoActivacionExplosion * 1.5f)
            {
                jugador.GetComponent<PlayerHealth>().RecibirDano(danoExplosion);
            }
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (explota) return;
            if (haceEmbestidas && !estaEmbistiendo) return;
            collision.gameObject.GetComponent<PlayerHealth>().RecibirDano(danoPorContacto);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        if (haceEmbestidas)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangoEmbestida);
        }

        if (explota)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, rangoActivacionExplosion);
        }

        if (esZombie)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // Naranja
            Gizmos.DrawWireSphere(transform.position, distanciaFrenesi);
        }
    }

    public void ResetearAtaque()
    {
        StopAllCoroutines();
        estaOcupado = false;
        estaEmbistiendo = false;
        puedeEmbestir = true;
        explosionIniciada = false;

        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
    }
}
