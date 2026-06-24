using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Paneles Nuevos de Partida")]
    public GameObject playSelectPanel;
    public GameObject newGameConfirmPanel;

    [Header("Botones Especiales")]
    public Button continueButton;

    private void Start()
    {
        if (SaveManager.Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Global_Managers");
            if (prefab != null)
            {
                Instantiate(prefab);
            }
        }

        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (playSelectPanel != null) playSelectPanel.SetActive(false);
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        mainPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void ShowPlaySelectPanel()
    {
        mainPanel.SetActive(false);
        if (playSelectPanel != null) playSelectPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (continueButton != null)
        {
            continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveGame();
        }
    }

    public void NewGame()
    {
        // Si hay una partida guardada, mostrar panel de confirmación
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveGame())
        {
            if (newGameConfirmPanel != null)
            {
                newGameConfirmPanel.SetActive(true);
            }
            else
            {
                ConfirmNewGame();
            }
        }
        else
        {
            ConfirmNewGame();
        }
    }

    public void ConfirmNewGame()
    {
        // Borramos el progreso anterior para empezar de 0
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveGame();
            SaveManager.Instance.destroyedObjects.Clear();
        }
        else
        {
            PlayerPrefs.DeleteKey("SavedLevel");
            PlayerPrefs.DeleteKey("PlayerCores");
            PlayerPrefs.Save();
        }

        // Cargamos el Hub
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade("01_Hub");
        }
        else
        {
            SceneManager.LoadScene("01_Hub");
        }
    }

    public void CancelNewGame()
    {
        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(false);
        }
    }

    public void ContinueGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveGame())
        {
            SaveManager.Instance.LoadGame();
        }
        else
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade("01_Hub");
            }
            else
            {
                SceneManager.LoadScene("01_Hub");
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}