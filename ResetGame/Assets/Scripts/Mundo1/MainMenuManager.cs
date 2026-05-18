using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    public GameObject collectionsPanel;

    [Header("Botones Especiales")]
    public Button continueButton;

    private void Start()
    {
        ShowMainPanel();

        /*if (PlayerPrefs.HasKey("SavedLevel"))
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }*/
    }

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        collectionsPanel.SetActive(false);
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

    public void ShowCollections()
    {
        mainPanel.SetActive(false);
        collectionsPanel.SetActive(true);
    }


    public void NewGame()
    {
        //Borramos el progreso anterior para empezar de 0
        PlayerPrefs.DeleteAll();

        //Cargamos el Hub
        SceneManager.LoadScene("01_Hub");
    }

    public void ContinueGame()
    {
        int levelToLoad = PlayerPrefs.GetInt("SavedLevel");
        SceneManager.LoadScene("01_Hub");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

}