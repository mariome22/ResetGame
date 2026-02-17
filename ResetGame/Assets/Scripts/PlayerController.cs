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
}