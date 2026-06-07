using UnityEngine;

public class GravityZone : MonoBehaviour
{
    [Header("Ajustes de Gravedad Espacial")]
    [Range(0.1f, 1f)]
    [Tooltip("El multiplicador de la gravedad del jugador dentro de esta zona (ej: 0.6 = 60% de gravedad normal, simulando gravedad lunar).")]
    public float gravityScaleMultiplier = 0.65f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerPlatformerController player = other.GetComponent<PlayerPlatformerController>();
        if (player != null)
        {
            // Activar estado de gravedad baja en el jugador y aplicar multiplicador
            player.SetGravityZoneState(true, gravityScaleMultiplier);
            Debug.Log($"[Gravedad Espacial] Jugador entró en zona de gravedad baja. Escala multiplicada por {gravityScaleMultiplier}. Dash bloqueado.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerPlatformerController player = other.GetComponent<PlayerPlatformerController>();
        if (player != null)
        {
            // Desactivar estado de gravedad baja y restaurar valores por defecto
            player.SetGravityZoneState(false, 1f);
            Debug.Log("[Gravedad Espacial] Jugador salió de la zona. Gravedad normal restaurada. Dash desbloqueado.");
        }
    }
}
