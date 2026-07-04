using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SceneTransitionManager>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    // Crear un objeto persistente dedicado en la raíz para asegurar que DontDestroyOnLoad funcione
                    GameObject obj = new GameObject("SceneTransitionManager_Persistent");
                    _instance = obj.AddComponent<SceneTransitionManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    public bool IsTransitionActive => canvasTransition != null && canvasTransition.enabled;

    [Header("Visuales de Transición")]
    [SerializeField] private Canvas canvasTransition;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCanvas();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCanvas()
    {
        // Si no se han asignado en el Inspector, crearlos automáticamente
        if (canvasTransition == null)
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvasObj.transform.SetParent(transform);
            
            canvasTransition = canvasObj.AddComponent<Canvas>();
            canvasTransition.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasTransition.sortingOrder = 999; // Asegurar que está por encima de todo
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform);
            
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            
            RectTransform rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        if (canvasTransition != null)
        {
            canvasTransition.enabled = false;
        }
    }

    public void FadeOut(float duration, System.Action onComplete)
    {
        StartCoroutine(FadeOutRoutine(duration, onComplete));
    }

    public void FadeIn(float duration, System.Action onComplete)
    {
        StartCoroutine(FadeInRoutine(duration, onComplete));
    }

    private IEnumerator FadeOutRoutine(float duration, System.Action onComplete)
    {
        if (canvasTransition != null) canvasTransition.enabled = true;
        if (fadeImage != null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }
            fadeImage.color = Color.black;
        }
        onComplete?.Invoke();
    }

    private IEnumerator FadeInRoutine(float duration, System.Action onComplete)
    {
        if (canvasTransition != null) canvasTransition.enabled = true;
        if (fadeImage != null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(1f - (elapsed / duration));
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }
        if (canvasTransition != null) canvasTransition.enabled = false;
        onComplete?.Invoke();
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        if (SaveManager.Instance != null && !SaveManager.Instance.isReloadingOnDeath)
        {
            SaveManager.Instance.SaveRuntimeState();
        }

        if (canvasTransition != null)
        {
            canvasTransition.enabled = true;
        }

        if (fadeImage == null)
        {
            SceneManager.LoadScene(sceneName);
            if (canvasTransition != null) canvasTransition.enabled = false;
            yield break;
        }

        // Fundido a negro (Fade Out)
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImage.color = Color.black;

        // Cargar escena
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Esperar un frame extra para inicialización de escena
        yield return null;

        // Fundido a transparente (Fade In)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        if (canvasTransition != null)
        {
            canvasTransition.enabled = false;
        }
    }
}
