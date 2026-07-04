using UnityEngine;

public class QuestDoor : MonoBehaviour
{
    [Header("Configuración de la Misión")]
    [Tooltip("Los objetos que el jugador debe recoger para abrir la puerta")]
    public GameObject[] questItems;

    [Tooltip("Sprite que se mostrará cuando la puerta esté abierta")]
    public Sprite openSprite;
    [Tooltip("Sprite que se mostrará cuando la puerta esté desbloqueada pero todavía cerrada")]
    public Sprite unlockedSprite;
    public GameObject bloqueo;

    [Header("Diálogos de la Misión")]
    [Tooltip("Diálogo al iniciar la misión por primera vez")]
    [SerializeField] private Dialogue dialogoInicio;
    [Tooltip("Diálogo cuando aún faltan objetos por recoger")]
    [SerializeField] private Dialogue dialogoProgreso;
    [Tooltip("Diálogo al completar la misión y abrir la puerta")]
    [SerializeField] private Dialogue dialogoCompletado;

    private SpriteRenderer spriteRenderer;
    private bool questActive = false;
    private bool isOpen = false;
    private int itemsCollected = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Dejamos que los objetos de misión estén activos por defecto desde el inicio
        // para que no se pierdan al cargar/guardar la partida y evitar reajustes de guardado.
        /*
        if (questItems != null)
        {
            foreach (GameObject item in questItems)
            {
                if (item != null)
                {
                    item.SetActive(false);
                }
            }
        }
        */
    }

    private void Start()
    {
        // Re-contamos cuántos objetos ya han sido recogidos
        itemsCollected = 0;
        if (questItems != null)
        {
            foreach (GameObject item in questItems)
            {
                if (item != null)
                {
                    PersistentObject po = item.GetComponent<PersistentObject>();
                    if (po != null)
                    {
                        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                        string uniqueId = string.IsNullOrEmpty(po.uniqueId) ? 
                            $"{sceneName}_{item.name}_{item.transform.position.x:F2}_{item.transform.position.y:F2}" : 
                            po.uniqueId;
                        
                        if (SaveManager.Instance != null && SaveManager.Instance.IsObjectDestroyed(uniqueId))
                        {
                            itemsCollected++;
                        }
                    }
                }
            }
        }

        // Comprobamos si la propia puerta ya fue abierta
        PersistentObject doorPo = GetComponent<PersistentObject>();
        if (doorPo != null)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string uniqueId = string.IsNullOrEmpty(doorPo.uniqueId) ? 
                $"{sceneName}_{gameObject.name}_{transform.position.x:F2}_{transform.position.y:F2}" : 
                doorPo.uniqueId;

            if (SaveManager.Instance != null && SaveManager.Instance.IsObjectDestroyed(uniqueId))
            {
                AbrirPuertaInstant();
            }
        }
    }

    // Este método se llamará desde el OnInteract del InteractableObject
    public void OnDoorInteract()
    {
        if (isOpen)
        {
            Debug.Log("La puerta ya está abierta.");
            return;
        }

        if (!questActive)
        {
            // Primera interacción: Iniciar la misión
            IniciarMision();
        }
        else
        {
            // Interacciones posteriores: Comprobar progreso
            ComprobarMision();
        }
    }

    private void IniciarMision()
    {
        questActive = true;

        if (DialogueManager.Instance != null && dialogoInicio != null && dialogoInicio.lines.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(dialogoInicio, () =>
            {
                ActivarObjetosMision();
            });
        }
        else
        {
            Debug.Log("NPC (Detrás de la puerta): '¡Hola! Para abrir esta puerta necesito que encuentres " + questItems.Length + " objetos.'");
            ActivarObjetosMision();
        }
    }

    private void ActivarObjetosMision()
    {
        // Activamos los objetos escondidos
        if (questItems != null)
        {
            foreach (GameObject item in questItems)
            {
                if (item != null)
                {
                    item.SetActive(true);
                }
            }
        }
    }

    private void ComprobarMision()
    {
        if (itemsCollected >= questItems.Length)
        {
            // Completada
            AbrirPuerta();
        }
        else
        {
            // No completada
            if (DialogueManager.Instance != null && dialogoProgreso != null && dialogoProgreso.lines.Count > 0)
            {
                DialogueManager.Instance.StartDialogue(dialogoProgreso);
            }
            else
            {
                int restantes = questItems.Length - itemsCollected;
                Debug.Log("NPC: 'Todavía te faltan " + restantes + " objetos para que te abra la puerta.'");
            }
        }
    }

    private void AbrirPuerta()
    {
        PersistentObject po = GetComponent<PersistentObject>();
        if (po != null) po.RegisterDestruction();

        if (DialogueManager.Instance != null && dialogoCompletado != null && dialogoCompletado.lines.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(dialogoCompletado, () =>
            {
                AbrirPuertaInstant();
            });
        }
        else
        {
            AbrirPuertaInstant();
            Debug.Log("NPC: '¡Gracias! Te abro las puertas.'");
        }
    }

    private void AbrirPuertaInstant()
    {
        isOpen = true;
        if (spriteRenderer != null)
        {
            if (unlockedSprite != null)
            {
                spriteRenderer.sprite = unlockedSprite;
            }
            else if (openSprite != null)
            {
                spriteRenderer.sprite = openSprite;
            }
        }

        Collider2D doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        if (bloqueo != null)
        {
            bloqueo.SetActive(false);
        }

        InteractableObject interactable = GetComponent<InteractableObject>();
        if (interactable != null)
        {
            interactable.enabled = false;
        }
    }

    public void RegistarObjetoRecogido()
    {
        itemsCollected++;
        Debug.Log("Objeto de misión recogido. Llevas " + itemsCollected + "/" + questItems.Length);
    }
}
