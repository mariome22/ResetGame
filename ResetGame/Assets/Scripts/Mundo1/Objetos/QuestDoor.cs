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
        isOpen = true;
        // AQUÍ IRÁ EL CÓDIGO DEL DIÁLOGO EN EL FUTURO
        Debug.Log("NPC: '¡Gracias! Te abro las puertas.'");

        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        // Si la puerta tiene un collider (para bloquear el paso), lo desactivamos
        Collider2D doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // NO re-delcaramos bloqueo para usar el público

        if (bloqueo != null)
        {
            bloqueo.SetActive(false);
        }

        // También desactivamos el InteractableObject para que no pueda volver a interactuar si se quiere
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
