using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float velocidad = 5f;

    private Rigidbody2D rb;
    private Animator anim; // <-- AÃ‘ADIDO: Referencia al cerebro de las animaciones
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

    [Header("Ajustes de Ataque a Distancia")]
    public GameObject prefabProyectil;
    [SerializeField] private float velocidadProyectil = 15f;
    private bool usandoArmaADistancia = false;
    private bool armaDesbloqueada = false;

        [Header("Ajustes del Cargador")]
    public int balasMaximasCargador = 10;
    public int balasActualesCargador;
    public float cadenciaDisparo = 0.5f;
    private float tiempoSiguienteDisparo = 0f;

    private Camera cam;

    private void Awake()
    {
        balasActualesCargador = balasMaximasCargador;
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        anim = GetComponent<Animator>(); // <-- AÃ‘ADIDO: Buscamos el Animator al arrancar
    }

    private void Update()
    {
        CalcularDireccionMirada();

        // <-- AÃ‘ADIDO: Le pasamos al Animator cuÃ¡nto nos estamos moviendo (0 = quieto, >0 = corriendo)
        if (anim != null)
        {
            anim.SetFloat("Velocidad", movimientoInput.sqrMagnitude);
        }
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

    
    public void OnReload(InputValue value)
    {
        if (value.isPressed && usandoArmaADistancia)
        {
            if (balasActualesCargador < balasMaximasCargador)
            {
                int balasFaltantes = balasMaximasCargador - balasActualesCargador;
                int recargadas;
                if (InventarioManager.Instance != null && InventarioManager.Instance.ExtraerMunicion(balasFaltantes, out recargadas))
                {
                    balasActualesCargador += recargadas;
                    Debug.Log("Recargado. Balas actuales: " + balasActualesCargador);
                }
                else
                {
                    Debug.Log("No tienes municion en el Inventario para recargar.");
                }
            }
            else
            {
                Debug.Log("El cargador ya esta lleno.");
            }
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && !isDashing)
        {
            if (usandoArmaADistancia)
            {
                if (Time.time >= tiempoSiguienteDisparo)
                {
                    if (balasActualesCargador > 0)
                    {
                        tiempoSiguienteDisparo = Time.time + cadenciaDisparo;
                        RealizarDisparo();
                    }
                    else
                    {
                        Debug.Log("Sin balas. Pulsa R para recargar.");
                    }
                }
            }
            else
            {
                RealizarAtaque();
            }
        }
    }

    public void OnSwitchWeapon(InputValue value)
    {
        if (value.isPressed && armaDesbloqueada)
        {
            usandoArmaADistancia = !usandoArmaADistancia;
            Debug.Log("Arma cambiada. Arma a distancia equipada: " + usandoArmaADistancia);
        }
        else if (value.isPressed && !armaDesbloqueada)
        {
            Debug.Log("TodavÃ­a no tienes el arma a distancia.");
        }
    }

    public void DesbloquearArmaADistancia()
    {
        armaDesbloqueada = true;
        usandoArmaADistancia = true; // El jugador prefiere que se equipe automÃ¡ticamente
        Debug.Log("Arma a distancia desbloqueada y equipada: " + usandoArmaADistancia);
    }

    private void RealizarDisparo()
    {
        balasActualesCargador--;
        if (prefabProyectil != null)
        {
            // Instanciamos el proyectil en la posiciÃ³n actual del jugador
            GameObject proyectilObj = Instantiate(prefabProyectil, transform.position, Quaternion.identity);
            PlayerProjectile proyectil = proyectilObj.GetComponent<PlayerProjectile>();
            
            if (proyectil != null)
            {
                // Inicializamos el proyectil para que se mueva en la direcciÃ³n que mira el jugador
                proyectil.Inicializar(direccionMirada, velocidadProyectil);
            }
        }
        else
        {
            Debug.LogWarning("No hay prefab de proyectil asignado en el PlayerController.");
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

        if (prefabEfectoAtaque != null)
        {
            //Instanciamos el sprite del slash
            GameObject efecto = Instantiate(prefabEfectoAtaque, transform.position, Quaternion.identity);

            //Calculamos el Ã¡ngulo hacia el ratÃ³n
            float anguloCentral = Mathf.Atan2(direccionMirada.y, direccionMirada.x) * Mathf.Rad2Deg;

            //Iniciamos la animaciÃ³n
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
            else
            {
                ObjetoRompible caja = objeto.GetComponent<ObjetoRompible>();
                if (caja != null)
                {
                    caja.RecibirDano(1);
                }
            }
        }
    }

    private IEnumerator AnimarTajoVisual(GameObject tajo, float anguloCentral)
    {
        float tiempo = 0f;
        float duracion = 0.15f;

        //El slash harÃ¡ un recorrido en arco de 60 grados
        float anguloInicio = anguloCentral - 30;
        float anguloFin = anguloCentral + 30;

        while (tiempo < duracion)
        {
            if (tajo == null) break;

            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;

            float anguloActual = Mathf.Lerp(anguloInicio, anguloFin, progreso);
            tajo.transform.rotation = Quaternion.Euler(0, 0, anguloActual);

            Vector2 direccionActual = new Vector2(Mathf.Cos(anguloActual * Mathf.Deg2Rad), Mathf.Sin(anguloActual * Mathf.Deg2Rad));

            //Para que el slash se mueva con el jugador y no se quede atras
            tajo.transform.position = (Vector2)transform.position + (direccionActual * distanciaVisualTajo);

            yield return null;
        }

        if (tajo != null) Destroy(tajo);
    }

    public void OnSwitchItem(InputValue value)
    {
        if (value.isPressed)
        {
            if (InventarioManager.Instance != null)
            {
                InventarioManager.Instance.CambiarSeleccion();
            }
        }
    }

    public void OnHeal(InputValue value)
    {
        if (value.isPressed)
        {
            if (InventarioManager.Instance != null)
            {
                InventarioManager.Instance.UsarObjetoSeleccionado();
            }
        }
    }
    private void OnDrawGizmos()
    {
        Vector2 direccionVisual = (Application.isPlaying) ? direccionMirada : Vector2.right;
        Vector2 centroDelAtaque = (Vector2)transform.position + (direccionVisual * distanciaAtaque);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centroDelAtaque, rangoAtaque);
    }
}

