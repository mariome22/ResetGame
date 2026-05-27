using UnityEngine;

public class HorizontalCameraTarget : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El jugador a seguir.")]
    [SerializeField] private Transform playerTransform;

    [Header("Configuración Eje Y")]
    [Tooltip("¿Usar la altura Y actual al iniciar el juego como la altura fija?")]
    [SerializeField] private bool useStartingY = true;
    
    [Tooltip("La altura fija del eje Y si no se usa la de inicio.")]
    [SerializeField] private float fixedYPosition = 0f;

    private float lockedY;

    private void Start()
    {
        if (useStartingY)
        {
            lockedY = transform.position.y;
        }
        else
        {
            lockedY = fixedYPosition;
        }

        // Si no se ha asignado el jugador, intentamos buscarlo por Tag
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // El punto sigue exactamente la posición X del jugador, pero mantiene la Y bloqueada
        transform.position = new Vector3(playerTransform.position.x, lockedY, transform.position.z);
    }
}
