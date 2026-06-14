using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class KeypadController : MonoBehaviour
{
    [Header("Configuración del Código")]
    [Tooltip("El código correcto que el jugador debe adivinar.")]
    public string correctCode = "1234";
    [Tooltip("El número máximo de dígitos permitidos.")]
    public int maxCodeLength = 4;
    [Tooltip("Escena opcional a la que cambiar cuando el código sea correcto.")]
    public string sceneToLoadOnCorrect = "";

    [Header("Referencias UI")]
    [Tooltip("El panel principal del teclado numérico (Canvas/Panel).")]
    public GameObject keypadPanel;
    [Tooltip("Texto donde se mostrará el código que se va introduciendo.")]
    public TextMeshProUGUI displayText;

    [Header("Eventos")]
    [Tooltip("Qué ocurre cuando el código es correcto (ej: Abrir puerta, cargar nivel, etc).")]
    public UnityEvent onCodeCorrect;
    [Tooltip("Qué ocurre cuando el código es incorrecto (opcional).")]
    public UnityEvent onCodeIncorrect;

    private string currentInput = "";
    private bool isProcessing = false;

    private void Start()
    {
        if (keypadPanel != null)
        {
            keypadPanel.SetActive(false);
        }
    }

    // Método para abrir el UI (Llamar desde el UnityEvent de un InteractableObject)
    public void OpenKeypad()
    {
        if (keypadPanel != null)
        {
            keypadPanel.SetActive(true);
            ClearInput();
            
            // Opcional: Pausar el juego para que los enemigos no ataquen mientras introduces el código
            Time.timeScale = 0f; 
        }
    }

    // Método para cerrar el UI (Llamar desde un botón "Salir")
    public void CloseKeypad()
    {
        if (keypadPanel != null)
        {
            keypadPanel.SetActive(false);
            
            // Si pausaste el juego al abrir, descomenta esto para reanudarlo
            Time.timeScale = 1f;
        }
    }

    // Método que llamarán los botones numéricos pasándole su número ("1", "2", etc.)
    public void AddNumber(string number)
    {
        if (isProcessing) return; // Si está procesando la comprobación, no admitir más números

        if (currentInput.Length < maxCodeLength)
        {
            currentInput += number;
            UpdateDisplay();
        }
    }

    // Método para un botón de borrar el último número (opcional)
    public void DeleteLastNumber()
    {
        if (isProcessing || currentInput.Length == 0) return;

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        UpdateDisplay();
    }

    // Método para un botón de borrar todo ("Clear")
    public void ClearInput()
    {
        if (isProcessing) return;

        currentInput = "";
        if (displayText != null)
        {
            displayText.text = currentInput;
            displayText.color = Color.white; // Restablecer color por si estaba en rojo/verde
        }
    }

    // Método para el botón de confirmar ("Enter")
    public void SubmitCode()
    {
        if (isProcessing) return;

        if (currentInput == correctCode)
        {
            StartCoroutine(HandleCorrectCode());
        }
        else
        {
            StartCoroutine(HandleIncorrectCode());
        }
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text = currentInput;
        }
    }

    private IEnumerator HandleCorrectCode()
    {
        isProcessing = true;
        
        if (displayText != null)
        {
            displayText.color = Color.green;
            displayText.text = "CORRECTO";
        }

        // Ejecutar los eventos configurados en el inspector
        onCodeCorrect.Invoke();

        // Esperar 1 segundo en tiempo real (incluso si Time.timeScale es 0)
        yield return new WaitForSecondsRealtime(1f);

        if (!string.IsNullOrEmpty(sceneToLoadOnCorrect))
        {
            Time.timeScale = 1f; // Asegurar que el juego se reanuda
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnCorrect);
        }
        else
        {
            CloseKeypad();
        }
        
        isProcessing = false;
    }

    private IEnumerator HandleIncorrectCode()
    {
        isProcessing = true;

        if (displayText != null)
        {
            displayText.color = Color.red;
            displayText.text = "ERROR";
        }

        // Ejecutar eventos de error si los hay (ej: reproducir sonido)
        onCodeIncorrect.Invoke();

        // Esperar 1 segundo en tiempo real
        yield return new WaitForSecondsRealtime(1f);

        ClearInput();
        isProcessing = false;
    }
}
