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

    [Header("Context Steering")]
    public LayerMask obstacleLayer;
    public float distanciaRaycast = 1f;
    [Tooltip("El radio del grosor del enemigo para no atascarse en las esquinas")]
    public float radioObstaculos = 0.4f;

    private Transform jugador;
    private bool estaOcupado = false;
    private bool puedeEmbestir = true;
    private bool estaEmbistiendo = false;

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
            if (explota && distancia <= rangoActivacionExplosion)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                StartCoroutine(RutinaExplosion());
            }
            else if (haceEmbestidas && distancia <= rangoEmbestida && puedeEmbestir)
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
            Debug.DrawRay(transform.position, mejorDireccion * distanciaRaycast, Color.green);
            rb.linearVelocity = mejorDireccion * velocidadNormal;
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
        estaOcupado = true;
        float tiempoPasado = 0f;
        Vector3 posicionBase = transform.position;

        while (tiempoPasado < tiempoParaExplotar)
        {
            transform.position = posicionBase + (Vector3)Random.insideUnitCircle * intensidadTemblor;
            tiempoPasado += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        if (jugador != null)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(2.5f);
            float distanciaFinal = Vector2.Distance(transform.position, jugador.position);
            if (distanciaFinal <= rangoActivacionExplosion)
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
    }

    public void ResetearAtaque()
    {
        StopAllCoroutines();
        estaOcupado = false;
        estaEmbistiendo = false;
        puedeEmbestir = true;

        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
    }
}
