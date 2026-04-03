using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    [Header("Configuración visual")]
    //[Tooltip("El aviso visual (ej: una tecla E flotando)")]
    //public GameObject visualCue;

    [Tooltip("Color de iluminación cuando el jugador está cerca")]
    [ColorUsage(true, true)] // Permite colores HDR para brillar más en URP
    public Color highlightColor = new Color(1.5f, 1.5f, 1.5f, 1f);

    [Header("¿Qué pasa al interactuar?")]
    public UnityEvent onInteract;

    private bool isPlayerClose = false;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;

    private void Start()
    {
        //if (visualCue != null) visualCue.SetActive(false);
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
            //if (visualCue != null) visualCue.SetActive(true);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            //if (visualCue != null) visualCue.SetActive(false);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }
        }
    }

    public void Interact()
    {
        if (isPlayerClose)
        {
            Debug.Log("¡Interactuando!");
            onInteract.Invoke();
        }
    }
}
