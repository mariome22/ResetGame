using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class LevelSelectorController : MonoBehaviour
{
    [Header("UI del Selector")]
    [Tooltip("El panel principal de este selector de niveles en el Canvas")]
    [SerializeField] private GameObject panelUI;

    [Header("Configuración de Niveles")]
    [Tooltip("Botones de los niveles en orden secuencial")]
    [SerializeField] private Button[] botonesNiveles;

    [Tooltip("Nombres exactos de las escenas que carga cada botón")]
    [SerializeField] private string[] escenasNiveles;

    [Tooltip("Cantidad de Cores (núcleos) requeridos para desbloquear cada nivel (coincidiendo en índice con los botones)")]
    [SerializeField] private int[] coresRequeridos;

    [Tooltip("Texto opcional en el Canvas para mostrar los cores actuales del jugador")]
    [SerializeField] private TMP_Text textoCoresActuales;

    [Header("Consejos de este Mundo")]
    [Tooltip("Consejos específicos de este mundo que se mostrarán en la pantalla de carga (opcional)")]
    [SerializeField] private List<string> consejosDeEsteMundo = new List<string>();

    [Header("Elementos de Escena Adicionales")]
    [Tooltip("Objetos adicionales en la escena (como fondos o luces) que se activarán al abrir el selector y se desactivarán al cerrarlo")]
    [SerializeField] private List<GameObject> elementosAdicionalesEscena = new List<GameObject>();

    [Header("Sonido (Opcional)")]
    [Tooltip("Música de fondo que sonará mientras este selector de nivel esté abierto")]
    [SerializeField] private AudioClip musicaSelector;

    // Estados públicos para que otros scripts (como la Pausa) los consulten
    public static bool IsSelectorOpen { get; private set; } = false;
    
    private static int lastClosedFrame = -1;
    public static bool CerradoEsteFrame
    {
        get { return lastClosedFrame == Time.frameCount; }
    }

    public static void ResetFlags()
    {
        IsSelectorOpen = false;
        lastClosedFrame = -1;
    }

    private void Awake()
    {
        ResetFlags();
    }

    private void OnDisable()
    {
        ResetFlags();
    }

    private void Start()
    {
        // El selector empieza cerrado a menos que se haya abierto justo al arrancar
        if (!IsSelectorOpen)
        {
            CerrarMenu();
        }
    }

    public void AbrirMenu()
    {
        if (panelUI != null)
        {
            panelUI.SetActive(true);
            
            // Pausar juego mientras se elige nivel
            Time.timeScale = 0f;
            IsSelectorOpen = true;
            lastClosedFrame = -1;

            if (musicaSelector != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(musicaSelector);
            }

            // Activar fondos y luces adicionales
            foreach (var elem in elementosAdicionalesEscena)
            {
                if (elem != null) elem.SetActive(true);
            }
            
            ActualizarBotones();
        }
    }

    public void CerrarMenu()
    {
        if (panelUI != null)
        {
            panelUI.SetActive(false);
            
            // Reanudar juego
            Time.timeScale = 1f;
            IsSelectorOpen = false;
            lastClosedFrame = Time.frameCount;

            // Restaurar la música de la escena
            PlaySceneMusic localMusic = FindFirstObjectByType<PlaySceneMusic>();
            if (localMusic != null)
            {
                localMusic.Play();
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
            }

            // Desactivar fondos y luces adicionales
            foreach (var elem in elementosAdicionalesEscena)
            {
                if (elem != null) elem.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Permitir cerrar el menú de niveles al presionar ESC si está abierto
        if (IsSelectorOpen && panelUI != null && panelUI.activeSelf)
        {
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            {
                CerrarMenu();
            }
        }
    }



    private void ActualizarBotones()
    {
        // Leer los cores directamente de PlayerPrefs
        int coresActuales = PlayerPrefs.GetInt("PlayerCores", 0);

        if (textoCoresActuales != null)
        {
            textoCoresActuales.text = "Cores: " + coresActuales;
        }

        for (int i = 0; i < botonesNiveles.Length; i++)
        {
            if (botonesNiveles[i] == null) continue;

            int req = 0;
            if (coresRequeridos != null && i < coresRequeridos.Length)
            {
                req = coresRequeridos[i];
            }

            bool desbloqueado = coresActuales >= req;
            botonesNiveles[i].interactable = desbloqueado;

            // Opcional: Cambiar la opacidad del texto del botón para indicar bloqueo
            TMP_Text btnText = botonesNiveles[i].GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                if (desbloqueado)
                {
                    btnText.color = Color.white;
                }
                else
                {
                    btnText.color = new Color(1f, 1f, 1f, 0.3f); // Atenuado si está bloqueado
                }
            }
        }
    }

    // Este método se asocia a los OnClick de cada botón de la UI en el Inspector
    public void CargarNivel(string nombreEscena)
    {
        // Reanudar tiempo antes de cargar para que el juego continúe con normalidad
        Time.timeScale = 1f;
        IsSelectorOpen = false; // Resetear la bandera estática para evitar bloqueos en la nueva escena

        if (SceneTransitionManager.Instance != null)
        {
            // Cargar usando el fundido a negro simple (original)
            // No llamamos a CerrarMenu() aquí para que el menú de niveles y el fondo permanezcan visibles mientras la pantalla se funde a negro.
            SceneTransitionManager.Instance.LoadSceneWithFade(nombreEscena);
        }
        else
        {
            CerrarMenu();
            UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
        }
    }
}
