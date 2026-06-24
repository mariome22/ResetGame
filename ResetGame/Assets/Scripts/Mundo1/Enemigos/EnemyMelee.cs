using UnityEngine;
using System.Collections;

public class EnemyMelee : MonoBehaviour
{
    [Header("Movimiento Base (Slime)")]
    public float velocidadNormal = 2f;
    public float rangoDeteccion = 10f;
    [Tooltip("Capas que bloquean la visión (ej. Muros, pero no Coches)")]
    public LayerMask capaBloqueoVision;

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

    public enum EstadoEnemigo { Patrullando, Persiguiendo, Regresando }

    [Header("Sistema de Patrulla")]
    public Transform[] puntosPatrulla;
    public float tiempoEsperaPatrulla = 1f;
    public float velocidadPatrulla = 1.5f;
    [Tooltip("Distancia a la que deja de perseguir al jugador (debe ser mayor que el rango de detección)")]
    public float rangoPerdida = 15f;

    private EstadoEnemigo estadoActual = EstadoEnemigo.Patrullando;
    private int indicePatrullaActual = 0;
    private float tiempoEsperaRestante = 0f;
    private float tiempoAtascado = 0f;
    private Vector2 ultimaPosicionAtasco;
    private Vector2 posicionInicial;
    private float offsetTambaleo;

    [Header("Dano de Contacto")]
    public int danoPorContacto = 1;

    [Header("Ajustes de Ataque Zombie")]
    public float rangoAtaqueZombie = 1.5f;
    public float tiempoEntreAtaquesZombie = 1.5f;

