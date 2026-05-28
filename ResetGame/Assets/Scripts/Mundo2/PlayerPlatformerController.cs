using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerPlatformerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    private float horizontalInput;

    [Header("Aceleración e Inercia")]
    [Tooltip("Aceleración en el suelo (valores altos = más responsivo)")]
    public float acceleration = 45f;
    [Tooltip("Deceleración en el suelo al soltar el botón")]
    public float deceleration = 45f;
    [Tooltip("Aceleración en el aire (menor valor = más inercia, no gira tan rápido)")]
    public float airAcceleration = 20f;
    [Tooltip("Deceleración en el aire al soltar el botón")]
    public float airDeceleration = 15f;

    [Header("Salto")]
    public float jumpForce = 16f;
    [Range(0, 1)]
    [Tooltip("Cuánto se reduce la fuerza del salto si sueltas el botón Espacio antes de tiempo.")]
    public float jumpHeightMultiplier = 0.5f;
    [Tooltip("Fuerza del rebote si mantienes pulsado Espacio justo al pisar un enemigo")]
    public float highBounceForce = 22f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

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

    [Header("Animaciones (Nombres de Estados)")]
    [Tooltip("Nombre de la animación de estar quieto en el Animator")]
    public string idleAnimName = "idle";
    [Tooltip("Nombre de la animación de caminar en el Animator")]
    public string walkAnimName = "walk";
    [Tooltip("Nombre de la animación de salto en el Animator")]
    public string jumpAnimName = "jump";
    [Tooltip("Nombre de la animación de caída en el Animator")]
    public string fallAnimName = "fall";
    [Tooltip("Nombre de la animación de recibir daño en el Animator")]
    public string hitAnimName = "hit";
    [Tooltip("Nombre de la animación de morir en el Animator")]
    public string deadAnimName = "dead";

    private Animator animator;
    private string currentAnimationState;
    private bool isDead = false;
    private Coroutine flickerCoroutine;

    [Header("Límites del Nivel")]
    [Tooltip("La altura (eje Y) a la que el jugador morirá instantáneamente si cae al vacío.")]
    public float fallDeathY = -15f;

    [HideInInspector]
    public Vector2 movingPlatformVelocity = Vector2.zero;

    [Header("Checkpoint System")]
    // Usamos variables estáticas para que sobrevivan a la recarga de la escena
    public static string lastCheckpointScene = "";
    public static Vector2 lastCheckpointPos;

    [Header("Monedas y Vidas Persistentes")]
    public static int totalCoins = 0;
    public static int lives = 3;
    public static int secretCoinsCollected = 0;

    // Variables de respaldo para la persistencia en checkpoints
    public static int checkpointCoins = 0;
    public static int checkpointSecretCoins = 0;
    public static System.Collections.Generic.HashSet<string> collectedCoinsActive = new System.Collections.Generic.HashSet<string>();
    public static System.Collections.Generic.HashSet<string> collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        currentHealth = maxHealth;

        // Si hemos guardado un checkpoint y es de esta misma escena, reaparecemos ahí
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (lastCheckpointScene == currentScene)
        {
            transform.position = lastCheckpointPos;
        }

        // Mostrar estadísticas iniciales por consola
        Debug.Log($"[Mundo 2] Nivel Iniciado. Vidas: {lives} | Monedas: {totalCoins} | Secretas: {secretCoinsCollected}/3");

        // Actualizar el HUD al iniciar la escena
        if (HUDPlatformerManager.Instance != null)
        {
            HUDPlatformerManager.Instance.UpdateHUD();
        }
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        Debug.Log($"¡Moneda recolectada! Monedas: {totalCoins}");

        if (totalCoins >= 50)
        {
            totalCoins -= 50;
            lives++;
            Debug.Log($"¡50 Monedas recolectadas! ¡VIDA EXTRA ganada! Vidas restantes: {lives}");
        }

        // Actualizar el HUD al recolectar monedas
        if (HUDPlatformerManager.Instance != null)
        {
            HUDPlatformerManager.Instance.UpdateHUD();
        }
    }

    public void CollectSecretCoin()
    {
        secretCoinsCollected++;
        Debug.Log($"¡¡MONEDA SECRETA ENCONTRADA!! Total secretas: {secretCoinsCollected}/3");

        // Actualizar el HUD al encontrar galletas secretas
        if (HUDPlatformerManager.Instance != null)
        {
            HUDPlatformerManager.Instance.UpdateHUD();
        }
    }

    void Update()
    {
        if (isDead)
        {
            ChangeAnimationState(deadAnimName);
            return;
        }

        // Si el jugador cae al vacío, muere instantáneamente (pierde todas las vidas y respawnea)
        if (transform.position.y < fallDeathY)
        {
            lives = 0;
            if (HUDPlatformerManager.Instance != null)
            {
                HUDPlatformerManager.Instance.UpdateHUD();
            }
            Die();
            return;
        }

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

        // 8. Control de Animaciones
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // 1. Calcular la velocidad objetivo a la que queremos ir (input * velocidad máxima)
        float targetSpeed = horizontalInput * moveSpeed;

        // 2. Elegir qué ritmo de aceleración usar (suelo vs aire, acelerar vs frenar)
        float accelRate;
        if (isGrounded)
        {
            // En el suelo: si pulsamos algo aceleramos, si soltamos frenamos
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        }
        else
        {
            // En el aire: aplicamos los valores de inercia del aire
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration;
        }

        // 3. Obtener la velocidad actual del jugador independiente de la plataforma móvil
        float currentRelativeX = rb.linearVelocity.x - movingPlatformVelocity.x;

        // 4. Mover la velocidad actual hacia la objetivo aplicando el accelRate
        float newRelativeX = Mathf.MoveTowards(currentRelativeX, targetSpeed, accelRate * Time.fixedDeltaTime);

        // 5. Aplicar la nueva velocidad sumando de nuevo la inercia de la plataforma móvil
        rb.linearVelocity = new Vector2(newRelativeX + movingPlatformVelocity.x, rb.linearVelocity.y);
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
        // Revisar si el jugador está manteniendo pulsado el botón de salto (Espacio)
        bool jumpHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        
        // Si lo mantiene pulsado, rebotará mucho más alto (Mecánica clásica de Mario)
        float forceToApply = jumpHeld ? highBounceForce : bounceForce;

        // Reseteamos la velocidad vertical y aplicamos un impulso para rebotar
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, forceToApply);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || invincibilityTimer > 0) return;

        // Cada golpe recibido resta una vida directamente
        lives--;
        Debug.Log("¡El jugador ha recibido daño! Vidas restantes: " + lives);

        // Actualizar HUD al instante
        if (HUDPlatformerManager.Instance != null)
        {
            HUDPlatformerManager.Instance.UpdateHUD();
        }

        if (lives > 0)
        {
            // Si aún le quedan vidas, solo parpadea (invulnerabilidad) en el sitio
            invincibilityTimer = invincibilityTime;
            if (spriteRenderer != null)
            {
                if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
                flickerCoroutine = StartCoroutine(DamageFlickerRoutine());
            }
        }
        else
        {
            // Muerte total y respawn al quedarse sin vidas (0 vidas)
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
        flickerCoroutine = null;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        // Detener flicker si está ocurriendo y forzar visibilidad del sprite
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Desactivar físicas y colisiones al morir para evitar interacciones raras
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Detiene colisiones, gravedad, etc.
        }

        // Reproducir la animación de muerte
        ChangeAnimationState(deadAnimName);

        // Esperar 1.5 segundos para que se aprecie el fotograma de muerte antes de recargar
        yield return new WaitForSeconds(1.5f);

        // Al respawnear (cuando te quitan las 3 vidas), restauramos las vidas a 3
        lives = 3;

        // Comprobar si hemos pasado el checkpoint en esta escena
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (lastCheckpointScene == currentScene)
        {
            // Respawn en el checkpoint: restauramos estadísticas al estado guardado del checkpoint
            totalCoins = checkpointCoins;
            secretCoinsCollected = checkpointSecretCoins;
            collectedCoinsActive = new System.Collections.Generic.HashSet<string>(collectedCoinsAtCheckpoint);

            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
        else
        {
            // Respawn al inicio del nivel: borramos todo de cero completo
            totalCoins = 0;
            secretCoinsCollected = 0;
            checkpointCoins = 0;
            checkpointSecretCoins = 0;
            collectedCoinsActive.Clear();
            collectedCoinsAtCheckpoint.Clear();
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }

    public static void SaveCheckpointStats()
    {
        checkpointCoins = totalCoins;
        checkpointSecretCoins = secretCoinsCollected;
        
        // Guardamos las monedas y galletas recolectadas permanentemente hasta el checkpoint
        collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>(collectedCoinsActive);
        
        Debug.Log($"[Checkpoint Guardado] Monedas: {checkpointCoins} | Galletas: {checkpointSecretCoins} | Total monedas registradas: {collectedCoinsAtCheckpoint.Count}");
    }

    public static void RegisterCollectedCoin(string key)
    {
        collectedCoinsActive.Add(key);
    }

    private void UpdateAnimations()
    {
        if (isDead)
        {
            ChangeAnimationState(deadAnimName);
            return;
        }

        if (invincibilityTimer > 0)
        {
            ChangeAnimationState(hitAnimName);
            return;
        }

        if (isGrounded)
        {
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                ChangeAnimationState(walkAnimName);
            }
            else
            {
                ChangeAnimationState(idleAnimName);
            }
        }
        else
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                ChangeAnimationState(jumpAnimName);
            }
            else
            {
                ChangeAnimationState(fallAnimName);
            }
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (animator == null) return;
        if (currentAnimationState == newState) return;

        animator.Play(newState);
        currentAnimationState = newState;
    }
}
