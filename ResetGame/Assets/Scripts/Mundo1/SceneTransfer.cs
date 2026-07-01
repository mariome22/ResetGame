using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransfer : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena a la que teletransportará el portal.")]
    public string sceneName;
    [Tooltip("Cantidad de núcleos necesarios para activar/desbloquear el portal.")]
    public int requiredCores = 0;

    [Header("Visuales del Portal")]
    [Tooltip("El SpriteRenderer del portal. Si no se asigna, se buscará en este GameObject.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("El Sprite que se mostrará cuando el portal esté bloqueado (menos núcleos de los requeridos).")]
    [SerializeField] private Sprite lockedSprite;
    [Tooltip("El Sprite que se mostrará cuando el portal esté desbloqueado.")]
    [SerializeField] private Sprite unlockedSprite;

    [Header("Efectos")]
    [Tooltip("El GameObject del efecto de partículas que se activará cuando el portal esté desbloqueado.")]
    [SerializeField] private GameObject particlesEffect;

    [Header("Selección de Niveles (Opcional)")]
    [Tooltip("Si se asigna, abrirá este menú de selección de niveles en lugar de cargar directamente la escena.")]
    [SerializeField] private LevelSelectorController selectorDeNivel;

    private CoreManager coreManager;
    private int lastCheckedCores = -1;

    private void Start()
    {
        // Si no se ha asignado manualmente, intentar obtener el SpriteRenderer del mismo GameObject o de sus hijos
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        // Buscar el CoreManager en la escena
        coreManager = Object.FindFirstObjectByType<CoreManager>();

        ActualizarVisualesPortal();
    }

    private void OnEnable()
    {
        // Forzar actualización al activarse
        lastCheckedCores = -1; 
        ActualizarVisualesPortal();
    }

    private void Update()
    {
        int currentCores = GetCores();
        if (currentCores != lastCheckedCores)
        {
            lastCheckedCores = currentCores;
            ActualizarVisualesPortal();
        }
    }

    private int GetCores()
    {
        if (coreManager != null)
        {
            return coreManager.totalCores;
        }
        return PlayerPrefs.GetInt("PlayerCores", 0);
    }

    /// <summary>
    /// Comprueba el progreso de núcleos y actualiza el sprite y el efecto de partículas del portal.
    /// </summary>
    public void ActualizarVisualesPortal()
    {
        int currentCores = GetCores();
        bool isUnlocked = currentCores >= requiredCores;

        // Cambiar el sprite según el estado de desbloqueo
        if (spriteRenderer != null)
        {
            if (isUnlocked && unlockedSprite != null)
            {
                spriteRenderer.sprite = unlockedSprite;
            }
            else if (!isUnlocked && lockedSprite != null)
            {
                spriteRenderer.sprite = lockedSprite;
            }
        }

        // Activar o desactivar las partículas/efectos del portal
        if (particlesEffect != null)
        {
            particlesEffect.SetActive(isUnlocked);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int currentCores = GetCores();

            if (currentCores >= requiredCores)
            {
                if (selectorDeNivel != null)
                {
                    // Detener al jugador
                    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = Vector2.zero;

                    // Desactivar temporalmente el script de movimiento para que no se mueva durante el fundido
                    var playerController = other.GetComponent<PlayerController>();
                    if (playerController != null) playerController.enabled = false;

                    if (SceneTransitionManager.Instance != null)
                    {
                        // Fundido a negro -> Activar menú -> Fundido a transparente
                        SceneTransitionManager.Instance.FadeOut(0.3f, () => {
                            selectorDeNivel.AbrirMenu();
                            if (playerController != null) playerController.enabled = true;
                            SceneTransitionManager.Instance.FadeIn(0.3f, null);
                        });
                    }
                    else
                    {
                        selectorDeNivel.AbrirMenu();
                        if (playerController != null) playerController.enabled = true;
                    }
                }
                else
                {
                    // Carga directa clásica si no hay selector asignado
                    if (SceneTransitionManager.Instance != null)
                    {
                        SceneTransitionManager.Instance.LoadSceneWithFade(sceneName);
                    }
                    else
                    {
                        SceneManager.LoadScene(sceneName);
                    }
                }
            }
            else
            {
                Debug.Log("¡Necesitas " + requiredCores + " núcleos para entrar aquí!");
            }
        }
    }
}
