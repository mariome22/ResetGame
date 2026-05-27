using UnityEngine;

public class EnemyBouncerPlatformer : EnemyPlatformerBase
{
    [Header("Rebote (Bouncer)")]
    [Tooltip("Velocidad a la que se mueve de lado a lado.")]
    public float bounceSpeed = 12f;
    
    [Header("Detección de Paredes")]
    [Tooltip("Objeto vacío colocado justo en el borde frontal del enemigo.")]
    public Transform wallCheck;
    [Tooltip("Distancia para detectar la pared. Si es muy corta y el enemigo va muy rápido, el rayo empezará dentro de la pared y fallará.")]
    public float checkDistance = 0.5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool movingRight = true;

    protected override void Start()
    {
        base.Start(); // Inicializa la vida desde el padre
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Movimiento constante e ininterrumpido
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * bounceSpeed, rb.linearVelocity.y);

        // Detección de muros usando Raycast para evitar el problema de los Tilemaps de Unity
        if (wallCheck != null)
        {
            bool hittingWall = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, checkDistance, groundLayer);
            if (hittingWall)
            {
                TurnAround();
            }
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Ejecutamos la lógica base (daño y muerte por pisotón)
        base.OnCollisionEnter2D(collision);

        // 2. Comprobamos si nos hemos chocado FÍSICAMENTE con el jugador de lado para rebotar
        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                TurnAround();
            }
        }
    }

    public override void TurnAround()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (movingRight ? Vector3.right : Vector3.left) * checkDistance);
        }
    }
}
