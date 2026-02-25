using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float velocidad = 5f;

    private Rigidbody2D rb;
    private Vector2 movimientoInput;

    private Vector2 direccionMirada = Vector2.right;
    private Vector2 ultimaDireccionTeclado = Vector2.right;

    [Header("Ajustes de Dash")]
    [SerializeField] private float dashVelocidad = 15f;
    [SerializeField] private float dashDuracion = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Ajustes de Ataque")]
    [SerializeField] private float distanciaAtaque = 1f;
    [SerializeField] private float rangoAtaque = 0.8f;

    // Referencia a la cámara para calcular la posición del ratón
    private Camera cam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main; // Guardamos la cámara principal
    }

    private void Update()
    {
        // Calculamos a dónde mira el ratón constantemente
        CalcularDireccionMirada();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = movimientoInput * velocidad;
    }

    // --- CÁLCULO DEL RATÓN ---
    private void CalcularDireccionMirada()
    {
        // Si tienes el New Input System, leemos la posición del ratón en la pantalla
        Vector2 posicionRatonPantalla = Mouse.current.position.ReadValue();

        // Convertimos esa posición de la pantalla al mundo real del juego
        Vector3 posicionRatonMundo = cam.ScreenToWorldPoint(posicionRatonPantalla);

        // La dirección es: Destino (Ratón) - Origen (Null)
        direccionMirada = (posicionRatonMundo - transform.position).normalized;
    }

    // --- SECCIÓN DE INPUTS ---
    public void OnMove(InputValue value)
    {
        movimientoInput = value.Get<Vector2>();
        if (movimientoInput != Vector2.zero)
        {
            ultimaDireccionTeclado = movimientoInput.normalized;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (isDashing) return;

        if (value.isPressed)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (var hitCollider in hitColliders)
            {
                InteractableObject interactable = hitCollider.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    interactable.Interact();
                    break;
                }
            }
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && !isDashing)
        {
            RealizarAtaque();
        }
    }

    // --- SECCIÓN DE HABILIDADES ---
    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        // El Dash usa el TECLADO (movimientoInput). Si estás quieto, usa la última tecla que pulsaste.
        Vector2 direccionDash = (movimientoInput != Vector2.zero) ? movimientoInput.normalized : ultimaDireccionTeclado;

        rb.linearVelocity = direccionDash * dashVelocidad;

        yield return new WaitForSeconds(dashDuracion);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void RealizarAtaque()
    {
        Debug.Log("Ataque hacia el raton.");

        Vector2 centroDelAtaque = (Vector2)transform.position + (direccionMirada * distanciaAtaque);

        Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(centroDelAtaque, rangoAtaque);

        foreach (Collider2D objeto in objetosGolpeados)
        {
            if (objeto.CompareTag("Enemy"))
            {
                Debug.Log("Le dimos a " + objeto.name);

                EnemyBase scriptEnemigo = objeto.GetComponent<EnemyBase>();
                if (scriptEnemigo != null)
                {
                    scriptEnemigo.RecibirDano(1);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujamos el círculo rojo apuntando siempre hacia el ratón (si el juego está en marcha)
        Vector2 direccionVisual = (Application.isPlaying) ? direccionMirada : Vector2.right;
        Vector2 centroDelAtaque = (Vector2)transform.position + (direccionVisual * distanciaAtaque);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centroDelAtaque, rangoAtaque);
    }
}