using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Botones Especiales")]
    public Button continueButton;

    private void Start()
    {
        ShowMainPanel();

        if (continueButton != null)
        {
            // Por ahora, dejamos el botón siempre activo para que se pueda ir directamente al Hub
            continueButton.interactable = true;
        }
    }

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }


    public void NewGame()
    {
        // Borramos el progreso anterior para empezar de 0
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveGame();
            SaveManager.Instance.destroyedObjects.Clear();
        }
        else
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        // Cargamos el Hub
        SceneManager.LoadScene("01_Hub");
    }

    public void ContinueGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveGame())
        {
            SaveManager.Instance.LoadGame();
        }
        else
        {
            SceneManager.LoadScene("01_Hub");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

}