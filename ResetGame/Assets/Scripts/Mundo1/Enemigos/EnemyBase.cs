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
    private bool tieneParametroCaminar = false;

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parametroCaminar)
                {
                    tieneParametroCaminar = true;
                    break;
                }
            }
        }

        ultimaPosicion = transform.position;

        if (GetComponent<EnemyMelee>() != null) 
            scriptMovimiento = GetComponent<EnemyMelee>();
        else if (GetComponent<EnemyFollow>() != null) 
            scriptMovimiento = GetComponent<EnemyFollow>();
        else if (GetComponent<EnemyShooter>() != null) 
            scriptMovimiento = GetComponent<EnemyShooter>();

        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
    }

    private void LateUpdate()
    {
        if (animator != null && rb != null && tieneParametroCaminar)
        {
            // Usamos la velocidad del RigidBody en lugar de la posición para evitar problemas de sincronía con las físicas
            bool seEstaMoviendo = rb.linearVelocity.sqrMagnitude > 0.01f;
            
            // Enviamos el booleano al Animator
            animator.SetBool(parametroCaminar, seEstaMoviendo);
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
        StartCoroutine(MorirRoutine());
    }

    private IEnumerator MorirRoutine()
    {
        PersistentObject po = GetComponent<PersistentObject>();
        if (po != null) po.RegisterDestruction();

        // Desactivar colisionador para evitar estorbar al jugador
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Detener movimiento y físicas
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Detiene colisiones, gravedad, etc.
        }

        // Desactivar script de IA/movimiento
        if (scriptMovimiento != null)
        {
            scriptMovimiento.enabled = false;
        }

        // Lanzar animación de muerte si existe
        if (animator != null)
        {
            animator.SetTrigger("Muerte");
        }

        // Esperar 1.2 segundos para que termine la animación antes de destruir el objeto
        yield return new WaitForSeconds(1.2f);

        Destroy(gameObject);
    }
}
