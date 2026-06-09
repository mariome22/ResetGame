using System.Collections;
using UnityEngine;

public class SaveStation : MonoBehaviour
{
    [Header("UI Feedback")]
    [Tooltip("Panel o texto visual que dice 'Partida Guardada' (se activará temporalmente)")]
    [SerializeField] private GameObject textoPartidaGuardada;
    [SerializeField] private float tiempoDeMuestra = 2f;

    private Coroutine feedbackCoroutine;

    private void Start()
    {
        if (textoPartidaGuardada != null)
        {
            textoPartidaGuardada.SetActive(false);
        }
    }

    /// <summary>
    /// Guarda el progreso actual de la partida.
    /// Conéctalo al evento OnInteract de tu script InteractableObject en el editor de Unity.
    /// </summary>
    public void GuardarPartida()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            
            if (textoPartidaGuardada != null)
            {
                if (feedbackCoroutine != null)
                {
                    StopCoroutine(feedbackCoroutine);
                }
                feedbackCoroutine = StartCoroutine(MostrarFeedback());
            }
        }
        else
        {
            Debug.LogWarning("No se encuentra SaveManager en la escena para guardar la partida.");
        }
    }

    private IEnumerator MostrarFeedback()
    {
        // Usar Realtime porque podría llamarse estando el juego en pausa
        textoPartidaGuardada.SetActive(true);
        yield return new WaitForSecondsRealtime(tiempoDeMuestra);
        textoPartidaGuardada.SetActive(false);
    }
}
