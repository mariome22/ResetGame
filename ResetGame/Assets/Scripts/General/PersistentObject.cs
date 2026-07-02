using UnityEngine;
using UnityEngine.Events;

public class PersistentObject : MonoBehaviour
{
    [Header("Identificador de Persistencia")]
    [Tooltip("ID único para este objeto en la escena. Si se deja en blanco, se autogenerará basándose en su nombre y posición.")]
    public string uniqueId;

    [Header("Comportamiento al Cargar")]
    [Tooltip("Si está activo, el objeto se destruirá automáticamente al iniciar la escena si ya fue destruido/recogido. Si está inactivo, invocará el evento de abajo en lugar de destruirse.")]
    public bool destroyOnLoad = true;

    [Tooltip("Evento ejecutado si el objeto ya fue completado/activado anteriormente (solo si destroyOnLoad está inactivo).")]
    public UnityEvent onAlreadyTriggered;

    private void Awake()
    {
        InitializeId();
    }

    private void Start()
    {
        // Comprobamos si el objeto ya está marcado como destruido/completado en SaveManager
        if (SaveManager.Instance != null && SaveManager.Instance.IsObjectDestroyed(uniqueId))
        {
            if (destroyOnLoad)
            {
                Destroy(gameObject);
            }
            else
            {
                onAlreadyTriggered.Invoke();
            }
        }
    }

    private void InitializeId()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            // Auto-generamos un ID basado en el nombre de la escena, el nombre del objeto y su posición 2D
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            uniqueId = $"{sceneName}_{gameObject.name}_{transform.position.x:F2}_{transform.position.y:F2}";
        }
    }

    /// <summary>
    /// Registra la destrucción o activación de este objeto en el SaveManager para que persista.
    /// </summary>
    public void RegisterDestruction()
    {
        InitializeId();
        if (SaveManager.Instance != null && !string.IsNullOrEmpty(uniqueId))
        {
            SaveManager.Instance.RegisterDestroyedObject(uniqueId);
        }
    }
}
