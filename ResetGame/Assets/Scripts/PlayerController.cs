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
    public GameObject prefabEfectoAtaque;
    public float distanciaVisualTajo = 1.2f;

    private Camera cam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        CalcularDireccionMirada();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = movimientoInput * velocidad;
    }

    private void CalcularDireccionMirada()
    {
        Vector2 posicionRatonPantalla = Mouse.current.position.ReadValue();
        Vector3 posicionRatonMundo = cam.ScreenToWorldPoint(posicionRatonPantalla);

        posicionRatonMundo.z = transform.position.z;
        direccionMirada = (posicionRatonMundo - transform.position).normalized;
    }

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

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        Vector2 direccionDash = (movimientoInput != Vector2.zero) ? movimientoInput.normalized : ultimaDireccionTeclado;
        rb.linearVelocity = direccionDash * dashVelocidad;

        yield return new WaitForSeconds(dashDuracion);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void RealizarAtaque()
    {
        Vector2 centroDelAtaque = (Vector2)transform.position + (direccionMirada * distanciaAtaque);

        // --- SOLUCIÓN AL MOVIMIENTO DEL TAJO ---
        if (prefabEfectoAtaque != null)
        {
            // Instanciamos el tajo en el jugador
            GameObject efecto = Instantiate(prefabEfectoAtaque, transform.position, Quaternion.identity);

            // Calculamos el ángulo hacia el ratón
            float anguloCentral = Mathf.Atan2(direccionMirada.y, direccionMirada.x) * Mathf.Rad2Deg;

            // Iniciamos la animación que lo hará moverse
            StartCoroutine(AnimarTajoVisual(efecto, anguloCentral));
        }

        Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(centroDelAtaque, rangoAtaque);

        foreach (Collider2D objeto in objetosGolpeados)
        {
            if (objeto.CompareTag("Enemy"))
            {
                EnemyBase scriptEnemigo = objeto.GetComponent<EnemyBase>();
                if (scriptEnemigo != null)
                {
                    scriptEnemigo.RecibirDano(1);
                }
            }
        }
    }

    // --- RUTINA QUE CREA EL BARRIDO VISUAL Y SIGUE AL JUGADOR ---
    private IEnumerator AnimarTajoVisual(GameObject tajo, float anguloCentral)
    {
        float tiempo = 0f;
        float duracion = 0.15f;

        // El tajo hará un recorrido en arco de 80 grados
        float anguloInicio = anguloCentral - 30;
        float anguloFin = anguloCentral + 30;

        while (tiempo < duracion)
        {
            if (tajo == null) break; // Por si se destruye por error

            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;

            // 1. Calculamos el ángulo actual del barrido
            float anguloActual = Mathf.Lerp(anguloInicio, anguloFin, progreso);
            tajo.transform.rotation = Quaternion.Euler(0, 0, anguloActual);

            // 2. Calculamos las coordenadas de ese ángulo en el espacio
            Vector2 direccionActual = new Vector2(Mathf.Cos(anguloActual * Mathf.Deg2Rad), Mathf.Sin(anguloActual * Mathf.Deg2Rad));

            // 3. Posicionamos el tajo pegado al jugador EN TODO MOMENTO (Si caminas, viaja contigo)
            tajo.transform.position = (Vector2)transform.position + (direccionActual * distanciaVisualTajo);

            yield return null;
        }

        if (tajo != null) Destroy(tajo);
    }

    private void OnDrawGizmos()
    {
        Vector2 direccionVisual = (Application.isPlaying) ? direccionMirada : Vector2.right;
        Vector2 centroDelAtaque = (Vector2)transform.position + (direccionVisual * distanciaAtaque);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centroDelAtaque, rangoAtaque);
    }
}