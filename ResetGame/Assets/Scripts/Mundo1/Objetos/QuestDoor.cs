using UnityEngine;

public class QuestDoor : MonoBehaviour
{
    [Header("Configuración de la Misión")]
    [Tooltip("Los objetos que el jugador debe recoger para abrir la puerta")]
    public GameObject[] questItems;

    [Tooltip("Sprite que se mostrará cuando la puerta esté abierta")]
    public Sprite openSprite;
    public GameObject bloqueo;

    private SpriteRenderer spriteRenderer;
    private bool questActive = false;
    private bool isOpen = false;
    private int itemsCollected = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Nos aseguramos de que los objetos estén desactivados al inicio
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

        // AQUÍ IRÁ EL CÓDIGO DEL DIÁLOGO EN EL FUTURO
        Debug.Log("NPC (Detrás de la puerta): '¡Hola! Para abrir esta puerta necesito que encuentres " + questItems.Length + " objetos que he perdido por este escenario.'");

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
            int restantes = questItems.Length - itemsCollected;
            // AQUÍ IRÁ EL CÓDIGO DEL DIÁLOGO EN EL FUTURO
            Debug.Log("NPC: 'Todavía te faltan " + restantes + " objetos para que te abra la puerta.'");
        }
    }

    private void AbrirPuerta()
    {
        PersistentObject po = GetComponent<PersistentObject>();
        if (po != null) po.RegisterDestruction();

        AbrirPuertaInstant();

        // NPC dialogue logging
        Debug.Log("NPC: '¡Gracias! Te abro las puertas.'");
    }

    private void AbrirPuertaInstant()
    {
        isOpen = true;
        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
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
