using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Para poder acceder a los botones

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    public GameObject collectionsPanel;

    [Header("Botones Especiales")]
    public Button continueButton; // Referencia al botón para desactivarlo si no hay partida

    private void Start()
    {
        // Al arrancar, volvemos al panel principal por si acaso
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

    // --- FUNCIONES DE NAVEGACIÓN (Para los botones) ---
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
        // Borramos el progreso anterior para empezar de 0 (Opcional)
        PlayerPrefs.DeleteAll();

        // Cargamos el Nivel 1 (Asegúrate de que está en Build Settings)
        SceneManager.LoadScene("Level1_Dungeon"); // O el nombre de tu primera escena real
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