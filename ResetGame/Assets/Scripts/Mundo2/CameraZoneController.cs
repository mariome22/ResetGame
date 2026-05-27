using UnityEngine;

public class CameraZoneController : MonoBehaviour
{
    [Header("Configuración de la Zona")]
    [Tooltip("La cámara que debe activarse cuando el jugador entra en esta zona.")]
    [SerializeField] private GameObject zoneCamera;

    [Tooltip("Lista de otras cámaras del nivel que deben desactivarse al entrar en esta zona.")]
    [SerializeField] private GameObject[] otherCamerasToDeactivate;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el objeto que entra es el jugador
        if (other.CompareTag("Player"))
        {
            ActivateZoneCamera();
        }
    }

    private void ActivateZoneCamera()
    {
        if (zoneCamera == null)
        {
            Debug.LogWarning($"No se ha asignado la cámara de la zona en el objeto {gameObject.name}", this);
            return;
        }

        // Activamos la cámara de esta zona
        zoneCamera.SetActive(true);

        // Desactivamos todas las demás cámaras configuradas
        if (otherCamerasToDeactivate != null)
        {
            foreach (GameObject cam in otherCamerasToDeactivate)
            {
                if (cam != null)
                {
                    cam.SetActive(false);
                }
            }
        }
    }
}
