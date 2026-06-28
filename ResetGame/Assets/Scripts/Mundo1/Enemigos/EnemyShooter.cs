using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    [Header("Ajustes Base")]
    public GameObject prefabProyectil;
    public float ritmoDeDisparo = 2f;
    [Tooltip("El disparo variará aleatoriamente este tiempo (ej. +-0.5s)")]
    public float variacionRitmo = 0.5f;
    public float rangoDeVision = 7f;
    [Tooltip("Capas que bloquean la visión (ej. Muros)")]
    public LayerMask capaBloqueoVision;

    [Header("Ajustes de Movimiento")]
    public float velocidad = 2f;
    public float distanciaOptima = 5f;
    public float distanciaHuir = 3f;
    [Tooltip("LayerMask para evitar obstáculos (Muros, etc.) al moverse")]
    public LayerMask obstacleLayer;
    public float distanciaRaycastObstaculos = 1f;
    public float radioObstaculos = 0.4f;
    [Tooltip("Ponderación del peligro para esquivar obstáculos. Valores más altos esquivan con más fuerza.")]
    public float factorEvasion = 1.5f;

    [Header("Ajustes de Ráfaga / Escopeta")]
    public int proyectilesPorAtaque = 1;
    public float anguloDeDispersion = 0f;

    [Tooltip("Si se marca, lanza todas las balas a la vez en abanico. Si no, las lanza en ráfaga (metralleta).")]
    public bool disparoSimultaneo = false;
    public float tiempoEntreBalas = 0.1f;

    [Header("Ajustes de Carga (Francotirador)")]
    public float tiempoDeCarga = 0f;
    [Tooltip("El rayo láser visual que apunta al jugador (opcional)")]
    public LineRenderer rayoLaser;
    [Tooltip("Tiempo antes de disparar en el que el enemigo deja de seguir al jugador (para permitir esquivar)")]
    public float tiempoFijacion = 0.5f;
    [Tooltip("Grosor del láser mientras está apuntando/siguiendo al jugador")]
    public float grosorLaserCarga = 0.05f;
    [Tooltip("Grosor del láser cuando se fija antes de disparar")]
    public float grosorLaserFijado = 0.1f;
    [Tooltip("Color del láser mientras está apuntando/siguiendo al jugador")]
    public Color colorLaserCarga = new Color(1f, 1f, 0f, 0.5f); // Amarillo semitransparente
    [Tooltip("Color del láser cuando se fija antes de disparar")]
    public Color colorLaserFijado = new Color(1f, 0f, 0f, 0.8f); // Rojo intenso

    private Transform jugador;
    private bool estaAtacando = false;
    private float temporizadorCooldown = 0f;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
    private Rigidbody2D rb;

    private Vector2[] direccionesMovimiento = new Vector2[]
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
    };

    private Animator anim;
    private bool tieneParametroAtacar = false;

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Atacar")
                {
                    tieneParametroAtacar = true;
                    break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (jugador == null) 
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (temporizadorCooldown > 0)
        {
            temporizadorCooldown -= Time.fixedDeltaTime;
        }

        float distancia = Vector2.Distance(transform.position, jugador.position);
        
        if (distancia <= rangoDeVision)
        {
            Vector2 direccionBase = (jugador.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionBase, distancia, capaBloqueoVision);
            bool tieneLineaVision = (hit.collider == null);

            // Logica de ataque (solo si no ataca y tiene el cooldown listo)
            if (!estaAtacando && tieneLineaVision && temporizadorCooldown <= 0)
            {
                StartCoroutine(RutinaDeAtaque());
            }

            // Logica de movimiento
            if (estaAtacando)
            {
                // Cuando está atacando/cargando se queda quieto
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
            else
            {
                // Decidir adónde moverse
                Vector2 objetivoMovimiento = transform.position;
                bool debeMoverse = false;

                if (!tieneLineaVision)
                {
                    // Si no tiene linea de vision, intenta acercarse al jugador para ganar vision
                    objetivoMovimiento = jugador.position;
                    debeMoverse = true;
                }
                else if (distancia < distanciaHuir)
                {
                    // Huir si el jugador está muy cerca
                    Vector2 dirHuida = (transform.position - jugador.position).normalized;
                    objetivoMovimiento = (Vector2)transform.position + dirHuida * 2f;
                    debeMoverse = true;
                }
                else if (distancia > distanciaOptima)
                {
                    // Acercarse si está más lejos de la distancia óptima
                    objetivoMovimiento = jugador.position;
                    debeMoverse = true;
                }

                if (debeMoverse && rb != null)
                {
                    MoverConContextSteering(objetivoMovimiento);
                }
                else if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        else
        {
            // Fuera de rango de visión, se queda quieto (o podría patrullar)
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    private void MoverConContextSteering(Vector2 objetivo)
    {
        if (rb == null) return;

        Vector2 direccionAlObjetivo = (objetivo - (Vector2)transform.position).normalized;
        Vector2 mejorDireccion = Vector2.zero;
        
        float[] intereses = new float[direccionesMovimiento.Length];
        float[] peligros = new float[direccionesMovimiento.Length];

        // 1. Calcular interés basado en el objetivo
        for (int i = 0; i < direccionesMovimiento.Length; i++)
        {
            float dot = Vector2.Dot(direccionesMovimiento[i], direccionAlObjetivo);
            intereses[i] = Mathf.Max(0f, dot);
        }

        // 2. Calcular peligro basado en CircleCast (con grosor)
        for (int i = 0; i < direccionesMovimiento.Length; i++)
        {
            Vector2 dir = direccionesMovimiento[i];
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radioObstaculos, dir, distanciaRaycastObstaculos, obstacleLayer);
            float peligro = 0f;

            if (hit.collider != null && hit.collider.gameObject != this.gameObject)
            {
                if (jugador != null && hit.collider.transform == jugador)
                    continue;

                float dist = hit.distance;
                peligro = 1f - (dist / distanciaRaycastObstaculos);
            }

            peligros[i] = peligro;

            // Dibujar rayo rojo según el peligro detectado
            if (peligro > 0f)
            {
                Debug.DrawRay(transform.position, dir * (distanciaRaycastObstaculos * peligro), Color.red);
            }
        }

        // 3. Evaluar dirección mediante la suma ponderada (Vector Blend)
        Vector2 direccionFinal = Vector2.zero;
        for (int i = 0; i < direccionesMovimiento.Length; i++)
        {
            float score = intereses[i] - peligros[i] * factorEvasion;
            score = Mathf.Max(0f, score);
            direccionFinal += direccionesMovimiento[i] * score;
        }

        if (direccionFinal == Vector2.zero)
        {
            direccionFinal = direccionAlObjetivo;
        }
        else
        {
            direccionFinal = direccionFinal.normalized;
        }

        rb.linearVelocity = direccionFinal * velocidad;
    }

    private IEnumerator RutinaDeAtaque()
    {
        estaAtacando = true;
        Vector2 objetivoApuntado = Vector2.zero;

        if (jugador != null)
        {
            objetivoApuntado = jugador.position;
        }

        if (tiempoDeCarga > 0)
        {
            float tiempoRestante = tiempoDeCarga;
            float ritmoParpadeo = 0.2f;
            float temporizadorParpadeo = ritmoParpadeo;
            bool parpadeoRojo = false;

            if (rayoLaser != null)
            {
                rayoLaser.enabled = true;
                rayoLaser.startColor = colorLaserCarga;
                rayoLaser.endColor = colorLaserCarga;
                rayoLaser.startWidth = grosorLaserCarga;
                rayoLaser.endWidth = grosorLaserCarga;
            }

            // Telegrafiado visual: Parpadea en rojo y actualiza el rayo láser
            while (tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime;
                temporizadorParpadeo -= Time.deltaTime;

                // Control visual de parpadeo del sprite
                if (temporizadorParpadeo <= 0)
                {
                    parpadeoRojo = !parpadeoRojo;
                    if (spriteRenderer != null) spriteRenderer.color = parpadeoRojo ? Color.red : colorOriginal;
                    
                    ritmoParpadeo = Mathf.Max(0.05f, (tiempoRestante / tiempoDeCarga) * 0.2f); // Más rápido cuanto menos tiempo queda
                    temporizadorParpadeo = ritmoParpadeo;
                }

                if (jugador != null)
                {
                    // Si aún no estamos en tiempo de fijación, seguimos al jugador
                    if (tiempoRestante > tiempoFijacion)
                    {
                        objetivoApuntado = jugador.position;
                        if (rayoLaser != null)
                        {
                            rayoLaser.startColor = colorLaserCarga;
                            rayoLaser.endColor = colorLaserCarga;
                            rayoLaser.startWidth = grosorLaserCarga;
                            rayoLaser.endWidth = grosorLaserCarga;
                        }
                    }
                    else
                    {
                        // Entramos en fijación, ya no actualizamos objetivoApuntado (el jugador puede esquivar)
                        if (rayoLaser != null)
                        {
                            rayoLaser.startColor = colorLaserFijado;
                            rayoLaser.endColor = colorLaserFijado;
                            rayoLaser.startWidth = grosorLaserFijado;
                            rayoLaser.endWidth = grosorLaserFijado;
                        }
                    }

                    // Actualizar posiciones del rayo láser
                    if (rayoLaser != null)
                    {
                        rayoLaser.SetPosition(0, transform.position);
                        
                        Vector2 dirLaser = (objetivoApuntado - (Vector2)transform.position).normalized;
                        // Proyectamos el rayo hacia adelante hasta chocar con una pared, o muy lejos
                        RaycastHit2D hitMuro = Physics2D.Raycast(transform.position, dirLaser, 50f, capaBloqueoVision);
                        
                        if (hitMuro.collider != null)
                        {
                            rayoLaser.SetPosition(1, hitMuro.point);
                        }
                        else
                        {
                            rayoLaser.SetPosition(1, (Vector2)transform.position + dirLaser * 50f);
                        }
                    }
                }

                yield return null;
            }
            if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
            if (rayoLaser != null) rayoLaser.enabled = false;
        }
        else
        {
            // Si no hay tiempo de carga, apuntamos directamente al jugador en el instante del disparo
            if (jugador != null) objetivoApuntado = jugador.position;
        }

        if (jugador != null)
        {
            // --- DISPARO / ANIMACIÓN ---
            // 1. Lanzamos la animación de ataque
            if (anim != null && tieneParametroAtacar)
            {
                anim.SetTrigger("Atacar");
            }

            // 2. Esperamos un instante muy breve de anticipación (0.15 segundos) para que la animación empiece y sea coherente con el proyectil
            yield return new WaitForSeconds(0.15f);

            // Re-verificar si el jugador sigue vivo/existe tras la espera
            if (jugador != null)
            {
                // Si no hay tiempo de carga, recalculamos la dirección al instante del disparo tras la breve espera de animación
                if (tiempoDeCarga <= 0)
                {
                    objetivoApuntado = jugador.position;
                }

                Vector2 direccionBase = (objetivoApuntado - (Vector2)transform.position).normalized;

                if (disparoSimultaneo)
                {
                    //ESCOPETA
                    float anguloInicial = -anguloDeDispersion;
                    float pasoAngulo = 0f;

                    if (proyectilesPorAtaque > 1)
                    {
                        pasoAngulo = (anguloDeDispersion * 2f) / (proyectilesPorAtaque - 1);
                    }

                    for (int i = 0; i < proyectilesPorAtaque; i++)
                    {
                        float anguloActual = anguloInicial + (pasoAngulo * i);
                        Vector3 direccionFinal = Quaternion.Euler(0, 0, anguloActual) * direccionBase;

                        GameObject bala = Instantiate(prefabProyectil, transform.position, Quaternion.identity);
                        bala.GetComponent<EnemyProjectile>().Disparar(direccionFinal);
                    }
                }
                else
                {
                    //FUSIL / FRANCOTIRADOR
                    for (int i = 0; i < proyectilesPorAtaque; i++)
                    {
                        float anguloRandom = Random.Range(-anguloDeDispersion, anguloDeDispersion);
                        Vector3 direccionFinal = Quaternion.Euler(0, 0, anguloRandom) * direccionBase;

                        GameObject bala = Instantiate(prefabProyectil, transform.position, Quaternion.identity);
                        bala.GetComponent<EnemyProjectile>().Disparar(direccionFinal);

                        if (tiempoEntreBalas > 0)
                        {
                            yield return new WaitForSeconds(tiempoEntreBalas);
                        }
                    }
                }
            }
        }

        // Ya ha disparado, así que el ataque en sí ha terminado. Puede moverse de nuevo.
        estaAtacando = false;

        // Variación de ritmo aleatoria para el tiempo de recarga
        float tiempoEspera = ritmoDeDisparo + Random.Range(-variacionRitmo, variacionRitmo);
        temporizadorCooldown = Mathf.Max(0.1f, tiempoEspera); // Nunca debe ser negativo
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeVision);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaOptima);

        Gizmos.color = new Color(1f, 0.5f, 0f); // Naranja
        Gizmos.DrawWireSphere(transform.position, distanciaHuir);
    }
}
