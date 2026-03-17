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

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
    private Rigidbody2D rb;
    private MonoBehaviour scriptMovimiento;

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Detecta qué script controla al enemigo
        if (GetComponent<EnemyMelee>() != null) scriptMovimiento = GetComponent<EnemyMelee>();

        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
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
        // 1. Detenemos cualquier ataque en curso de forma segura
        if (scriptMovimiento != null)
        {
            EnemyMelee melee = scriptMovimiento as EnemyMelee;
            if (melee != null) melee.ResetearAtaque();

            scriptMovimiento.enabled = false;
        }

        // 2. Aplicamos el empujón físico
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null && rb != null)
        {
            Vector2 direccionRebote = (transform.position - jugador.transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direccionRebote * fuerzaKnockback, ForceMode2D.Impulse);
        }

        // 3. Dejamos que el Linear Drag de Unity lo frene suavemente
        yield return new WaitForSeconds(tiempoAturdido);

        // 4. Reactivamos el cerebro
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