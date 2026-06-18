using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private bool isActivated = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos que sea el jugador y que el checkpoint no esté ya activado
        if (collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            
            // Si tienes un Animator (por ejemplo para subir la banderita), lanza el trigger
            if (anim != null)
            {
                anim.SetTrigger("Activate"); // Asegúrate de tener este trigger en tu Animator
            }

            Debug.Log("¡Checkpoint alcanzado!");

            // Guardamos la posición exacta de este checkpoint y en qué escena estamos
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            PlayerPlatformerController.lastCheckpointScene = currentSceneName;
            PlayerPlatformerController.lastCheckpointPos = transform.position;

            // Guardamos el estado de monedas y galletas recolectadas en este checkpoint
            PlayerPlatformerController.SaveCheckpointStats();

            // Auto-guardado en el disco si es el Nivel 1 del Mundo 2
            if (currentSceneName == "2_Level1")
            {
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveGame();
                }
            }
        }
    }
}
