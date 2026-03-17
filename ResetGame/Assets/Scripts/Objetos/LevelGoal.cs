using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscenaHub = "01_Hub"; // El nombre de tu Hub

    // Esta función la llamaremos desde el evento OnInteract del Inspector
    public void CollectCore()
    {
        Debug.Log("¡Núcleo recogido!");

        // 1. Sumamos el núcleo a la memoria
        int currentCores = PlayerPrefs.GetInt("PlayerCores", 0);
        PlayerPrefs.SetInt("PlayerCores", currentCores + 1);
        PlayerPrefs.Save();

        // 2. (Opcional) Aquí puedes reproducir un sonido o partículas antes de salir

        // 3. Volvemos al Hub
        SceneManager.LoadScene(nombreEscenaHub);
    }
}