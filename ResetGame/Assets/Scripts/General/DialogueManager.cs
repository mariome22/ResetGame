using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class Dialogue
{
    public string npcName;
    [TextArea(3, 10)]
    public string[] sentences;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Componentes")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Configuración")]
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<string> sentencesQueue;
    private bool isTyping = false;
    private string currentSentence = "";
    private Coroutine typingCoroutine;

    public bool IsDialogueActive => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sentencesQueue = new Queue<string>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Si el diálogo está activo y el jugador presiona E (o clickea), avanza
        if (IsDialogueActive && Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialoguePanel == null || nameText == null || dialogueText == null)
        {
            Debug.LogWarning("Faltan componentes en el DialogueManager UI.");
            return;
        }

        // Pausar el movimiento del jugador si es necesario (se puede reanudar al final)
        Time.timeScale = 0f; // Pausar juego durante diálogo para evitar ataques de enemigos

        dialoguePanel.SetActive(true);
        nameText.text = dialogue.npcName;

        sentencesQueue.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentencesQueue.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            // Si el jugador pulsa interactuar mientras escribe, muestra la frase completa inmediatamente
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentSentence;
            isTyping = false;
            return;
        }

        if (sentencesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentencesQueue.Dequeue();
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        // Escribe letra a letra (usando tiempo real ya que Time.timeScale es 0)
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        Time.timeScale = 1f; // Reanudar tiempo
        Debug.Log("Fin del diálogo.");
    }
}
