using UnityEngine;
using System.Collections;

public class EnemyMelee : MonoBehaviour
{
    [Header("Movimiento Base (Slime)")]
    public float velocidadNormal = 2f;
    public float rangoDeteccion = 10f; // Si estás más lejos que esto, se queda quieto esperando

    [Header("Ajustes de Embestida")]
    public bool haceEmbestidas = false;
    public float rangoEmbestida = 4f; // A qué distancia decide lanzarse
    public float velocidadEmbestida = 8f;
    public float tiempoPreparacion = 0.5f; // Pausa dramática antes de volar hacia ti
    public float tiempoRecargaEmbestida = 2f;

    [Header("Ajustes de Explosión")]
    public bool explota = false;
    public float rangoActivacionExplosion = 1.5f; // Cuando estés a esta distancia, inicia la cuenta atrás
    public float tiempoParaExplotar = 1f; // Tiempo que parpadea antes de hacer BOOM
    public int danoExplosion = 2; // Cuánto duele la explosión

    [Header("Daño de Contacto")]
    public int danoPorContacto = 1;

    private Transform jugador;
    private bool estaOcupado = false; // Bloquea el movimiento normal si está embistiendo o explotando
    private bool puedeEmbestir = true;
    private bool estaEmbistiendo = false; // Para saber si el murciélago está en pleno vuelo

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null) jugador = objJugador.transform;
    }

    private void Update()
    {
        if (jugador == null || estaOcupado) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        // Solo se mueve si el jugador está dentro de su radar
        if (distancia <= rangoDeteccion)
        {
            // Prioridad 1: Explotar (Kamikaze)
            if (explota && distancia <= rangoActivacionExplosion)
            {
                StartCoroutine(RutinaExplosion());
            }
            // Prioridad 2: Embestir (Murciélago)
            else if (haceEmbestidas && distancia <= rangoEmbestida && puedeEmbestir)
            {
                StartCoroutine(RutinaEmbestida());
            }
            // Prioridad 3: Perseguir normal (Slime)
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

        // 1. Telegrafiado (se para, avisa de que va a atacar)
        Debug.Log(gameObject.name + " se prepara para embestir...");
        yield return new WaitForSeconds(tiempoPreparacion);

        // 2. Guarda la posición a la que va a volar
        Vector2 posicionObjetivo = jugador.position;

        // 3. El vuelo rápido
        estaEmbistiendo = true;
        while (Vector2.Distance(transform.position, posicionObjetivo) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, posicionObjetivo, velocidadEmbestida * Time.deltaTime);
            yield return null;
        }
        estaEmbistiendo = false;

        estaOcupado = false;

        // 4. Recarga de la habilidad
        yield return new WaitForSeconds(tiempoRecargaEmbestida);
        puedeEmbestir = true;
    }

    private IEnumerator RutinaExplosion()
    {
        estaOcupado = true; // Frena en seco al activar la bomba

        // 1. Telegrafiado (Aviso visual para el jugador)
        Debug.Log(gameObject.name + " va a EXPLOTAR en " + tiempoParaExplotar + " segundos!");
        yield return new WaitForSeconds(tiempoParaExplotar);

        // 2. Comprueba si el jugador no escapó a tiempo del área
        if (jugador != null)
        {
            float distanciaFinal = Vector2.Distance(transform.position, jugador.position);
            if (distanciaFinal <= rangoActivacionExplosion)
            {
                Debug.Log("¡BOOM! Null recibe daño por explosión.");
                // Descomenta la siguiente línea y pon tu script de vida:
                jugador.GetComponent<PlayerHealth>().RecibirDano(danoExplosion); 
            }
        }

        // 3. Autodestrucción
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si chocamos contra Null
        if (collision.gameObject.CompareTag("Player"))
        {
            // Regla 1: Si es un Kamikaze, el contacto físico normal no hace daño
            if (explota) return;

            // Regla 2: Si es un Murciélago y NO está embistiendo, no hace daño al rozarlo
            if (haceEmbestidas && !estaEmbistiendo) return;

            // Si llegamos aquí, es un Slime normal o un Murciélago en plena embestida
            Debug.Log("¡Impacto físico! " + gameObject.name + " le hace " + danoPorContacto + " de daño a Null.");

            // Descomenta la siguiente línea y pon tu script de vida:
            collision.gameObject.GetComponent<PlayerHealth>().RecibirDano(danoPorContacto);
        }
    }

    private void OnDrawGizmosSelected()
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
}