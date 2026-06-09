using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [SerializeField] private Dialogue dialogue;

    /// <summary>
    /// Inicia el diálogo a través del DialogueManager.
    /// Conéctalo al evento OnInteract de tu script InteractableObject en el editor de Unity.
    /// </summary>
    public void IniciarDialogo()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("No se encuentra la instancia de DialogueManager en la escena.");
        }
    }
}
