using UnityEngine;

public class EnemyGrenadierPlatformer : EnemyPlatformerBase
{
    private enum State { Patrolling, Attacking }
    private State currentState = State.Patrolling;

    [Header("Patrulla (Movimiento)")]
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

    [Header("Ataque (Granadas)")]
    public float attackRange = 10f;
    public float timeBetweenShots = 2.5f;
    private float shotTimer;
    
    [Header("Balística")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    [Tooltip("La altura extra (en unidades) que alcanzará el proyectil por encima del punto más alto entre el enemigo y el jugador.")]
    public float arcHeight = 3f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        ResetTurnTimer();
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrolling:
                UpdatePatrolling(dist);
                break;
            case State.Attacking:
                UpdateAttacking(dist);
                break;
        }

        if (anim != null)
        {
            anim.SetBool("IsWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            anim.SetBool("IsAction", currentState == State.Attacking);
        }
    }

    private void UpdatePatrolling(float dist)
    {
        // Movimiento de patrulla
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * patrolSpeed, rb.linearVelocity.y);

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

        // Si el jugador está cerca, paramos y atacamos
        if (dist <= attackRange)
        {
            currentState = State.Attacking;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Frena
            shotTimer = 0.5f; // Primer disparo casi inmediato
            
            // Mirar al jugador
            if ((player.position.x > transform.position.x && !movingRight) ||
                (player.position.x < transform.position.x && movingRight))
            {
                TurnAround();
            }
        }
    }

    private void UpdateAttacking(float dist)
    {
        // Se queda quieto mientras ataca
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Si el jugador se aleja, vuelve a patrullar
        if (dist > attackRange)
        {
            currentState = State.Patrolling;
            ResetTurnTimer();
            return;
        }

        // Mirar siempre al jugador
        if ((player.position.x > transform.position.x && !movingRight) ||
            (player.position.x < transform.position.x && movingRight))
        {
            TurnAround();
        }

        shotTimer -= Time.deltaTime;
        if (shotTimer <= 0)
        {
            FireAtPlayer();
            shotTimer = timeBetweenShots;
        }
    }

    private void FireAtPlayer()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        EnemyBouncingProjectile proj = projObj.GetComponent<EnemyBouncingProjectile>();
        Rigidbody2D projRb = projObj.GetComponent<Rigidbody2D>();

        if (proj == null || projRb == null) return;

        // Evitar que el proyectil choque con el propio enemigo nada más nacer (lo que causaba el fallo en el tiro)
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D projCollider = projObj.GetComponent<Collider2D>();
        if (myCollider != null && projCollider != null)
        {
            Physics2D.IgnoreCollision(myCollider, projCollider);
        }

        Vector2 p0 = firePoint.position;
        Vector2 pT = player.position;

        // --- CÁLCULO DE TIRO PARABÓLICO ---
        // 1. Decidimos la altura máxima del arco (Apex). Debe ser más alta que el jugador y que el propio enemigo.
        float highestY = Mathf.Max(p0.y, pT.y);
        float apexY = highestY + arcHeight;

        // 2. Leemos la gravedad real a la que está sometido el proyectil
        float gravityScale = projRb.gravityScale;
        if (gravityScale <= 0.01f) gravityScale = 1f; // Failsafe
        
        float gravity = Mathf.Abs(Physics2D.gravity.y * gravityScale);
        if (gravity < 0.1f) gravity = 9.81f; // Failsafe secundario

        // 3. Calculamos las distancias verticales desde el Apex hasta el origen (Y1) y hasta el destino (Y2)
        float displacementY1 = apexY - p0.y;
        float displacementY2 = apexY - pT.y;

        if (displacementY1 < 0) displacementY1 = 0;
        if (displacementY2 < 0) displacementY2 = 0;

        // 4. Calculamos la velocidad vertical inicial necesaria para subir hasta esa altura
        float vy = Mathf.Sqrt(2 * gravity * displacementY1);

        // 5. Calculamos el tiempo que tarda en subir al Apex (t_up) y el que tarda en caer al objetivo (t_down)
        float t_up = vy / gravity;
        float t_down = Mathf.Sqrt(2 * displacementY2 / gravity);
        float totalTime = t_up + t_down;

        // 6. Sabiendo el tiempo exacto de vuelo, calculamos la velocidad horizontal constante para llegar al jugador a tiempo
        float vx = (pT.x - p0.x) / totalTime;

        // ¡Fuego!
        proj.Fire(new Vector2(vx, vy));
    }

    private void TurnAround()
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
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
