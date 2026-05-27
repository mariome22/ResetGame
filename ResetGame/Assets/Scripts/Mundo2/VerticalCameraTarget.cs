using UnityEngine;

public class VerticalCameraTarget : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El jugador a seguir.")]
    [SerializeField] private Transform playerTransform;

    [Header("Configuración Eje X")]
    [Tooltip("¿Usar la posición X actual al iniciar el juego como la posición X fija?")]
    [SerializeField] private bool useStartingX = true;
    
    [Tooltip("La posición fija del eje X si no se usa la de inicio (centro horizontal de la torre).")]
    [SerializeField] private float fixedXPosition = 0f;

    [Header("Límites del Eje Y")]
    [Tooltip("¿Limitar la altura mínima a la que puede bajar el objetivo de la cámara? (Útil para no ver el suelo inferior).")]
    [SerializeField] private bool clampMinY = false;
    [Tooltip("La altura Y mínima permitida para la cámara (puedes calcularla sumando el Orthographic Size al nivel del suelo).")]
    [SerializeField] private float minYPosition = 0f;

    private float lockedX;

    private void Start()
    {
        if (useStartingX)
        {
            lockedX = transform.position.x;
        }
        else
        {
            lockedX = fixedXPosition;
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

        // Calculamos la posición Y del jugador
        float targetY = playerTransform.position.y;

        // Si el límite mínimo está activado, evitamos que baje de ahí
        if (clampMinY)
        {
            targetY = Mathf.Max(targetY, minYPosition);
        }

        // El punto sigue exactamente la posición Y calculada, pero mantiene la X bloqueada en el centro de la torre
        transform.position = new Vector3(lockedX, targetY, transform.position.z);
    }
}
