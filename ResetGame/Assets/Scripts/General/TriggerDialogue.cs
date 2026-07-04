using UnityEngine;

public class TriggerDialogue : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [SerializeField] private Dialogue dialogue;

    [Header("Persistencia (Opcional)")]
    [Tooltip("ID único para este diálogo. Si se asigna, solo se reproducirá una vez en toda la partida y se guardará en la persistencia del juego.")]
    [SerializeField] private string dialogoID;

    [Header("Condición de Progreso (Opcional)")]
    [Tooltip("El número de núcleos (cores) que debe tener el jugador para activar este diálogo. Si es -1, se ignorará esta condición.")]
    [SerializeField] private int coresRequeridos = -1;

    [Header("Pantalla Continuará (Opcional)")]
    [Tooltip("El panel que dice 'CONTINUARÁ' que se mostrará al finalizar este diálogo.")]
    [SerializeField] private GameObject panelContinuara;
    [Tooltip("Nombre de la escena a cargar al aceptar el panel 'CONTINUARÁ' (por ejemplo, 'MainMenu').")]
    [SerializeField] private string escenaCargarAlAceptar = "MainMenu";

    private bool activado = false;
    private bool isTransitioningOut = false;

    private void Start()
    {
        // Si tiene ID de persistencia, comprobar al iniciar si ya se reprodujo
        if (!string.IsNullOrEmpty(dialogoID) && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.dialogosReproducidos.Contains(dialogoID))
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado) return;

        if (other.CompareTag("Player"))
        {
            // Comprobar condición de progreso de núcleos
            if (coresRequeridos != -1)
            {
                int coresActuales = PlayerPrefs.GetInt("PlayerCores", 0);
                if (coresActuales != coresRequeridos)
                {
                    // No cumple el número exacto de núcleos requeridos, ignoramos el trigger
                    return;
                }
            }

            // Si tiene ID de persistencia, comprobar si ya se reprodujo
            if (!string.IsNullOrEmpty(dialogoID) && SaveManager.Instance != null)
            {
                if (SaveManager.Instance.dialogosReproducidos.Contains(dialogoID))
                {
                    // Ya se ha reproducido en una partida guardada anterior, destruir el trigger directamente
                    Destroy(gameObject);
                    return;
                }
            }

            activado = true;
            IniciarDialogo();
        }
    }

    private void IniciarDialogo()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, () =>
            {
                FinalizarOMostrarContinuara();
            });
        }
        else
        {
            FinalizarOMostrarContinuara();
        }
    }

    private void FinalizarOMostrarContinuara()
    {
        // Al terminar el diálogo, si tiene ID, lo guardamos en la persistencia en memoria
        if (!string.IsNullOrEmpty(dialogoID) && SaveManager.Instance != null)
        {
            if (!SaveManager.Instance.dialogosReproducidos.Contains(dialogoID))
            {
                SaveManager.Instance.dialogosReproducidos.Add(dialogoID);
            }
        }

        if (panelContinuara != null)
        {
            // Desactivar el colisionador para evitar cualquier colisión extra
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            if (SceneTransitionManager.Instance != null)
            {
                // Fundido a negro antes de mostrar el panel
                SceneTransitionManager.Instance.FadeOut(0.5f, () =>
                {
                    panelContinuara.SetActive(true);
                    Time.timeScale = 0f; // Pausar juego para que lean el panel

                    // Revelar el panel mediante un fundido de negro a transparente
                    SceneTransitionManager.Instance.FadeIn(0.5f, null);
                });
            }
            else
            {
                panelContinuara.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AceptarContinuara()
    {
        if (isTransitioningOut) return;
        isTransitioningOut = true;

        Time.timeScale = 1f;

        if (SceneTransitionManager.Instance != null)
        {
            // Cargamos la escena con fundido. No apagamos panelContinuara aquí,
            // de modo que el fundido a negro lo cubra de forma natural y evite el parpadeo del Hub.
            SceneTransitionManager.Instance.LoadSceneWithFade(escenaCargarAlAceptar);
        }
        else
        {
            if (panelContinuara != null)
            {
                panelContinuara.SetActive(false);
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(escenaCargarAlAceptar);
        }
    }

    private void Update()
    {
        // Si el panel de Continuará está activo, permitir avanzar pulsando cualquier tecla del teclado
        if (panelContinuara != null && panelContinuara.activeSelf && !isTransitioningOut)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame)
            {
                AceptarContinuara();
            }
        }
    }
}
