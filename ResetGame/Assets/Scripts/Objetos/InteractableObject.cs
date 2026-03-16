using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El aviso visual (ej: una tecla E flotando)")]
    public GameObject visualCue;

    [Header("¿Qué pasa al interactuar?")]
    public UnityEvent onInteract;

    private bool isPlayerClose = false;

    private void Start()
    {
        if (visualCue != null) visualCue.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
            if (visualCue != null) visualCue.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            if (visualCue != null) visualCue.SetActive(false);
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