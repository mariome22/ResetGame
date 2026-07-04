using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Configuración de Transición")]
    [Tooltip("La escena que se cargará al completar el nivel.")]
    public string sceneToLoad = "01_Hub";

    [Tooltip("Si está activo, entrar en el trigger del colisionador completará el nivel.")]
    public bool triggerOnPlayerEnter = true;

    [Header("Configuración del Diálogo")]
    [Tooltip("Si está activo, se reproducirá un diálogo antes de la transición.")]
    public bool hasDialogue = false;
    
    [Tooltip("Datos del diálogo que se mostrará antes de transicionar.")]
    public Dialogue dialogue;

    [Header("Eventos")]
    [Tooltip("Eventos que ocurren al completar el nivel (ej: sonidos, guardar partida, desactivar controles del jugador).")]
    public UnityEvent onLevelCompleted;

    [Header("Pantalla de Victoria")]
    [Tooltip("El panel de victoria que se mostrará al completar el nivel (opcional).")]
    public GameObject panelVictoria;

    [Header("Sonido de Victoria")]
    [SerializeField] private AudioClip sonidoVictoria;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobar si es el jugador y si no estamos ya en proceso de transición
        if (triggerOnPlayerEnter && !isTransitioning && other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    public void CompleteLevel()
    {
        if (isTransitioning) return;

        Debug.Log("Completando nivel. Cargando escena: " + sceneToLoad);

        // Otorgar núcleo al jugador si es la primera vez que se completa este nivel
        string sceneName = SceneManager.GetActiveScene().name;
        string key = "LevelCompleted_" + sceneName;

        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            int currentCores = PlayerPrefs.GetInt("PlayerCores", 0);
            PlayerPrefs.SetInt("PlayerCores", currentCores + 1);
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"[LevelExit] Primer completado de {sceneName}. Otorgado 1 núcleo. Total núcleos: {currentCores + 1}");
        }
        else
        {
            Debug.Log($"[LevelExit] El nivel {sceneName} ya había sido completado anteriormente. No se otorgan más núcleos.");
        }

        onLevelCompleted.Invoke();

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            Time.timeScale = 0f;

            if (sonidoVictoria != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(sonidoVictoria);
            }
        }
        else
        {
            isTransitioning = true;
            IniciarTransicionCarga();
        }
    }

    public void ContinuarTrasVictoria()
    {
        Time.timeScale = 1f;
        if (isTransitioning) return;
        isTransitioning = true;

        // Mantenemos el panel de victoria activo durante la transición para que no se
        // vea al jugador moverse o al juego descongelado por debajo antes de que la pantalla se ponga negra.
        IniciarTransicionCarga();
    }

    private void IniciarTransicionCarga()
    {
        if (hasDialogue && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, () => LoadNextScene());
        }
        else
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        // Reanudamos la escala de tiempo por si el diálogo u otra acción la pausó
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
