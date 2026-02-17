using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransfer : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] public string nombreEscenaDestino;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Viajando a " + nombreEscenaDestino + "!");
            SceneManager.LoadScene(nombreEscenaDestino);
        }
    }
}