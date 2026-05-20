using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerPlatformerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    private float horizontalInput;

    [Header("Salto")]
    public float jumpForce = 16f;
    [Range(0, 1)]
    [Tooltip("Cuánto se reduce la fuerza del salto si sueltas el botón Espacio antes de tiempo.")]
    public float jumpHeightMultiplier = 0.5f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Mejoras (Game Feel)")]
    [Tooltip("Tiempo de gracia para saltar tras caer por un borde.")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Tooltip("Tiempo de gracia en el que se recuerda el botón de salto si lo pulsas antes de tocar el suelo.")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private bool facingRight = true;

    [Header("Salud y Combate")]
    public int maxHealth = 2;
    private int currentHealth;

    [Tooltip("Tiempo de invulnerabilidad tras recibir daño")]
    public float invincibilityTime = 1f;
    private float invincibilityTimer;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        // 1. Entrada horizontal usando el Nuevo Input System (Teclado)
        horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput -= 1f;
        }

        // 2. Girar el personaje hacia donde se mueve
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }

        // 3. Detección de suelo
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // 4. Lógica de Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 5. Lógica de Jump Buffer
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 6. Ejecutar Salto
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // 7. Salto Variable (soltar el botón antes)
        bool jumpReleased = Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame;
        if (jumpReleased && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpHeightMultiplier);
            coyoteTimeCounter = 0f;
        }
    }

    void FixedUpdate()
    {
        // Aplicar movimiento físico en FixedUpdate para mayor estabilidad
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // Dibujar un círculo rojo en el editor de Unity para poder ajustar el GroundCheck fácilmente
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void Bounce(float bounceForce = 12f)
    {
        // Reseteamos la velocidad vertical y aplicamos un impulso para rebotar
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
    }

    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0) return;

        currentHealth -= damage;
        Debug.Log("¡El jugador ha recibido daño! Vida restante: " + currentHealth);

        if (currentHealth > 0)
        {
            invincibilityTimer = invincibilityTime;
            if (spriteRenderer != null) StartCoroutine(DamageFlickerRoutine());
        }
        else if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlickerRoutine()
    {
        float elapsed = 0f;
        while (elapsed < invincibilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        spriteRenderer.enabled = true;
    }

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");
        // TODO: Añadir lógica para recargar escena o Game Over
    }
}
