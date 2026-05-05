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

    [Header("Ajustes de Ráfaga / Escopeta")]
    public int proyectilesPorAtaque = 1;
    public float anguloDeDispersion = 0f;

    [Tooltip("Si se marca, lanza todas las balas a la vez en abanico. Si no, las lanza en ráfaga (metralleta).")]
    public bool disparoSimultaneo = false;
    public float tiempoEntreBalas = 0.1f;

    [Header("Ajustes de Carga (Francotirador)")]
    public float tiempoDeCarga = 0f;

    private Transform jugador;
    private bool estaAtacando = false;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
    }

    private void Update()
    {
        if (jugador == null || estaAtacando) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia <= rangoDeVision)
        {
            Vector2 direccionBase = (jugador.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionBase, distancia, capaBloqueoVision);
            
            // Solo ataca si no hay muros en medio
            if (hit.collider == null)
            {
                StartCoroutine(RutinaDeAtaque());
            }
        }
    }

    private IEnumerator RutinaDeAtaque()
    {
        estaAtacando = true;

        if (tiempoDeCarga > 0)
        {
            float tiempoRestante = tiempoDeCarga;
            float ritmoParpadeo = 0.2f;

            // Telegrafiado visual: Parpadea en rojo cada vez más rápido
            while (tiempoRestante > 0)
            {
                if (spriteRenderer != null) spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(Mathf.Min(ritmoParpadeo / 2f, tiempoRestante));
                tiempoRestante -= ritmoParpadeo / 2f;

                if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
                
                if (tiempoRestante > 0)
                {
                    yield return new WaitForSeconds(Mathf.Min(ritmoParpadeo / 2f, tiempoRestante));
                    tiempoRestante -= ritmoParpadeo / 2f;
                }
                
                ritmoParpadeo = Mathf.Max(0.05f, ritmoParpadeo * 0.8f); // Acelera el parpadeo
            }
            if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
        }

        if (jugador != null)
        {
            Vector2 direccionBase = (jugador.position - transform.position).normalized;

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

        // Variación de ritmo aleatoria
        float tiempoEspera = ritmoDeDisparo + Random.Range(-variacionRitmo, variacionRitmo);
        tiempoEspera = Mathf.Max(0.1f, tiempoEspera); // Nunca debe ser negativo
        
        yield return new WaitForSeconds(tiempoEspera);
        estaAtacando = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeVision);
    }
}
