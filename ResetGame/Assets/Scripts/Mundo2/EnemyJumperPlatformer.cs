using UnityEngine;

public class EnemyJumperPlatformer : EnemyPlatformerBase
{
    private enum State { Sliding, Jumping, Recovering }
    private State currentState = State.Sliding;

    [Header("Patrulla (Sliding)")]
    public float patrolSpeed = 3f;
    public Transform ledgeCheck; 
    public Transform wallCheck;  
    public float checkDistance = 0.5f;
    public LayerMask groundLayer;
    private bool movingRight = true;

    [Header("Comportamiento Aleatorio")]
    public float minTimeBeforeTurn = 2f;
    public float maxTimeBeforeTurn = 6f;
    private float turnTimer;

    [Header("Salto Predictivo")]
    public float jumpRange = 10f;
    [Tooltip("El tiempo exacto en segundos que debe durar el vuelo del enemigo hasta caer en el jugador")]
    public float jumpAirTime = 0.8f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    
    [Header("Recuperacion (Fallo)")]
    public float minCooldownDuration = 1f;
    public float maxCooldownDuration = 2f;
    private float cooldownTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private Rigidbody2D playerRb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ResetTurnTimer();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case State.Sliding:
                UpdateSliding();
                break;
            case State.Jumping:
                UpdateJumping();
                break;
            case State.Recovering:
                UpdateRecovering();
                break;
        }

        if (anim != null)
        {
            // Solo muestra animacion de caminar si esta deslizando y en el suelo
            bool isWalking = currentState == State.Sliding && Mathf.Abs(rb.linearVelocity.x) > 0.1f && IsGrounded();
            anim.SetBool("IsWalking", isWalking);
            anim.SetBool("IsAction", currentState == State.Jumping);
        }
    }

    private void UpdateSliding()
    {
        // Movimiento fisico constante
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * patrolSpeed, rb.linearVelocity.y);

        // Raycasts de detección
        bool hittingWall = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, checkDistance, groundLayer);
        bool hittingLedge = Physics2D.Raycast(ledgeCheck.position, Vector2.down, checkDistance, groundLayer);

        if (hittingWall || !hittingLedge)
        {
            TurnAround();
        }

        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0)
        {
            TurnAround();
        }

        // Chequear proximidad del jugador
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= jumpRange && IsGrounded())
        {
            JumpTowardsPlayer();
        }
    }

    private void UpdateJumping()
    {
        // Esperamos a aterrizar de nuevo (rb.linearVelocity.y <= 0.1f evita que toque el suelo nada más despegar)
        if (rb.linearVelocity.y <= 0.1f && IsGrounded())
        {
            // Aterrizó, pasamos a recuperación
            currentState = State.Recovering;
            cooldownTimer = Random.Range(minCooldownDuration, maxCooldownDuration);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Frena al instante
        }
    }

    private void UpdateRecovering()
    {
        // Se queda paralizado y vulnerable (como castigo por fallar o atacar)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            currentState = State.Sliding;
            
            // Voltea hacia donde esté el jugador antes de empezar a deslizarse otra vez
            if ((player.position.x > transform.position.x && !movingRight) ||
                (player.position.x < transform.position.x && movingRight))
            {
                TurnAround();
            }
        }
    }

    private void JumpTowardsPlayer()
    {
        currentState = State.Jumping;

        Vector2 p0 = transform.position;
        Vector2 pT = player.position;

        // OPCIÓN 1: Sin predicción. Saltamos a la última posición conocida del jugador.
        // Le sumamos un ligero offset en Y (+1.2f) para que el arco del salto sea más alto 
        // y el enemigo caiga claramente "desde arriba" sobre la cabeza del jugador.
        Vector2 target = new Vector2(pT.x, pT.y + 1.2f);

        // Ecuaciones de Movimiento Parabólico para alcanzar el target
        float gravity = Physics2D.gravity.y * rb.gravityScale;
        
        float vx = (target.x - p0.x) / jumpAirTime;
        float vy = (target.y - p0.y - 0.5f * gravity * jumpAirTime * jumpAirTime) / jumpAirTime;

        // Clamp de velocidades por seguridad
        vx = Mathf.Clamp(vx, -25f, 25f);
        vy = Mathf.Clamp(vy, -10f, 30f); 

        rb.linearVelocity = new Vector2(vx, vy);
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public override void TurnAround()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
        ResetTurnTimer();
    }

    private void ResetTurnTimer()
    {
        turnTimer = Random.Range(minTimeBeforeTurn, maxTimeBeforeTurn);
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (movingRight ? Vector3.right : Vector3.left) * checkDistance);
        }

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * checkDistance);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, jumpRange);
    }
}
