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

        int currentCores = PlayerPrefs.GetInt("PlayerCores", 0);
        PlayerPrefs.SetInt("PlayerCores", currentCores + 1);
        PlayerPrefs.Save();

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
