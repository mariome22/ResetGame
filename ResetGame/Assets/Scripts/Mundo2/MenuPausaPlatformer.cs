using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausaPlatformer : MonoBehaviour
{
    [Header("UI del Menú de Pausa")]
    [Tooltip("El GameObject del Canvas o panel principal de pausa en Mundo 2")]
    [SerializeField] private GameObject canvasPausa;

    [Header("Confirmación de Salida")]
    [Tooltip("Opcional: Panel de confirmación para guardar antes de salir")]
    [SerializeField] private GameObject panelConfirmacionSalir;
    [SerializeField] private Button botonGuardarYSalir;
    [SerializeField] private Button botonSalirSinGuardar;
    [SerializeField] private Button botonCancelarSalir;

    private bool estaPausado = false;
    private string escenaDestinoPending = "";

    private void Start()
    {
        // Asegurarnos de que los paneles empiecen cerrados al iniciar el nivel
        if (canvasPausa != null) canvasPausa.SetActive(false);
        if (panelConfirmacionSalir != null) panelConfirmacionSalir.SetActive(false);

        // Configurar botones de confirmación
        Debug.Log($"[MenuPausaPlatformer] Inicializando menú de pausa en '{gameObject.name}'...");
        if (botonGuardarYSalir != null)
        {
            botonGuardarYSalir.onClick.RemoveAllListeners();
            botonGuardarYSalir.onClick.AddListener(ConfirmarGuardarYSalir);
        }
        if (botonSalirSinGuardar != null)
        {
            botonSalirSinGuardar.onClick.RemoveAllListeners();
            botonSalirSinGuardar.onClick.AddListener(ConfirmarSalirSinGuardar);
        }
        if (botonCancelarSalir != null)
        {
            botonCancelarSalir.onClick.RemoveAllListeners();
            botonCancelarSalir.onClick.AddListener(CancelarSalir);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Abrir/Cerrar menú al presionar la tecla I
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            // Si el panel de confirmación está abierto, al presionar I lo cancelamos en lugar de cerrar toda la pausa
            if (estaPausado && panelConfirmacionSalir != null && panelConfirmacionSalir.activeSelf)
            {
                CancelarSalir();
            }
            else
            {
                AlternarPausa();
            }
        }
    }

    /// <summary>
    /// Activa o desactiva la pausa del juego.
    /// </summary>
    public void AlternarPausa()
    {
        if (canvasPausa == null) return;

        estaPausado = !estaPausado;

        if (estaPausado)
        {
            // Pausar tiempo y activar la UI
            canvasPausa.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            // Reanudar tiempo y desactivar la UI
            canvasPausa.SetActive(false);
            if (panelConfirmacionSalir != null) panelConfirmacionSalir.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Método público para conectar al botón 'Reanudar' de la UI.
    /// </summary>
    public void ReanudarJuego()
    {
        if (estaPausado)
        {
            AlternarPausa();
        }
    }

    /// <summary>
    /// Prepara la salida mostrando el panel de confirmación si está asignado.
    /// </summary>
    private void PrepararSalida(string escenaDestino)
    {
        Debug.Log($"[MenuPausaPlatformer] PrepararSalida hacia '{escenaDestino}'. panelConfirmacionSalir: {panelConfirmacionSalir != null}");
        if (panelConfirmacionSalir != null)
        {
            escenaDestinoPending = escenaDestino;
            panelConfirmacionSalir.SetActive(true);
            Debug.Log("[MenuPausaPlatformer] Panel de confirmación activado.");
        }
        else
        {
            Debug.Log("[MenuPausaPlatformer] Saliendo directamente sin confirmación.");
            Time.timeScale = 1f;
            SceneManager.LoadScene(escenaDestino);
        }
    }

    public void SalirAlHub()
    {
        PrepararSalida("01_Hub");
    }

    public void SalirAlMenuPrincipal()
    {
        PrepararSalida("MainMenu");
    }

    private void ConfirmarGuardarYSalir()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaDestinoPending);
    }

    private void ConfirmarSalirSinGuardar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaDestinoPending);
    }

    private void CancelarSalir()
    {
        if (panelConfirmacionSalir != null)
        {
            panelConfirmacionSalir.SetActive(false);
        }
        escenaDestinoPending = "";
    }
}
