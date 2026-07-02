using UnityEngine;
using TMPro;

public class RegisterDialogueUI : MonoBehaviour
{
    [Header("Referencias del Panel de Diálogo Local")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private void Awake()
    {
        // Registrar este panel local en el DialogueManager persistente
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.RegisterUI(gameObject, nameText, dialogueText);
            Debug.Log("[RegisterDialogueUI] Panel de diálogo local registrado correctamente.");
        }
        else
        {
            Debug.LogWarning("[RegisterDialogueUI] No se encontró la instancia de DialogueManager.");
        }
    }
}
