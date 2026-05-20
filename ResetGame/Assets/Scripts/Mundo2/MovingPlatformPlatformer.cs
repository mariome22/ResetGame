using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformPlatformer : MonoBehaviour
{
    [Header("Ruta de Movimiento")]
    [Tooltip("Distancia en casillas (unidades) que recorrerá hacia la izquierda y derecha desde su punto inicial")]
    public float distance = 3f;
    [Tooltip("Velocidad de movimiento de la plataforma")]
    public float speed = 2f;
    [Tooltip("¿Empieza a moverse hacia la derecha? Si lo desmarcas, empezará hacia la izquierda.")]
    public bool startMovingRight = true;

    private Vector3 startPosition;
    private Rigidbody2D rb;
    private int direction;

    private void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        
        // Es imperativo que la plataforma sea Kinematic para que la física la mueva sin caerse
        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        direction = startMovingRight ? 1 : -1;
    }

    private void FixedUpdate()
    {
        // Calcular cuánto se ha alejado del centro original en el eje X
        float currentDistX = transform.position.x - startPosition.x;

        // Si ha llegado al límite derecho y se movía a la derecha, invierte
        if (direction == 1 && currentDistX >= distance)
        {
            direction = -1;
        }
        // Si ha llegado al límite izquierdo y se movía a la izquierda, invierte
        else if (direction == -1 && currentDistX <= -distance)
        {
            direction = 1;
        }

        // Aplicamos la velocidad al Rigidbody
        rb.linearVelocity = new Vector2(direction * speed, 0);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si el jugador está tocando la plataforma, le transferimos nuestra velocidad
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerPlatformerController player = collision.gameObject.GetComponent<PlayerPlatformerController>();
            if (player != null && IsPlayerOnTop(collision))
            {
                player.movingPlatformVelocity = rb.linearVelocity;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Si el jugador se baja o salta, le dejamos de aplicar nuestra velocidad
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerPlatformerController player = collision.gameObject.GetComponent<PlayerPlatformerController>();
            if (player != null)
            {
                player.movingPlatformVelocity = Vector2.zero;
            }
        }
    }

    private bool IsPlayerOnTop(Collision2D collision)
    {
        // Asegurarnos de que el jugador está pisando la plataforma desde arriba y no chocando desde abajo o un lado
        ContactPoint2D contact = collision.GetContact(0);
        
        // En Unity 2D, cuando el jugador (other) choca con la plataforma (this), 
        // la normal del contacto suele apuntar desde el jugador hacia la plataforma (es decir, hacia abajo: y negativa).
        return contact.normal.y <= -0.5f;
    }

    private void OnDrawGizmos()
    {
        // Dibujar una línea en el editor para ver exactamente hasta dónde va a llegar la plataforma
        Vector3 leftPoint = Application.isPlaying ? startPosition - new Vector3(distance, 0, 0) : transform.position - new Vector3(distance, 0, 0);
        Vector3 rightPoint = Application.isPlaying ? startPosition + new Vector3(distance, 0, 0) : transform.position + new Vector3(distance, 0, 0);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(leftPoint, rightPoint);
        Gizmos.DrawWireSphere(leftPoint, 0.2f);
        Gizmos.DrawWireSphere(rightPoint, 0.2f);
    }
}
