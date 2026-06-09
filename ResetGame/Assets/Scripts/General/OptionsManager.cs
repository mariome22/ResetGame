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

        if (botonSalirAlHub != null)
        {
            if (panelConfirmacionSalir != null)
            {
                botonSalirAlHub.onClick.RemoveAllListeners();
                botonSalirAlHub.onClick.AddListener(ExitToHub);
            }
        }

        if (botonSalirAlMenu != null)
        {
            if (panelConfirmacionSalir != null)
            {
                botonSalirAlMenu.onClick.RemoveAllListeners();
                botonSalirAlMenu.onClick.AddListener(ExitToMainMenu);
            }
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
        if (panelConfirmacionSalir != null)
        {
            escenaDestinoPending = escenaDestino;
            panelConfirmacionSalir.SetActive(true);
        }
        else
        {
            // Salida directa si no se configuró el panel en el inspector
            Time.timeScale = 1f;
            SceneManager.LoadScene(escenaDestino);
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
        SceneManager.LoadScene(escenaDestinoPending);
    }

    private void ConfirmarSalirSinGuardar()
    {
        Time.timeScale = 1f; // Reanudar tiempo
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
