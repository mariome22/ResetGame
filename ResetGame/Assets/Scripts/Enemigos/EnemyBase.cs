using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    [Header("Estadisticas")]
    public int vidaMaxima = 3;
    private int vidaActual;

    [Header("Knockback")]
    [Tooltip("Fuerza bruta para vencer el Linear Drag")]
    public float fuerzaKnockback = 15f;
    public float tiempoAturdido = 0.2f;

    [Header("Animaciones")]
    [Tooltip("Nombre del parámetro booleano en el Animator que indica si camina")]
    public string parametroCaminar = "isWalking";

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
    private Rigidbody2D rb;
    private MonoBehaviour scriptMovimiento;
    private Animator animator;
    private Vector3 ultimaPosicion;

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        ultimaPosicion = transform.position;

        if (GetComponent<EnemyMelee>() != null) scriptMovimiento = GetComponent<EnemyMelee>();

        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
    }

    private void LateUpdate()
    {
        if (animator != null)
        {
            // Calculamos la distancia movida desde el último frame
            float distanciaMoviendose = Vector3.Distance(transform.position, ultimaPosicion);
            
            // Si supera un mínimo para evitar vibraciones, asumimos que intenta andar
            bool seEstaMoviendo = distanciaMoviendose > 0.001f;
            
            // Enviamos el booleano al Animator
            animator.SetBool(parametroCaminar, seEstaMoviendo);

            // Actualizamos la posición para el siguiente frame
            ultimaPosicion = transform.position;
        }
    }

    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;

        StartCoroutine(EfectoDano());
        StartCoroutine(AplicarKnockback());

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private IEnumerator AplicarKnockback()
    {
        //Para detener el movimiento
        if (scriptMovimiento != null)
        {
            EnemyMelee melee = scriptMovimiento as EnemyMelee;
            if (melee != null) melee.ResetearAtaque();

            scriptMovimiento.enabled = false;
        }

        //Empujón
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null && rb != null)
        {
            Vector2 direccionRebote = (transform.position - jugador.transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direccionRebote * fuerzaKnockback, ForceMode2D.Impulse);
        }

        //Hasta que se frena el retroceso
        yield return new WaitForSeconds(tiempoAturdido);

        //Activamos de nuevo
        if (this != null && scriptMovimiento != null)
        {
            scriptMovimiento.enabled = true;
        }
    }

    private IEnumerator EfectoDano()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = colorOriginal;
        }
    }

    private void Morir()
    {
        Destroy(gameObject);
    }
}
