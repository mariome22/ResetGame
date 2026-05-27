using UnityEngine;

public class EnemyPatrolPlatformer : EnemyPlatformerBase
{
    [Header("Patrulla")]
    public float patrolSpeed = 3f;
    public Transform ledgeCheck; // Objeto vacío situado un poco por delante y por debajo del enemigo
    public Transform wallCheck;  // Objeto vacío situado justo enfrente del enemigo
    public float checkDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Comportamiento Aleatorio")]
    public float minTimeBeforeTurn = 2f;
    public float maxTimeBeforeTurn = 6f;
    private float turnTimer;

    [Header("Sentido de Inicio")]
    [Tooltip("¿Empieza moviéndose o mirando hacia la derecha? Si lo desmarcas, empezará mirando y moviéndose a la izquierda.")]
    public bool startMovingRight = true;

    private Rigidbody2D rb;
    private Animator anim;
    private bool movingRight = true;

    protected override void Start()
    {
        base.Start(); // Llama al Start del padre para inicializar la vida
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        movingRight = startMovingRight;
        if (!movingRight)
        {
            Vector3 scaler = transform.localScale;
            scaler.x = -Mathf.Abs(scaler.x);
            transform.localScale = scaler;
        }
        
        ResetTurnTimer();
    }

    void Update()
    {
        // Si la velocidad es 0, es un enemigo estático: no se mueve ni hace comprobaciones de borde/giros
        if (patrolSpeed <= 0.01f)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null)
            {
                anim.SetBool("IsWalking", false);
            }
            return;
        }

        // 1. Aplicar movimiento constante hacia la dirección actual
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * patrolSpeed, rb.linearVelocity.y);

        // 2. Raycasts de detección
        // Raycast desde la cara del enemigo hacia adelante para detectar paredes
        bool hittingWall = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, checkDistance, groundLayer);

        // Raycast desde el frente-abajo hacia abajo para detectar si hay suelo
        bool hittingLedge = Physics2D.Raycast(ledgeCheck.position, Vector2.down, checkDistance, groundLayer);

        // 3. Girar si hay una pared o si llegamos al borde (no hay suelo)
        if (hittingWall || !hittingLedge)
        {
            TurnAround();
        }

        // 4. Temporizador de giro aleatorio (para que sea impredecible)
        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0)
        {
            TurnAround();
        }

        // 5. Animacion de caminar
        if (anim != null)
        {
            anim.SetBool("IsWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }

    public override void TurnAround()
    {
        movingRight = !movingRight;
        
        // Voltear el sprite y los checks (al hacer flip en el transform, se voltean los hijos automáticamente)
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
        
        ResetTurnTimer(); // Reiniciar el temporizador aleatorio cada vez que gira
    }

    private void ResetTurnTimer()
    {
        turnTimer = Random.Range(minTimeBeforeTurn, maxTimeBeforeTurn);
    }

    // Para ver los rayos en la pestaña Scene y ajustarlos visualmente
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
    }
}
