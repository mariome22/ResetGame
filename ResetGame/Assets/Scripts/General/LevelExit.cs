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
        isTransitioning = true;

        Debug.Log("Completando nivel. Cargando escena: " + sceneToLoad);
        onLevelCompleted.Invoke();

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
        SceneManager.LoadScene(sceneToLoad);
    }
}
