using UnityEngine;

public class CameraTargetFollower : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El personaje del jugador a seguir.")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerPlatformerController playerController;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Ajustes de Plataforma (Salto)")]
    [Tooltip("La altura máxima que el jugador puede subir respecto al suelo antes de que la cámara empiece a seguirle hacia arriba (evita que salga de la pantalla por arriba).")]
    [SerializeField] private float maxJumpHeightBeforeFollow = 4.5f;

    [Header("Ajustes de Caída")]
    [Tooltip("Distancia extra que se desplazará la cámara hacia abajo cuando el jugador esté cayendo para anticipar obstáculos.")]
    [SerializeField] private float fallOffset = 2.5f;
    [Tooltip("Velocidad a la que la cámara se desplaza hacia el offset de caída.")]
    [SerializeField] private float fallOffsetSmoothSpeed = 4f;

    private float currentYOffset = 0f;
    private float groundY;

    private void Start()
    {
        if (playerTransform != null)
        {
            if (playerController == null) playerController = playerTransform.GetComponent<PlayerPlatformerController>();
            if (playerRb == null) playerRb = playerTransform.GetComponent<Rigidbody2D>();
            
            groundY = playerTransform.position.y;
            transform.position = new Vector3(playerTransform.position.x, groundY, transform.position.z);
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. El eje X siempre sigue al jugador al instante
        float targetX = playerTransform.position.x;

        // 2. Lógica del eje Y
        float targetY = transform.position.y;

        bool isGrounded = playerController != null ? playerController.IsGrounded : true;
        float verticalVelocity = playerRb != null ? playerRb.linearVelocity.y : 0f;

        if (isGrounded)
        {
            // En el suelo: la altura de referencia es la altura actual del jugador
            groundY = playerTransform.position.y;
            
            // Retornamos el offset de caída suavemente a 0
            currentYOffset = Mathf.MoveTowards(currentYOffset, 0f, Time.deltaTime * fallOffsetSmoothSpeed);
            targetY = groundY + currentYOffset;
        }
        else
        {
            if (verticalVelocity < -0.1f)
            {
                // CAYENDO: La cámara sigue al jugador hacia abajo inmediatamente
                // Además, aplicamos el offset hacia abajo de forma suave para tener un "look ahead" vertical
                float targetOffset = -fallOffset;
                currentYOffset = Mathf.MoveTowards(currentYOffset, targetOffset, Time.deltaTime * fallOffsetSmoothSpeed);
                
                targetY = playerTransform.position.y + currentYOffset;
            }
            else
            {
                // SUBIENDO/SALTANDO: Bloqueamos el eje Y del objetivo en el último suelo pisado.
                // Esto hace que la cámara NO suba con los saltos estándar.
                
                // Medida de seguridad: Si el jugador sube más de la cuenta (plataformas móviles, rebotadores, saltos super altos),
                // desplazamos la referencia hacia arriba para que no se salga de la pantalla por la parte superior.
                if (playerTransform.position.y > groundY + maxJumpHeightBeforeFollow)
                {
                    groundY = playerTransform.position.y - maxJumpHeightBeforeFollow;
                }

                currentYOffset = Mathf.MoveTowards(currentYOffset, 0f, Time.deltaTime * fallOffsetSmoothSpeed);
                targetY = groundY + currentYOffset;
            }
        }

        // 3. Aplicamos la posición calculada al objeto "Shadow Target"
        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }
}
