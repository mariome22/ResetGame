using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    [Header("Ajustes Base")]
    public GameObject prefabProyectil;
    public float ritmoDeDisparo = 2f;
    public float rangoDeVision = 7f;

    [Header("Ajustes de Ráfaga / Escopeta")]
    public int proyectilesPorAtaque = 1;
    public float anguloDeDispersion = 0f;

    [Tooltip("Si se marca, lanza todas las balas a la vez en abanico. Si no, las lanza en ráfaga (metralleta).")]
    public bool disparoSimultaneo = false; // LA MAGIA ESTÁ AQUÍ
    public float tiempoEntreBalas = 0.1f;

    [Header("Ajustes de Carga (Francotirador)")]
    public float tiempoDeCarga = 0f;

    private Transform jugador;
    private bool estaAtacando = false;

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
        if (jugador == null || estaAtacando) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia <= rangoDeVision)
        {
            StartCoroutine(RutinaDeAtaque());
        }
    }

    private IEnumerator RutinaDeAtaque()
    {
        estaAtacando = true;

        if (tiempoDeCarga > 0)
        {
            yield return new WaitForSeconds(tiempoDeCarga);
        }

        if (jugador != null)
        {
            Vector2 direccionBase = (jugador.position - transform.position).normalized;

            if (disparoSimultaneo)
            {
                // MODO ESCOPETA: Dispara todo a la vez en un abanico perfecto
                float anguloInicial = -anguloDeDispersion;
                float pasoAngulo = 0f;

                // Evitamos dividir por cero si solo hay 1 bala
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
                // MODO METRALLETA: Dispara una detrás de otra
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

        yield return new WaitForSeconds(ritmoDeDisparo);
        estaAtacando = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeVision);
    }
}