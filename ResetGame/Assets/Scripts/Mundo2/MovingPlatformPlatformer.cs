using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformPlatformer : MonoBehaviour
{
    public enum MovementDirection { Horizontal, Vertical }

    [Header("Ruta de Movimiento")]
    [Tooltip("Dirección del movimiento: Horizontal (izquierda/derecha) o Vertical (arriba/abajo)")]
    public MovementDirection movementDirection = MovementDirection.Horizontal;

    [Tooltip("Distancia en casillas (unidades) que recorrerá la plataforma desde su punto inicial")]
    public float distance = 3f;
    [Tooltip("Velocidad de movimiento de la plataforma")]
    public float speed = 2f;
    [Tooltip("¿Empieza a moverse en sentido positivo? (Derecha en Horizontal, Arriba en Vertical)")]
    [UnityEngine.Serialization.FormerlySerializedAs("startMovingRight")]
    public bool startMovingRightOrUp = true;

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
        
        direction = startMovingRightOrUp ? 1 : -1;
    }

    private void FixedUpdate()
    {
        if (movementDirection == MovementDirection.Horizontal)
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

            // Aplicamos la velocidad horizontal al Rigidbody
            rb.linearVelocity = new Vector2(direction * speed, 0);
        }
        else
        {
            // Calcular cuánto se ha alejado del centro original en el eje Y
            float currentDistY = transform.position.y - startPosition.y;

            // Si ha llegado al límite superior y se movía hacia arriba, invierte
            if (direction == 1 && currentDistY >= distance)
            {
                direction = -1;
            }
            // Si ha llegado al límite inferior y se movía hacia abajo, invierte
            else if (direction == -1 && currentDistY <= -distance)
            {
                direction = 1;
            }

            // Aplicamos la velocidad vertical al Rigidbody
            rb.linearVelocity = new Vector2(0, direction * speed);
        }
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
        Vector3 point1, point2;

        if (movementDirection == MovementDirection.Horizontal)
        {
            point1 = Application.isPlaying ? startPosition - new Vector3(distance, 0, 0) : transform.position - new Vector3(distance, 0, 0);
            point2 = Application.isPlaying ? startPosition + new Vector3(distance, 0, 0) : transform.position + new Vector3(distance, 0, 0);
        }
        else
        {
            point1 = Application.isPlaying ? startPosition - new Vector3(0, distance, 0) : transform.position - new Vector3(0, distance, 0);
            point2 = Application.isPlaying ? startPosition + new Vector3(0, distance, 0) : transform.position + new Vector3(0, distance, 0);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(point1, point2);
        Gizmos.DrawWireSphere(point1, 0.2f);
        Gizmos.DrawWireSphere(point2, 0.2f);
    }
}
