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
    [Tooltip("Color que tomará para avisar del ataque")]
    public Color colorAvisoEmbestida = Color.red;

    [Header("Ajustes de Explosión")]
    public bool explota = false;
    public float rangoActivacionExplosion = 1.5f;
    public float tiempoParaExplotar = 1f;
    public int danoExplosion = 2;
    [Tooltip("Fuerza con la que vibra antes de explotar")]
    public float intensidadTemblor = 0.05f;

    [Header("Daño de Contacto")]
    public int danoPorContacto = 1;

    private Transform jugador;
    private bool estaOcupado = false;
    private bool puedeEmbestir = true;
    private bool estaEmbistiendo = false;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null) jugador = objJugador.transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
    }

    private void Update()
    {
        if (jugador == null || estaOcupado) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia <= rangoDeteccion)
        {
            if (explota && distancia <= rangoActivacionExplosion)
            {
                StartCoroutine(RutinaExplosion());
            }
            else if (haceEmbestidas && distancia <= rangoEmbestida && puedeEmbestir)
            {
                StartCoroutine(RutinaEmbestida());
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, jugador.position, velocidadNormal * Time.deltaTime);
            }
        }
    }

    private IEnumerator RutinaEmbestida()
    {
        estaOcupado = true;
        puedeEmbestir = false;

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