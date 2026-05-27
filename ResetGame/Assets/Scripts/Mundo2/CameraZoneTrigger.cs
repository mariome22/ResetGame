using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Cámaras a Intercambiar")]
    [Tooltip("La cámara de la que venimos (se desactivará o bajará de prioridad).")]
    [SerializeField] private GameObject cameraToDeactivate;
    
    [Tooltip("La cámara a la que queremos cambiar (se activará o subirá de prioridad).")]
    [SerializeField] private GameObject cameraToActivate;

    [Header("Configuración del Trigger")]
    [Tooltip("Si está activado, la cámara volverá al estado original cuando el jugador salga del trigger.")]
    [SerializeField] private bool revertOnExit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos si es el jugador quien entra al trigger
        if (other.CompareTag("Player"))
        {
            SwitchCamera(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (revertOnExit && other.CompareTag("Player"))
        {
            SwitchCamera(false);
        }
    }

    private void SwitchCamera(bool enterZone)
    {
        if (cameraToActivate == null || cameraToDeactivate == null)
        {
            Debug.LogWarning("Por favor, asigna ambas cámaras en el componente CameraZoneTrigger.", this);
            return;
        }

        if (enterZone)
        {
            // Activar la nueva cámara y desactivar la anterior
            // Nota: Cinemachine detecta automáticamente la activación y hace una transición suave (blend)
            cameraToActivate.SetActive(true);
            cameraToDeactivate.SetActive(false);
        }
        else
        {
            // Volver al estado anterior
            cameraToActivate.SetActive(false);
            cameraToDeactivate.SetActive(true);
        }
    }
}
