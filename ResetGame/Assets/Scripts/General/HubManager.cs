using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HubManager : MonoBehaviour
{
    [System.Serializable]
    public class HubPortal
    {
        [Tooltip("El GameObject del portal, puerta o bloqueo que queremos activar/desactivar")]
        public GameObject portalObject;
        [Tooltip("Número de núcleos requeridos para desbloquear este portal")]
        public int nucleosRequeridos;
    }

    [Header("UI del Hub")]
    [Tooltip("Texto para mostrar la cantidad de núcleos recolectados (ej: 'Núcleos: 2')")]
    [SerializeField] private TextMeshProUGUI textoNucleos;

    [Header("Portales y Bloqueos")]
    [Tooltip("Lista de portales y sus requisitos en el Hub")]
    [SerializeField] private List<HubPortal> portalesMundos = new List<HubPortal>();

    private void Start()
    {
        ActualizarEstadoHub();
    }

    /// <summary>
    /// Lee el progreso del jugador y actualiza la UI y los portales disponibles.
    /// </summary>
    public void ActualizarEstadoHub()
    {
        // Obtener núcleos del PlayerPrefs (mantenido sincronizado por SaveManager)
        int nucleosActuales = PlayerPrefs.GetInt("PlayerCores", 0);

        // 1. Actualizar texto de la UI
        if (textoNucleos != null)
        {
            textoNucleos.text = "Núcleos: " + nucleosActuales.ToString();
        }

        // 2. Activar/Desactivar portales según requisitos
        foreach (var portal in portalesMundos)
        {
            if (portal.portalObject != null)
            {
                // Si el jugador tiene los núcleos requeridos, se desbloquea (se activa o desactiva la barrera)
                // Dependiendo de cómo configures el portal:
                // Si el objeto representa el bloqueo (ej: barrera de energía): se desactiva si tiene suficientes núcleos.
                // Si el objeto representa el portal activo: se activa si tiene suficientes núcleos.
                // Por defecto, asumiremos que activamos el portal de acceso si se cumplen los requisitos.
                portal.portalObject.SetActive(nucleosActuales >= portal.nucleosRequeridos);
            }
        }
    }
}
