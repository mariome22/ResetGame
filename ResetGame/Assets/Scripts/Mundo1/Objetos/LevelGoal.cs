using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [Header("Configuracion")]
    public string nombreEscenaHub = "01_Hub";
    public GameObject panelVictoria;

    [Header("Sonido (Opcional)")]
    [SerializeField] private AudioClip sonidoVictoria;

    private bool isTransitioning = false;

    public void CollectCore()
    {
        if (isTransitioning) return;

        Debug.Log("Core collected!");

        string sceneName = SceneManager.GetActiveScene().name;
        string key = "LevelCompleted_" + sceneName;

        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            int currentCores = PlayerPrefs.GetInt("PlayerCores", 0);
            PlayerPrefs.SetInt("PlayerCores", currentCores + 1);
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"[LevelGoal] Primer completado de {sceneName}. Otorgado 1 núcleo. Total núcleos: {currentCores + 1}");
        }
        else
        {
            Debug.Log($"[LevelGoal] El nivel {sceneName} ya había sido completado anteriormente. No se otorgan más núcleos.");
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            Time.timeScale = 0f;

            if (sonidoVictoria != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(sonidoVictoria);
            }
        }
        else
        {
            isTransitioning = true;
            CargarHub();
        }
    }

    public void ConfirmarCargaHub()
    {
        Time.timeScale = 1f;
        if (isTransitioning) return;
        isTransitioning = true;
        CargarHub();
    }

    private void CargarHub()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(nombreEscenaHub);
        }
        else
        {
            SceneManager.LoadScene(nombreEscenaHub);
        }
    }
}
