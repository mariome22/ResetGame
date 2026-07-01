using UnityEngine;

public class PortalLevelSelector : MonoBehaviour
{
    [Header("Configuración del Portal")]
    [Tooltip("El controlador del selector de niveles que abrirá este portal al entrar en él")]
    [SerializeField] private LevelSelectorController selectorController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobar si quien entra es el jugador
        if (other.CompareTag("Player"))
        {
            if (selectorController != null)
            {
                // Detener velocidades para que no siga moviéndose al abrir la UI (opcional)
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                // Abrir el selector de niveles
                selectorController.AbrirMenu();
            }
            else
            {
                Debug.LogWarning("No se ha asignado un LevelSelectorController en el portal: " + gameObject.name);
            }
        }
    }
}
