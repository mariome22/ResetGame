using UnityEngine;
using UnityEngine.Events;

public class NPCDialogue : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [SerializeField] private Dialogue dialogue;

    [Header("Eventos al Terminar (Opcional)")]
    [Tooltip("Acciones que ocurrirán en la escena cuando este diálogo finalice por completo")]
    [SerializeField] private UnityEvent alTerminarDialogo;

    /// <summary>
    /// Inicia el diálogo a través del DialogueManager.
    /// Conéctalo al evento OnInteract de tu script InteractableObject en el editor de Unity.
    /// </summary>
    public void IniciarDialogo()
    {
        if (DialogueManager.Instance != null)
        {
            // Pasamos un callback para disparar el UnityEvent local al finalizar las frases
            DialogueManager.Instance.StartDialogue(dialogue, () =>
            {
                alTerminarDialogo?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("No se encuentra la instancia de DialogueManager en la escena.");
        }
    }
}
