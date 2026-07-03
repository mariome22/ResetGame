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

    [Header("Panel de Controles")]
    [Tooltip("Panel de controles del Mundo 2 que se puede abrir desde la pausa")]
    [SerializeField] private GameObject panelControles;

    private bool estaPausado = false;
    private string escenaDestinoPending = "";
    private bool canvasEsElMismoObjeto = false;

    private void Start()
    {
        canvasEsElMismoObjeto = (canvasPausa == gameObject);

        // Asegurarnos de que los paneles empiecen cerrados al iniciar el nivel
        if (canvasPausa != null)
        {
            if (canvasEsElMismoObjeto)
            {
                Debug.LogWarning($"[MenuPausaPlatformer] Advertencia en '{gameObject.name}': El script 'MenuPausaPlatformer' está en el mismo GameObject que 'canvasPausa'. Se gestionará desactivando sus elementos hijos para mantener el script activo.");
                SetChildrenActive(canvasPausa, false);
            }
            else
            {
                canvasPausa.SetActive(false);
            }
        }
        if (panelConfirmacionSalir != null) panelConfirmacionSalir.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);

        // Auto-buscar botones si no están asignados en el inspector
        if (panelConfirmacionSalir != null)
        {
            if (botonGuardarYSalir == null) botonGuardarYSalir = BuscarBotonEnObjeto(panelConfirmacionSalir, "Guardar");
            if (botonSalirSinGuardar == null) botonSalirSinGuardar = BuscarBotonEnObjeto(panelConfirmacionSalir, "No_Guardar");
            if (botonCancelarSalir == null) botonCancelarSalir = BuscarBotonEnObjeto(panelConfirmacionSalir, "Cerrar");
        }

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

    private Button BuscarBotonEnObjeto(GameObject parent, string nombreBoton)
    {
        foreach (Button b in parent.GetComponentsInChildren<Button>(true))
        {
            if (b.name == nombreBoton)
            {
                return b;
            }
        }
        return null;
    }

    private void Update()
    {
        bool iPressed = false;
        bool escPressed = false;

        if (Keyboard.current != null)
        {
            iPressed = Keyboard.current.iKey.wasPressedThisFrame;
            escPressed = Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame;
        }
        else
        {
            // Fallback al Input System antiguo si Keyboard.current es nulo
            iPressed = Input.GetKeyDown(KeyCode.I);
            escPressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
        }

        // Log temporal de depuración para la tecla ESC/I
        if (iPressed || escPressed)
        {
            Debug.Log($"[MenuPausaPlatformer] Se pulsó ESC o I. estaPausado: {estaPausado}, IsSelectorOpen: {LevelSelectorController.IsSelectorOpen}, CerradoEsteFrame: {LevelSelectorController.CerradoEsteFrame}");
        }

        if (LevelSelectorController.IsSelectorOpen || LevelSelectorController.CerradoEsteFrame) return;

        // Abrir/Cerrar menú al presionar la tecla I o ESC
        if (iPressed || escPressed)
        {
            // Si el panel de controles está abierto, lo cerramos
            if (estaPausado && panelControles != null && panelControles.activeSelf)
            {
                CerrarControles();
            }
            // Si el panel de confirmación está abierto, lo cancelamos en lugar de cerrar toda la pausa
            else if (estaPausado && panelConfirmacionSalir != null && panelConfirmacionSalir.activeSelf)
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
    private void SetChildrenActive(GameObject parent, bool active)
    {
        if (parent == null) return;
        foreach (Transform child in parent.transform)
        {
            if (!active && (child.gameObject == panelConfirmacionSalir || child.gameObject == panelControles))
            {
                child.gameObject.SetActive(false);
                continue;
            }
            child.gameObject.SetActive(active);
        }
    }

    public void AlternarPausa()
    {
        if (canvasPausa == null) return;

        estaPausado = !estaPausado;

        if (estaPausado)
        {
            if (canvasEsElMismoObjeto)
            {
                SetChildrenActive(canvasPausa, true);
            }
            else
            {
                canvasPausa.SetActive(true);
            }
            Time.timeScale = 0f;
        }
        else
        {
            if (canvasEsElMismoObjeto)
            {
                SetChildrenActive(canvasPausa, false);
            }
            else
            {
                canvasPausa.SetActive(false);
            }
            if (panelConfirmacionSalir != null) panelConfirmacionSalir.SetActive(false);
            if (panelControles != null) panelControles.SetActive(false);
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
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade(escenaDestino);
            }
            else
            {
                SceneManager.LoadScene(escenaDestino);
            }
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
        Debug.Log($"[MenuPausaPlatformer] ConfirmarGuardarYSalir llamada. SaveManager.Instance es nulo?: {SaveManager.Instance == null}");
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogError("[MenuPausaPlatformer] ¡No se pudo guardar la partida porque SaveManager.Instance es NULL!");
        }
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(escenaDestinoPending);
        }
        else
        {
            SceneManager.LoadScene(escenaDestinoPending);
        }
    }

    private void ConfirmarSalirSinGuardar()
    {
        Debug.Log("[MenuPausaPlatformer] ConfirmarSalirSinGuardar llamada.");
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(escenaDestinoPending);
        }
        else
        {
            SceneManager.LoadScene(escenaDestinoPending);
        }
    }

    private void CancelarSalir()
    {
        if (panelConfirmacionSalir != null)
        {
            panelConfirmacionSalir.SetActive(false);
        }
        escenaDestinoPending = "";
    }

    public void AbrirControles()
    {
        if (panelControles != null)
        {
            panelControles.SetActive(true);
        }
    }

    public void CerrarControles()
    {
        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }
    }
}
