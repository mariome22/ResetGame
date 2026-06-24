using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    [Header("Sliders de Volumen")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Botones de Navegación")]
    [SerializeField] private Button botonSalirAlHub;
    [SerializeField] private Button botonSalirAlMenu;

    [Header("Confirmación de Salida")]
    [Tooltip("Opcional: Panel de confirmación para guardar antes de salir")]
    [SerializeField] private GameObject panelConfirmacionSalir;
    [SerializeField] private Button botonGuardarYSalir;
    [SerializeField] private Button botonSalirSinGuardar;
    [SerializeField] private Button botonCancelarSalir;

    private string escenaDestinoPending = "";

    private void Start()
    {
        InitializeOptions();
    }

    private void OnEnable()
    {
        // Volver a cargar los valores por si cambiaron en otra parte
        UpdateUIValues();

        // Asegurarse de que el panel de confirmación comience oculto al abrir opciones
        if (panelConfirmacionSalir != null)
        {
            panelConfirmacionSalir.SetActive(false);
        }
    }

    private void InitializeOptions()
    {
        Debug.Log($"[OptionsManager] Inicializando opciones en el objeto '{gameObject.name}'...");

        if (sliderMusica != null)
        {
            sliderMusica.minValue = 0f;
            sliderMusica.maxValue = 1f;
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sliderSFX != null)
        {
            sliderSFX.minValue = 0f;
            sliderSFX.maxValue = 1f;
            sliderSFX.onValueChanged.RemoveAllListeners();
            sliderSFX.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Si estamos en un nivel de Mundo 2 (con MenuPausaPlatformer), delegamos la navegación a él
        bool isWorld2Platformer = FindFirstObjectByType<MenuPausaPlatformer>() != null;
        if (isWorld2Platformer)
        {
            Debug.Log("[OptionsManager] Detectado MenuPausaPlatformer en escena. Omitiendo configuración de botones de salida en OptionsManager para evitar conflictos.");
            UpdateUIValues();
            return;
        }

        // Auto-buscar panel y botones si no están asignados en el inspector
        if (panelConfirmacionSalir == null)
        {
            panelConfirmacionSalir = BuscarObjetoEnEscena("Panel_Confirmacion");
            if (panelConfirmacionSalir != null)
            {
                Debug.Log($"[OptionsManager] panelConfirmacionSalir auto-asignado al objeto '{panelConfirmacionSalir.name}' en '{gameObject.name}'");
            }
        }

        if (panelConfirmacionSalir != null)
        {
            if (botonGuardarYSalir == null)
            {
                botonGuardarYSalir = BuscarBotonEnObjeto(panelConfirmacionSalir, "Guardar");
                if (botonGuardarYSalir != null) Debug.Log($"[OptionsManager] botonGuardarYSalir auto-asignado en '{gameObject.name}'");
            }
            if (botonSalirSinGuardar == null)
            {
                botonSalirSinGuardar = BuscarBotonEnObjeto(panelConfirmacionSalir, "No_Guardar");
                if (botonSalirSinGuardar != null) Debug.Log($"[OptionsManager] botonSalirSinGuardar auto-asignado en '{gameObject.name}'");
            }
            if (botonCancelarSalir == null)
            {
                botonCancelarSalir = BuscarBotonEnObjeto(panelConfirmacionSalir, "Cerrar");
                if (botonCancelarSalir != null) Debug.Log($"[OptionsManager] botonCancelarSalir auto-asignado en '{gameObject.name}'");
            }
        }

        Debug.Log($"[OptionsManager] Configurando botones de navegación en '{gameObject.name}'. Panel confirmación asignado: {panelConfirmacionSalir != null}");

        if (botonSalirAlHub != null)
        {
            botonSalirAlHub.onClick.RemoveAllListeners();
            botonSalirAlHub.onClick.AddListener(ExitToHub);
        }

        if (botonSalirAlMenu != null)
        {
            botonSalirAlMenu.onClick.RemoveAllListeners();
            botonSalirAlMenu.onClick.AddListener(ExitToMainMenu);
        }

        // Configurar botones de confirmación
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

        UpdateUIValues();
    }

    private void UpdateUIValues()
    {
        if (AudioManager.Instance != null)
        {
            if (sliderMusica != null) sliderMusica.value = AudioManager.Instance.MusicVolume;
            if (sliderSFX != null) sliderSFX.value = AudioManager.Instance.SFXVolume;
        }
    }

    private void OnMusicVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(val);
        }
    }

    private void OnSFXVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(val);
        }
    }

    private void PrepararSalida(string escenaDestino)
    {
        Debug.Log($"[OptionsManager] PrepararSalida hacia '{escenaDestino}' en '{gameObject.name}'. panelConfirmacionSalir asignado inicial: {panelConfirmacionSalir != null}");

        // Doble comprobación y auto-búsqueda en caliente (por si estaba inactivo en Start)
        if (panelConfirmacionSalir == null)
        {
            panelConfirmacionSalir = BuscarObjetoEnEscena("Panel_Confirmacion");
            if (panelConfirmacionSalir != null)
            {
                Debug.Log($"[OptionsManager] panelConfirmacionSalir auto-asignado en caliente al objeto '{panelConfirmacionSalir.name}' en '{gameObject.name}'");
            }
        }

        if (panelConfirmacionSalir != null)
        {
            if (botonGuardarYSalir == null) botonGuardarYSalir = BuscarBotonEnObjeto(panelConfirmacionSalir, "Guardar");
            if (botonSalirSinGuardar == null) botonSalirSinGuardar = BuscarBotonEnObjeto(panelConfirmacionSalir, "No_Guardar");
            if (botonCancelarSalir == null) botonCancelarSalir = BuscarBotonEnObjeto(panelConfirmacionSalir, "Cerrar");

            // Volver a configurar listeners para asegurar que esta instancia específica maneje la confirmación
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

            escenaDestinoPending = escenaDestino;
            panelConfirmacionSalir.SetActive(true);
            Debug.Log($"[OptionsManager] Panel de confirmación activado desde '{gameObject.name}'.");
        }
        else
        {
            Debug.Log($"[OptionsManager] Saliendo directamente sin confirmación desde '{gameObject.name}'.");
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

    public void ExitToHub()
    {
        PrepararSalida("01_Hub");
    }

    public void ExitToMainMenu()
    {
        PrepararSalida("MainMenu");
    }

    private void ConfirmarGuardarYSalir()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        Time.timeScale = 1f; // Reanudar tiempo
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
        Time.timeScale = 1f; // Reanudar tiempo
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

    // Métodos auxiliares de búsqueda robusta en escena (incluyendo inactivos)
    private GameObject BuscarObjetoEnEscena(string nombreObjeto)
    {
        // 1. Intentar buscar en el Canvas de esta UI
        Canvas canvas = GetComponentInParent<Canvas>(true);
        if (canvas != null)
        {
            foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.name == nombreObjeto)
                {
                    return t.gameObject;
                }
            }
        }

        // 2. Si no está en el Canvas, buscar en toda la escena (incluyendo inactivos)
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.name == nombreObjeto)
                {
                    return t.gameObject;
                }
            }
        }

        return null;
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
}
