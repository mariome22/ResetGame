using UnityEngine;

public class EnemyPredictiveShooterPlatformer : EnemyPlatformerBase
{
    [Header("Patrulla")]
    public float patrolSpeed = 3f;
    public Transform ledgeCheck; 
    public Transform wallCheck;  
    public float checkDistance = 0.5f;
    public LayerMask groundLayer;
    [Tooltip("¿Empieza moviéndose o mirando hacia la derecha? Si lo desmarcas, empezará mirando y moviéndose a la izquierda.")]
    public bool startMovingRight = true;

    [Header("Comportamiento Aleatorio")]
    public float minTimeBeforeTurn = 2f;
    public float maxTimeBeforeTurn = 6f;
    private float turnTimer;

    [Header("Disparo Predictivo")]
    public GameObject projectilePrefab;
    public float fireRate = 2f;
    public float visionRange = 7f;
    public float projectileSpeed = 7f; // Debe coincidir con la del proyectil
    public LayerMask visionBlockingLayer;
    public Transform firePoint;

    private Rigidbody2D rb;
    private Animator anim;
    private bool movingRight = true;
    private float fireCooldown;
    private Transform player;
    private Rigidbody2D playerRb;

    protected override void Start()
    {
        base.Start(); 
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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (player == null) return;

        if (fireCooldown > 0)
        {
            fireCooldown -= Time.deltaTime;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= visionRange)
        {
            // Chequear linea de vision
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, visionBlockingLayer);
            
            if (hit.collider == null || hit.collider.CompareTag("Player"))
            {
                // El jugador esta en rango y con linea de vision
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Parar para disparar
                
                // Mirar al jugador
                if ((player.position.x > transform.position.x && !movingRight) || 
                    (player.position.x < transform.position.x && movingRight))
                {
                    TurnAround();
                }

                if (fireCooldown <= 0)
                {
                    FireProjectilePredictively();
                    fireCooldown = fireRate;
                }

                if (anim != null)
                {
                    anim.SetBool("IsWalking", false);
                    anim.SetBool("IsAction", true);
                }
                return; // Salir del Update para no ejecutar la logica de patrulla
            }
        }

        // --- Logica de Patrulla ---
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * patrolSpeed, rb.linearVelocity.y);

        if (patrolSpeed > 0.01f)
        {
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
        }

        if (anim != null)
        {
            anim.SetBool("IsWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            anim.SetBool("IsAction", false);
        }
    }

    private void FireProjectilePredictively()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Vector2 playerPos = player.position;
        Vector2 playerVel = playerRb != null ? playerRb.linearVelocity : Vector2.zero;
        Vector2 firePos = firePoint.position;

        Vector2 predictedPos = CalculateInterceptionPoint(firePos, playerPos, playerVel, projectileSpeed);

        // --- PREVENCIÓN DE DISPARO TRASERO Y ÁNGULOS EXTREMOS ---
        // Si el jugador se mueve rápido hacia el enemigo, la predicción matemática puede dar un punto
        // situado detrás del enemigo o extremadamente cerca de su espalda, haciendo que dispare de espaldas.
        float margin = 0.5f; // Margen en unidades de Unity para evitar disparos hacia su propio cuerpo o espalda
        float currentRelativeX = playerPos.x - transform.position.x;
        float predictedRelativeX = predictedPos.x - transform.position.x;

        // Si el jugador está a la izquierda y la predicción cruza al lado derecho (o se acerca demasiado)
        if (currentRelativeX < 0 && predictedRelativeX > -margin)
        {
            predictedPos = playerPos; // Cancelamos predicción y disparamos a su posición actual
        }
        // Si el jugador está a la derecha y la predicción cruza al lado izquierdo (o se acerca demasiado)
        else if (currentRelativeX > 0 && predictedRelativeX < margin)
        {
            predictedPos = playerPos; // Cancelamos predicción y disparamos a su posición actual
        }

        Vector2 fireDir = (predictedPos - firePos).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePos, Quaternion.identity);
        EnemyProjectilePlatformer projScript = proj.GetComponent<EnemyProjectilePlatformer>();
        if (projScript != null)
        {
            projScript.speed = projectileSpeed; 
            projScript.Fire(fireDir);
        }
    }

    private Vector2 CalculateInterceptionPoint(Vector2 shooterPos, Vector2 targetPos, Vector2 targetVel, float projSpeed)
    {
        Vector2 relativePos = targetPos - shooterPos;
        
        float a = projSpeed * projSpeed - targetVel.sqrMagnitude;
        float b = -2f * Vector2.Dot(relativePos, targetVel);
        float c = -relativePos.sqrMagnitude;

        if (Mathf.Abs(a) < 0.001f)
        {
            if (Mathf.Abs(b) > 0.001f)
            {
                float t = -c / b;
                if (t > 0) return targetPos + targetVel * t;
            }
            return targetPos;
        }

        float discriminant = b * b - 4f * a * c;

        if (discriminant < 0f)
        {
            return targetPos; // No se puede interceptar
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2f * a);
        float t2 = (-b - sqrtDiscriminant) / (2f * a);

        float interceptionTime = -1f;

        if (t1 > 0f && t2 > 0f) interceptionTime = Mathf.Min(t1, t2);
        else if (t1 > 0f) interceptionTime = t1;
        else if (t2 > 0f) interceptionTime = t2;

        if (interceptionTime > 0f)
        {
            return targetPos + targetVel * interceptionTime;
        }

        return targetPos;
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
