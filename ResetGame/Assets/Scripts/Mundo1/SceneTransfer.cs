using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransfer : MonoBehaviour
{
    public string sceneName;
    public int requiredCores = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int currentCores = PlayerPrefs.GetInt("PlayerCores", 0);

            if (currentCores >= requiredCores)
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.Log("¡Necesitas " + requiredCores + " núcleos para entrar aquí!");
            }
        }
    }
}