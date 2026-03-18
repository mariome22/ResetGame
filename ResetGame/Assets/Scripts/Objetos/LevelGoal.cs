using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscenaHub = "01_Hub";

    public void CollectCore()
    {
        Debug.Log("¡Núcleo recogido!");

        int currentCores = PlayerPrefs.GetInt("PlayerCores", 0);
        PlayerPrefs.SetInt("PlayerCores", currentCores + 1);
        PlayerPrefs.Save();


        SceneManager.LoadScene(nombreEscenaHub);
    }
}