    private bool puedeAtacarZombie = true;
    private Animator anim;
    private bool tieneParametroAtacar = false;
    private bool tieneParametroCharging = false;
    private bool tieneParametroMuerte = false;

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
        anim = GetComponent<Animator>();

        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Atacar") tieneParametroAtacar = true;
                if (param.name == "isCharging") tieneParametroCharging = true;
                if (param.name == "Muerte") tieneParametroMuerte = true;
            }
        }

        posicionInicial = transform.position;
        ultimaPosicionAtasco = transform.position;
        offsetTambaleo = Random.Range(0f, 1000f);
    }

    private void FixedUpdate()
    {
        if (jugador == null || estaOcupado) 
        {
            if (rb != null && !estaEmbistiendo) rb.linearVelocity = Vector2.zero;
            return;
        }

        float distancia = Vector2.Distance(transform.position, jugador.position);
        bool tieneLineaVision = VerificarLineaVision(distancia);

        switch (estadoActual)
        {
            case EstadoEnemigo.Patrullando:
                if (distancia <= rangoDeteccion && tieneLineaVision)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                }
                else
                {
                    EjecutarPatrulla();
                }
                break;

            case EstadoEnemigo.Persiguiendo:
                if (distancia > rangoPerdida || !tieneLineaVision)
                {
                    estadoActual = EstadoEnemigo.Regresando;
                    tiempoAtascado = 0f;
                    ultimaPosicionAtasco = transform.position;
                }
                else
                {
                    EjecutarPersecucion(distancia);
                }
                break;

            case EstadoEnemigo.Regresando:
                if (distancia <= rangoDeteccion && tieneLineaVision)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                }
                else
                {
                    EjecutarRegreso();
                }
                break;
        }
    }

    private bool VerificarLineaVision(float distancia)
    {
        if (jugador == null) return false;
        
        Vector2 direccion = (jugador.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, distancia, capaBloqueoVision);
        
        return hit.collider == null;
    }

    private void EjecutarPersecucion(float distancia)
    {
        if (explota && distancia <= rangoActivacionExplosion && !explosionIniciada)
        {
            explosionIniciada = true;
            StartCoroutine(RutinaExplosion());
            return;
        }
        
        if (haceEmbestidas && distancia <= rangoEmbestida && puedeEmbestir && !explosionIniciada)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            StartCoroutine(RutinaEmbestida());
            return;
        }

        if (esZombie && distancia <= rangoAtaqueZombie && puedeAtacarZombie && !estaOcupado && !explosionIniciada)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            StartCoroutine(RutinaAtaqueZombie());
            return;
        }
        
        if (!estaOcupado || explosionIniciada) 
        {
            MoverConContextSteering(jugador.position, velocidadNormal, true);
        }
    }

    private void EjecutarPatrulla()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            // Sin puntos, intenta volver a donde nació o se queda quieto si ya está allí
            if (Vector2.Distance(transform.position, posicionInicial) > 0.5f)
                MoverConContextSteering(posicionInicial, velocidadPatrulla, false);
            else
                if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (tiempoEsperaRestante > 0)
        {
            tiempoEsperaRestante -= Time.fixedDeltaTime;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform objetivoActual = puntosPatrulla[indicePatrullaActual];
        if (objetivoActual == null) return;

        if (Vector2.Distance(transform.position, objetivoActual.position) < 0.5f)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            tiempoEsperaRestante = tiempoEsperaPatrulla;
            indicePatrullaActual++;
            if (indicePatrullaActual >= puntosPatrulla.Length) indicePatrullaActual = 0;
        }
        else
        {
            MoverConContextSteering(objetivoActual.position, velocidadPatrulla, false);
        }
    }

    private void EjecutarRegreso()
    {
        Vector2 objetivoRegreso = posicionInicial;
        if (puntosPatrulla != null && puntosPatrulla.Length > 0 && puntosPatrulla[indicePatrullaActual] != null)
        {
            objetivoRegreso = puntosPatrulla[indicePatrullaActual].position;
        }

        if (Vector2.Distance(transform.position, objetivoRegreso) < 0.5f)
        {
            estadoActual = EstadoEnemigo.Patrullando;
            return;
        }

        MoverConContextSteering(objetivoRegreso, velocidadPatrulla, false);

        // Sistema Anti-atasco
        if (Vector2.Distance(transform.position, ultimaPosicionAtasco) > 0.5f)
        {
            ultimaPosicionAtasco = transform.position;
            tiempoAtascado = 0f;
        }
        else
        {
            tiempoAtascado += Time.fixedDeltaTime;
            if (tiempoAtascado >= 3f)
            {
                transform.position = objetivoRegreso;
                estadoActual = EstadoEnemigo.Patrullando;
                tiempoAtascado = 0f;
            }
        }
    }

    private void MoverConContextSteering(Vector2 objetivo, float velocidadBase, bool esPersecucion)
    {
        if (rb == null) return;

        Vector2 direccionAlObjetivo = (objetivo - (Vector2)transform.position).normalized;
        Vector2 mejorDireccion = Vector2.zero;
        
        float[] intereses = new float[8];
        float[] peligros = new float[8];

        // 1. Calcular interés basado en el objetivo
        for (int i = 0; i < 8; i++)
        {
            float dot = Vector2.Dot(direcciones[i], direccionAlObjetivo);
            intereses[i] = Mathf.Max(0f, dot);
        }

        // 2. Calcular peligro basado en Raycast
        for (int i = 0; i < 8; i++)
        {
            Vector2 dir = direcciones[i];
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distanciaRaycast, obstacleLayer);
            float peligro = 0f;

            if (hit.collider != null && hit.collider.gameObject != this.gameObject)
            {
                // Ignorar al jugador si está en la capa de obstáculos por error
                if (jugador != null && hit.collider.transform == jugador)
                    continue;

                float dist = hit.distance;
                peligro = 1f - (dist / distanciaRaycast);
            }

            peligros[i] = peligro;

            // Dibujar rayo rojo según el peligro detectado
            if (peligro > 0f)
            {
                Debug.DrawRay(transform.position, dir * (distanciaRaycast * peligro), Color.red);
            }
        }

        // 3. Evaluar la mejor dirección (interés - peligro)
        float mejorScore = -Mathf.Infinity;
        for (int i = 0; i < 8; i++)
        {
            // Ponderamos el peligro multiplicándolo por un factor de evasión
            float score = intereses[i] - peligros[i] * 1.5f;
            if (score > mejorScore)
            {
                mejorScore = score;
                mejorDireccion = direcciones[i];
            }
        }

        // Si todas las direcciones están extremadamente penalizadas, por defecto ir al objetivo
        if (mejorDireccion == Vector2.zero)
        {
            mejorDireccion = direccionAlObjetivo;
        }

        if (mejorDireccion != Vector2.zero)
        {
            float velocidadActual = velocidadBase;
            Vector2 direccionFinal = mejorDireccion;

            if (esZombie)
            {
                if (esPersecucion)
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
                }

                // Fase 1: Tambaleo (Se aplica siempre si es zombie, en patrulla o persecución)
                Vector2 perpendicular = new Vector2(-mejorDireccion.y, mejorDireccion.x);
                float factorTambaleo = Mathf.Sin((Time.time + offsetTambaleo) * velocidadTambaleo) * amplitudTambaleo;
                
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

        if (anim != null && tieneParametroCharging)
        {
            anim.SetBool("isCharging", true);
        }

        // Añadimos control contra atascos y límite de tiempo de embestida
        float maxChargeTime = 2.0f; 
        float elapsed = 0f;
        Vector2 lastPos = transform.position;
        float stuckTimer = 0f;

        while (Vector2.Distance(transform.position, posicionObjetivo) > 0.1f && elapsed < maxChargeTime)
        {
            transform.position = Vector2.MoveTowards(transform.position, posicionObjetivo, velocidadEmbestida * Time.deltaTime);
            elapsed += Time.deltaTime;

            if (Vector2.Distance(transform.position, lastPos) < 0.001f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 0.15f) // Si choca contra un muro durante 0.15s, cancelamos
                {
                    break;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPos = transform.position;
            yield return null;
        }

        if (anim != null && tieneParametroCharging)
        {
            anim.SetBool("isCharging", false);
        }

        estaEmbistiendo = false;
        estaOcupado = false;

        yield return new WaitForSeconds(tiempoRecargaEmbestida);
        puedeEmbestir = true;
    }

    private IEnumerator RutinaExplosion()
    {
        float tiempoPasado = 0f;

        // Disparar animación de aviso/cuenta atrás ("Atacar") si existe
        if (anim != null && tieneParametroAtacar)
        {
            anim.SetTrigger("Atacar");
        }

        // Parpadeo rojo en lugar de temblor físico para no interrumpir el movimiento
        while (tiempoPasado < tiempoParaExplotar)
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            tiempoPasado += 0.2f;
        }

        // --- INICIO DE LA EXPLOSIÓN / MUERTE ---
        
        // 1. Desactivar colisiones y físicas inmediatamente para detener al zombie
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // 2. Desactivar script de IA
        this.enabled = false;

        // 3. Disparar animación de "Muerte" (caída/explosión)
        if (anim != null && tieneParametroMuerte)
        {
            anim.SetTrigger("Muerte");
        }

        // 4. Esperamos un instante (ej. 0.4s) mientras "cae" antes de hacer el daño real
        yield return new WaitForSeconds(0.4f);

        // 5. Aplicar daño y temblor de cámara
        if (jugador != null)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(2.5f);
            float distanciaFinal = Vector2.Distance(transform.position, jugador.position);
            // Ampliamos un poco el rango real de explosión para ser justos si el enemigo sigue andando
            if (distanciaFinal <= rangoActivacionExplosion * 1.5f)
            {
                jugador.GetComponent<PlayerHealth>().RecibirDano(danoExplosion, transform.position);
            }
        }

        // 6. Esperamos a que termine el resto de la animación de explosión (0.8s) antes de destruirlo
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }

    private IEnumerator RutinaAtaqueZombie()
    {
        estaOcupado = true;
        puedeAtacarZombie = false;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Disparar animación de ataque "Atacar" si existe
        if (anim != null && tieneParametroAtacar)
        {
            anim.SetTrigger("Atacar");
        }

        // Breve anticipación del ataque (ej: 0.3 segundos) antes del golpe real
        yield return new WaitForSeconds(0.3f);

        // Comprobamos si el jugador sigue estando a nuestro alcance
        if (jugador != null)
        {
            float dist = Vector2.Distance(transform.position, jugador.position);
            if (dist <= rangoAtaqueZombie * 1.3f)
            {
                PlayerHealth ph = jugador.GetComponent<PlayerHealth>();
                if (ph != null) ph.RecibirDano(danoPorContacto, transform.position);
            }
        }

        // Recuperación del ataque
        yield return new WaitForSeconds(0.4f);
        estaOcupado = false;

        // Tiempo de recarga entre ataques individuales
        yield return new WaitForSeconds(Mathf.Max(0.1f, tiempoEntreAtaquesZombie - 0.7f));
        puedeAtacarZombie = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (explota) return;
            
            // Si es el Toro y hace embestidas, solo hace daño si está embistiendo activamente
            if (haceEmbestidas)
            {
                if (!estaEmbistiendo) return;
            }
            // Si es un zombie melee común, ignora el daño por simple contacto (solo daña con su animación/rutina de ataque)
            else if (esZombie)
            {
                return;
            }
            
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.RecibirDano(danoPorContacto, transform.position);
            }
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

        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < puntosPatrulla.Length; i++)
            {
                if (puntosPatrulla[i] != null)
                {
                    Gizmos.DrawSphere(puntosPatrulla[i].position, 0.2f);
                    if (i < puntosPatrulla.Length - 1 && puntosPatrulla[i + 1] != null)
                    {
                        Gizmos.DrawLine(puntosPatrulla[i].position, puntosPatrulla[i + 1].position);
                    }
                    else if (puntosPatrulla.Length > 1 && puntosPatrulla[0] != null)
                    {
                        Gizmos.DrawLine(puntosPatrulla[i].position, puntosPatrulla[0].position);
                    }
                }
            }
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

        if (anim != null)
        {
            if (tieneParametroCharging) anim.SetBool("isCharging", false);
        }
    }
}
