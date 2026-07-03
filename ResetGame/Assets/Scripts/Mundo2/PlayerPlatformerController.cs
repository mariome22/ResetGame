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

    [Header("Habilidades de Parkour (Nivel 2)")]
    [Tooltip("Permite deslizarse por las paredes al caer.")]
    public bool enableWallSlide = false;
    [Tooltip("Permite saltar apoyándose en las paredes.")]
    public bool enableWallJump = false;
    [Tooltip("Permite realizar un Dash horizontal en el aire.")]
    public bool enableAirDash = false;
    [Tooltip("Permite realizar un doble salto en el aire.")]
    public bool enableDoubleJump = false;

    [Header("Configuración de Deslizamiento (Wall Slide)")]
    public float wallSlideSpeed = 2f;
    [Tooltip("Offset horizontal desde el centro del jugador para detectar paredes.")]
    public float wallCheckOffsetX = 0.5f;
    [Tooltip("Offset vertical desde el centro del jugador para detectar paredes.")]
    public float wallCheckOffsetY = 0f;
    [Tooltip("Radio del círculo de detección de pared.")]
    public float wallCheckRadius = 0.2f;
    [Tooltip("Nombre de la animación de Wall Slide (dejar vacío si no existe).")]
    public string wallSlideAnimName = "";

    [Header("Configuración de Salto en Pared (Wall Jump)")]
    [Tooltip("Fuerza aplicada al saltar de la pared: X (empuje lateral), Y (empuje vertical).")]
    public Vector2 wallJumpForce = new Vector2(10f, 15f);
    [Tooltip("Tiempo en segundos que se bloquea el control horizontal del jugador tras saltar de la pared.")]
    public float wallJumpControlLockTime = 0.15f;

    [Header("Configuración de Air Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    [Tooltip("Nombre de la animación de Dash (dejar vacío si no existe).")]
    public string dashAnimName = "";

    [Header("Configuración de Doble Salto")]
    public float doubleJumpForce = 14f;

    // Variables de estado internas para parkour
    private bool isTouchingWall;
    private bool wallOnRight;
    private bool wallOnLeft;
    private bool isWallSliding;
    private bool isControlLocked = false;
    private bool isDashing = false;
    private bool canDash = true;
    private bool doubleJumpAvailable = true;
    private float dashCooldownTimer;
    private float normalGravityScale;
    private bool isInsideGravityZone = false;

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
    public int maxHealth = 3;
    private int currentHealth;

    [Tooltip("Si está activo, al iniciar esta escena las vidas del jugador se forzarán al valor de Max Health, ignorando las acumuladas de niveles anteriores.")]
    public bool forzarVidaAlIniciar = false;

    [Tooltip("Tiempo de invulnerabilidad tras recibir daño")]
    public float invincibilityTime = 1f;
    private float invincibilityTimer;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    [Header("Sonidos (SFX)")]
    [SerializeField] private AudioClip sonidoSalto;
    [SerializeField] private AudioClip sonidoDash;
    [SerializeField] private AudioClip sonidoPaso;
    [SerializeField] private float pasoIntervalo = 0.35f;
    private float pasoTimer = 0f;

    [Header("Animaciones")]
    public string idleAnimName = "idle";
    public string walkAnimName = "walk";
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

    [Header("Configuración de Tiempo")]
    [Tooltip("Tiempo inicial para pasar el nivel en segundos.")]
    public float startingTime = 500f;

    [Tooltip("Cantidad de monedas necesarias para añadir 10 segundos.")]
    public int coinsForTimeBonus = 50;

    public static float remainingTime = 500f;
    public static float checkpointTime = 500f;
    public static int consecutiveCheckpointDeaths = 0;
    public static bool isReloadingFromDeath = false;

    [Header("Sistema de Intentos")]
    [Tooltip("Si está activo, el jugador tendrá un número limitado de intentos (3) antes del Game Over.")]
    public bool enableAttempts = true;

    public static bool attemptsEnabled = true;

    [Header("UI Game Over")]
    [Tooltip("Panel UI que se muestra al perder todos los intentos")]
    public GameObject panelGameOver;

    [Header("Sonido Game Over")]
    [SerializeField] private AudioClip sonidoGameOver;

    // Variables de respaldo para la persistencia en checkpoints
    public static int checkpointCoins = 0;
    public static int checkpointSecretCoins = 0;
    public static System.Collections.Generic.HashSet<string> collectedCoinsActive = new System.Collections.Generic.HashSet<string>();
    public static System.Collections.Generic.HashSet<string> collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>();

    void Start()
    {
        attemptsEnabled = enableAttempts;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            normalGravityScale = rb.gravityScale;
        }
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        currentHealth = maxHealth;

        // Si se activa forzar vida al iniciar, sobreescribimos las vidas estáticas
        if (forzarVidaAlIniciar)
        {
            lives = maxHealth;
        }

        // Si hemos guardado un checkpoint y es de esta misma escena, reaparecemos ahí
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (lastCheckpointScene == currentScene)
        {
            transform.position = lastCheckpointPos;
            remainingTime = checkpointTime;
        }
        else
        {
            remainingTime = startingTime;
            checkpointTime = startingTime;
        }

        // Si recargamos la escena debido a una muerte, evitamos reiniciar el contador de intentos
        if (isReloadingFromDeath)
        {
            isReloadingFromDeath = false;
        }
        else if (lastCheckpointScene != currentScene)
        {
            consecutiveCheckpointDeaths = 0;
        }

        // Mostrar estadísticas iniciales por consola
        Debug.Log($"[Mundo 2] Nivel Iniciado. Vidas: {lives} | Monedas: {totalCoins} | Secretas: {secretCoinsCollected}/3 | Tiempo: {remainingTime}s");

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

        if (totalCoins >= coinsForTimeBonus)
        {
            totalCoins -= coinsForTimeBonus;
            remainingTime += 10f;
            Debug.Log($"¡{coinsForTimeBonus} Monedas recolectadas! ¡+10s TIEMPO ganados! Tiempo restante: {remainingTime}");
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
        remainingTime += 10f;
        Debug.Log($"¡¡MONEDA SECRETA ENCONTRADA!! Total secretas: {secretCoinsCollected}/3. +10s tiempo. Tiempo restante: {remainingTime}");

        // Actualizar el HUD al encontrar galletas secretas
        if (HUDPlatformerManager.Instance != null)
        {
            HUDPlatformerManager.Instance.UpdateHUD();
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (isDead)
        {
            ChangeAnimationState(deadAnimName);
            return;
        }

        // Decrementar el tiempo del nivel
        if (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                Die();
                return;
            }
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
        if (!isControlLocked && !isDashing && Keyboard.current != null)
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

        // Detección de paredes para parkour
        if (enableWallSlide || enableWallJump)
        {
            if (playerCollider != null)
            {
                Bounds bounds = playerCollider.bounds;
                float castDistance = 0.08f; // Margen exterior de detección fuera del collider del jugador

                // Hacer un boxcast a la derecha
                RaycastHit2D hitRight = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.right, castDistance, groundLayer);
                wallOnRight = hitRight.collider != null;

                // Hacer un boxcast a la izquierda
                RaycastHit2D hitLeft = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.left, castDistance, groundLayer);
                wallOnLeft = hitLeft.collider != null;

                isTouchingWall = wallOnRight || wallOnLeft;
            }
            else
            {
                // Fallback manual si por alguna razón no hay collider
                wallOnRight = Physics2D.OverlapCircle(new Vector2(transform.position.x + wallCheckOffsetX, transform.position.y + wallCheckOffsetY), wallCheckRadius, groundLayer);
                wallOnLeft = Physics2D.OverlapCircle(new Vector2(transform.position.x - wallCheckOffsetX, transform.position.y - wallCheckOffsetY), wallCheckRadius, groundLayer);
                isTouchingWall = wallOnRight || wallOnLeft;
            }
        }
        else
        {
            isTouchingWall = false;
            wallOnRight = false;
            wallOnLeft = false;
        }

        // Recargar habilidades de parkour al tocar el suelo
        if (isGrounded)
        {
            canDash = true;
            doubleJumpAvailable = true;
        }

        // Lógica de Wall Slide (Deslizamiento por la pared)
        isWallSliding = false;
        if (enableWallSlide && !isGrounded && isTouchingWall && rb.linearVelocity.y < 0.1f)
        {
            // Solo desliza si el jugador está empujando activamente en dirección a la pared (D o A).
            // Esto requiere mayor precisión técnica y añade dificultad al parkour.
            bool pushingWall = (wallOnRight && horizontalInput > 0.01f) || (wallOnLeft && horizontalInput < -0.01f);
            if (pushingWall)
            {
                isWallSliding = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
            }
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
            // Salto normal
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            if (sonidoSalto != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(sonidoSalto);
            }
        }
        else if (enableWallJump && !isGrounded && isTouchingWall && jumpBufferCounter > 0f)
        {
            // Salto de pared (Wall Jump)
            isWallSliding = false;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            // Saltar en dirección contraria a la pared
            float jumpDir = wallOnRight ? -1f : 1f;
            rb.linearVelocity = new Vector2(wallJumpForce.x * jumpDir, wallJumpForce.y);

            if (sonidoSalto != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(sonidoSalto);
            }

            // Rotar de inmediato hacia la dirección del salto
            if ((jumpDir > 0 && !facingRight) || (jumpDir < 0 && facingRight))
            {
                Flip();
            }

            // Iniciar bloqueo temporal de control horizontal
            StartCoroutine(WallJumpControlLockRoutine());
        }
        else if (enableDoubleJump && doubleJumpAvailable && !isGrounded && !isWallSliding && jumpBufferCounter > 0f)
        {
            // Doble salto en el aire
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            doubleJumpAvailable = false;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            if (sonidoSalto != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(sonidoSalto);
            }
        }

        // 7. Salto Variable (soltar el botón antes)
        bool jumpReleased = Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame;
        if (jumpReleased && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpHeightMultiplier);
            coyoteTimeCounter = 0f;
        }

        // Lógica de Air Dash
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        bool dashPressed = enableAirDash && !isInsideGravityZone && Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame;
        if (dashPressed && canDash && !isGrounded && !isWallSliding && dashCooldownTimer <= 0f)
        {
            StartCoroutine(DashRoutine());
        }

        // Lógica de pasos en plataformas
        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f && !isDashing && !isDead)
        {
            pasoTimer += Time.deltaTime;
            if (pasoTimer >= pasoIntervalo)
            {
                pasoTimer = 0f;
                if (sonidoPaso != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(sonidoPaso);
                }
            }
        }
        else
        {
            pasoTimer = pasoIntervalo;
        }

        // 8. Control de Animaciones
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;

        if (isDead)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // Si el control está bloqueado (tras wall jump) o estamos haciendo dash, dejamos que las físicas del impulso actúen
        if (isControlLocked || isDashing)
        {
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

        // Pintar áreas de detección de pared en cian
        Gizmos.color = Color.cyan;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Bounds bounds = col.bounds;
            float castDistance = 0.08f;
            
            // Dibujar caja de detección derecha
            Gizmos.DrawWireCube(new Vector2(bounds.center.x + castDistance / 2f + bounds.extents.x, bounds.center.y), new Vector3(castDistance, bounds.size.y, 0f));
            // Dibujar caja de detección izquierda
            Gizmos.DrawWireCube(new Vector2(bounds.center.x - castDistance / 2f - bounds.extents.x, bounds.center.y), new Vector3(castDistance, bounds.size.y, 0f));
        }
        else
        {
            Gizmos.DrawWireSphere(new Vector2(transform.position.x + wallCheckOffsetX, transform.position.y + wallCheckOffsetY), wallCheckRadius);
            Gizmos.DrawWireSphere(new Vector2(transform.position.x - wallCheckOffsetX, transform.position.y - wallCheckOffsetY), wallCheckRadius);
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

    private IEnumerator WallJumpControlLockRoutine()
    {
        isControlLocked = true;
        yield return new WaitForSeconds(wallJumpControlLockTime);
        isControlLocked = false;
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        if (sonidoDash != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(sonidoDash);
        }

        // Guardar gravedad original y pausarla
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Aplicar velocidad en la dirección hacia la que mira el jugador
        float dashDir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        // Esperar duración
        yield return new WaitForSeconds(dashDuration);

        // Restaurar gravedad e inercia lateral leve
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
        isDashing = false;
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

        // Al respawnear, restauramos las vidas al máximo configurado
        lives = maxHealth;

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (attemptsEnabled)
        {
            // Incrementar el contador de muertes consecutivas
            consecutiveCheckpointDeaths++;
            Debug.Log($"[Muerte] Intento perdido. Intentos usados: {consecutiveCheckpointDeaths}/3");

            if (consecutiveCheckpointDeaths >= 3)
            {
                Debug.Log("¡GAME OVER! Se han agotado los 3 intentos. Mostrando pantalla de Game Over.");
                consecutiveCheckpointDeaths = 0;
                isReloadingFromDeath = false;

                // Limpiar checkpoint para forzar reinicio completo
                lastCheckpointScene = "";
                lastCheckpointPos = Vector2.zero;
                
                totalCoins = 0;
                secretCoinsCollected = 0;
                checkpointCoins = 0;
                checkpointSecretCoins = 0;
                collectedCoinsActive.Clear();
                collectedCoinsAtCheckpoint.Clear();

                if (panelGameOver != null)
                {
                    // Hacer fundido a negro antes de mostrar la pantalla de Game Over
                    if (SceneTransitionManager.Instance != null)
                    {
                        bool fadeDone = false;
                        SceneTransitionManager.Instance.FadeOut(0.5f, () => {
                            fadeDone = true;
                        });
                        yield return new WaitUntil(() => fadeDone);
                    }

                    panelGameOver.SetActive(true);
                    Time.timeScale = 0f; // Pausar juego al perder todos los intentos

                    if (sonidoGameOver != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(sonidoGameOver);
                    }

                    // Fundido de vuelta a transparente revelando el panel
                    if (SceneTransitionManager.Instance != null)
                    {
                        SceneTransitionManager.Instance.FadeIn(0.5f, null);
                    }
                }
                else
                {
                    // Fallback
                    if (SceneTransitionManager.Instance != null)
                    {
                        SceneTransitionManager.Instance.LoadSceneWithFade(currentScene);
                    }
                    else
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
                    }
                }
                yield break;
            }
            else
            {
                isReloadingFromDeath = true;
            }
        }
        else
        {
            isReloadingFromDeath = true;
        }

        // Comprobar si hemos pasado el checkpoint en esta escena
        if (lastCheckpointScene == currentScene)
        {
            // Respawn en el checkpoint: restauramos estadísticas al estado guardado del checkpoint
            totalCoins = checkpointCoins;
            secretCoinsCollected = checkpointSecretCoins;
            collectedCoinsActive = new System.Collections.Generic.HashSet<string>(collectedCoinsAtCheckpoint);

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade(currentScene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
            }
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
            
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade(currentScene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
            }
        }
    }

    public static void SaveCheckpointStats()
    {
        checkpointCoins = totalCoins;
        checkpointSecretCoins = secretCoinsCollected;
        checkpointTime = remainingTime;
        
        // Guardamos las monedas y galletas recolectadas permanentemente hasta el checkpoint
        collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>(collectedCoinsActive);
        
        Debug.Log($"[Checkpoint Guardado] Monedas: {checkpointCoins} | Galletas: {checkpointSecretCoins} | Tiempo: {checkpointTime} | Muertes acumuladas: {consecutiveCheckpointDeaths}/3 | Total monedas registradas: {collectedCoinsAtCheckpoint.Count}");
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

        if (isDashing)
        {
            ChangeAnimationState(string.IsNullOrEmpty(dashAnimName) ? jumpAnimName : dashAnimName);
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
            if (isWallSliding)
            {
                ChangeAnimationState(string.IsNullOrEmpty(wallSlideAnimName) ? fallAnimName : wallSlideAnimName);
            }
            else if (rb.linearVelocity.y > 0.1f)
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

    public bool RechargeDash()
    {
        if (!canDash || dashCooldownTimer > 0f)
        {
            canDash = true;
            dashCooldownTimer = 0f;
            return true;
        }
        return false;
    }

    public void SetGravityZoneState(bool inside, float scaleMultiplier)
    {
        isInsideGravityZone = inside;
        if (inside)
        {
            rb.gravityScale = normalGravityScale * scaleMultiplier;
        }
        else
        {
            rb.gravityScale = normalGravityScale;
        }
    }

    public void ReiniciarNivelCompleto()
    {
        Time.timeScale = 1f;
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(currentScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }

    public void SalirAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
