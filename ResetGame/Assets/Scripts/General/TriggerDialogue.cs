using UnityEngine;

public class TriggerDialogue : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [SerializeField] private Dialogue dialogue;

    [Header("Persistencia (Opcional)")]
    [Tooltip("ID único para este diálogo. Si se asigna, solo se reproducirá una vez en toda la partida y se guardará en la persistencia del juego.")]
    [SerializeField] private string dialogoID;

    [Header("Condición de Progreso (Opcional)")]
    [Tooltip("El número de núcleos (cores) que debe tener el jugador para activar este diálogo. Si es -1, se ignorará esta condición.")]
    [SerializeField] private int coresRequeridos = -1;

    private bool activado = false;

    private void Start()
    {
        // Si tiene ID de persistencia, comprobar al iniciar si ya se reprodujo
        if (!string.IsNullOrEmpty(dialogoID) && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.dialogosReproducidos.Contains(dialogoID))
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado) return;

        if (other.CompareTag("Player"))
        {
            // Comprobar condición de progreso de núcleos
            if (coresRequeridos != -1)
            {
                int coresActuales = PlayerPrefs.GetInt("PlayerCores", 0);
                if (coresActuales != coresRequeridos)
                {
                    // No cumple el número exacto de núcleos requeridos, ignoramos el trigger
                    return;
                }
            }

            // Si tiene ID de persistencia, comprobar si ya se reprodujo
            if (!string.IsNullOrEmpty(dialogoID) && SaveManager.Instance != null)
            {
                if (SaveManager.Instance.dialogosReproducidos.Contains(dialogoID))
                {
                    // Ya se ha reproducido en una partida guardada anterior, destruir el trigger directamente
                    Destroy(gameObject);
                    return;
                }
            }

            activado = true;
            IniciarDialogo();
        }
    }

    private void IniciarDialogo()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, () =>
            {
                // Al terminar el diálogo, si tiene ID, lo guardamos en la persistencia en memoria
                if (!string.IsNullOrEmpty(dialogoID) && SaveManager.Instance != null)
                {
                    if (!SaveManager.Instance.dialogosReproducidos.Contains(dialogoID))
                    {
                        SaveManager.Instance.dialogosReproducidos.Add(dialogoID);
                    }
                }
                // Destruir el trigger de la escena para que no vuelva a saltar en esta sesión
                Destroy(gameObject);
            });
        }
    }
}
