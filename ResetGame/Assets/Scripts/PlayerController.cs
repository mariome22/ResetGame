using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float velocidad = 5f;

    private Rigidbody2D rb;
    private Vector2 movimientoInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        movimientoInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movimientoInput * velocidad;
    }
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("TECLA RECIBIDA: He pulsado el botón de interactuar.");
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2f);
            Debug.Log("BUSCANDO: He encontrado " + hitColliders.Length + " objetos cerca.");

            foreach (var hitCollider in hitColliders)
            {
                InteractableObject interactable = hitCollider.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    Debug.Log("ÉXITO: ¡Encontré un objeto interactuable! Ejecutando...");
                    interactable.Interact();
                    break;
                }
            }
        }
    }
}