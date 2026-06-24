using UnityEngine;
using UnityEngine.InputSystem; // Importamos el nuevo Input System
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class HubPlayerController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float velocidad = 5f;

    private Rigidbody2D rb;
    private Vector2 movimientoInput;
    private Animator animator;

    // Guardar la última dirección de movimiento para las animaciones de reposo (Idle)
    private float lastHorizontal = 0f;
    private float lastVertical = -1f; // Por defecto mirando hacia abajo (mirada inicial)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Si el juego está pausado, congelar movimiento y animaciones
        if (Time.timeScale == 0f)
        {
            movimientoInput = Vector2.zero;
            ActualizarAnimaciones(0f, 0f);
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        // Leer directamente desde el teclado usando el nuevo Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                vertical = 1f;
            }
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                vertical = -1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal = 1f;
            }
            else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal = -1f;
            }
        }

        // Opcional: Leer también desde un mando (Gamepad)
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.magnitude > 0.2f) // Zona muerta para evitar drift
            {
                // Si el joystick se inclina más en horizontal que en vertical
                if (Mathf.Abs(stick.x) > Mathf.Abs(stick.y))
                {
                    horizontal = stick.x > 0 ? 1f : -1f;
                }
                else
                {
                    vertical = stick.y > 0 ? 1f : -1f;
                }
            }
        }

        // Combinamos la entrada y la normalizamos para que al ir en diagonal no duplique la velocidad
        movimientoInput = new Vector2(horizontal, vertical);
        if (movimientoInput.sqrMagnitude > 1f)
        {
            movimientoInput.Normalize();
        }

        // Si el jugador se está moviendo, actualizamos la última dirección registrada
        if (movimientoInput.sqrMagnitude > 0.01f)
        {
            lastHorizontal = horizontal;
            lastVertical = vertical;
        }

        ActualizarAnimaciones(horizontal, vertical);
    }

    private void FixedUpdate()
    {
        // Mover el Rigidbody2D
        rb.linearVelocity = movimientoInput * velocidad;
    }

    private void ActualizarAnimaciones(float horizontal, float vertical)
    {
        if (animator == null) return;

        // Si hay movimiento (magnitud mayor a cero)
        bool estaMoviendose = movimientoInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsMoving", estaMoviendose);

        // Aseguramos que la velocidad del reproductor sea 1
        animator.speed = 1f;

        // Enviamos las direcciones de movimiento actuales (para el Blend Tree de Caminar)
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        // Enviamos la última dirección registrada (para el Blend Tree de Idle)
        animator.SetFloat("LastHorizontal", lastHorizontal);
        animator.SetFloat("LastVertical", lastVertical);
    }
}
