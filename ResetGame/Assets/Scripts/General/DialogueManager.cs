using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [Tooltip("Nombre del personaje que habla en esta frase")]
    public string npcName;
    
    [TextArea(3, 10)]
    [Tooltip("Texto de la frase")]
    public string sentence;
}

[System.Serializable]
public class Dialogue
{
    [Tooltip("Lista de frases de la conversación en orden secuencial")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Global_Managers");
                    if (prefab != null)
                    {
                        GameObject instantiated = Instantiate(prefab);
                        _instance = instantiated.GetComponentInChildren<DialogueManager>(true);
                        if (!instantiated.activeSelf) instantiated.SetActive(true);
                        DontDestroyOnLoad(instantiated);
                    }
                    else
                    {
                        GameObject obj = new GameObject("DialogueManager");
                        _instance = obj.AddComponent<DialogueManager>();
                        DontDestroyOnLoad(obj);
                    }
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("UI Componentes")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Configuración")]
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<DialogueLine> sentencesQueue;
    private bool isTyping = false;
    private string currentSentence = "";
    private Coroutine typingCoroutine;
    private System.Action onDialogueEndCallback;

    public bool IsDialogueActive => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            sentencesQueue = new Queue<DialogueLine>();
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void RegisterUI(GameObject panel, TextMeshProUGUI nameTxt, TextMeshProUGUI dialogueTxt)
    {
        dialoguePanel = panel;
        nameText = nameTxt;
        dialogueText = dialogueTxt;

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

    public void StartDialogue(Dialogue dialogue, System.Action onEnd = null)
    {
        if (dialoguePanel == null || nameText == null || dialogueText == null)
        {
            Debug.LogWarning("Faltan componentes en el DialogueManager UI.");
            return;
        }

        // Pausar el juego durante el diálogo para evitar ataques de enemigos
        Time.timeScale = 0f;

        dialoguePanel.SetActive(true);
        onDialogueEndCallback = onEnd;

        sentencesQueue.Clear();
        foreach (DialogueLine line in dialogue.lines)
        {
            sentencesQueue.Enqueue(line);
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

        DialogueLine nextLine = sentencesQueue.Dequeue();
        
        // Asignamos el nombre del personaje que habla en esta línea específica
        nameText.text = nextLine.npcName;
        currentSentence = nextLine.sentence;
        
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

        if (onDialogueEndCallback != null)
        {
            System.Action callback = onDialogueEndCallback;
            onDialogueEndCallback = null;
            callback.Invoke();
        }
    }
}
