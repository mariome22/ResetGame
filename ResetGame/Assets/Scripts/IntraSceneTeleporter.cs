using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntraSceneTeleporter : MonoBehaviour
{
    [Header("Ajustes de Teletransporte")]
    [Tooltip("El punto (Transform) al que se teletransportará el jugador.")]
    public Transform teleportDestination;
    [Tooltip("Duración del fundido a negro (en segundos).")]
    public float fadeDuration = 0.5f;
    [Tooltip("Tiempo que la pantalla se queda totalmente en negro.")]
    public float blackScreenDuration = 1f;

    [Header("Ajustes de Luz Global (Opcional)")]
    [Tooltip("Arrastra aquí la Light2D que hará de luz global a modificar.")]
    public UnityEngine.Rendering.Universal.Light2D luzGlobal;
    [Tooltip("La intensidad a la que se pondrá la luz al acabar el pequeño tiempo en negro.")]
    public float intensidadDestino = 1f;

    private static Canvas fadeCanvas;
    private static Image fadeImage;
    private bool isTeleporting = false;

    private void Awake()
    {
        // Creamos el Canvas y la Imagen para el fundido a negro de forma automática si no existen
        if (fadeCanvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 999; 
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = new Color(0, 0, 0, 0); // Empieza transparente

            RectTransform rect = fadeImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            // Para que este canvas no se destruya al cargar otras escenas, por si acaso
            DontDestroyOnLoad(canvasObj);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportRoutine(other.gameObject));
        }
    }

    private IEnumerator TeleportRoutine(GameObject playerObj)
    {
        isTeleporting = true;

        // Desactivar el PlayerController para evitar que se mueva mientras se teletransporta
        MonoBehaviour[] scripts = playerObj.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script.GetType().Name == "PlayerController")
            {
                script.enabled = false;
            }
        }

        // Frenar su inercia
        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Fundido a negro (Fade In)
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(elapsedTime / fadeDuration));
            yield return null;
        }
        fadeImage.color = Color.black;

        // Pequeña pausa antes de mover para que la pantalla esté totalmente negra
        yield return new WaitForSeconds(blackScreenDuration / 2f);

        // Mover al jugador al destino
        if (teleportDestination != null)
        {
            playerObj.transform.position = teleportDestination.position;
        }
        else
        {
            Debug.LogWarning("¡Falta asignar el destino (teleportDestination) en el inspector!");
        }

        // Cambiamos la luz global (si la hemos asignado) antes de quitar el negro
        if (luzGlobal != null)
        {
            luzGlobal.intensity = intensidadDestino;
        }

        // Esperar el resto del tiempo de pantalla negra antes de revelar
        yield return new WaitForSeconds(blackScreenDuration / 2f);

        // Quitar el fundido negro (Fade Out)
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(elapsedTime / fadeDuration));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);

        // Reactivar el PlayerController
        foreach (var script in scripts)
        {
            if (script.GetType().Name == "PlayerController")
            {
                script.enabled = true;
            }
        }

        isTeleporting = false;
    }
}
