using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VictoryPanelAnimator : MonoBehaviour
{
    [Header("Componentes UI")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI levelCompletedText;
    [SerializeField] private Image coreImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private CanvasGroup buttonCanvasGroup;

    [Header("Ajustes de Animacion")]
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float elementDelay = 0.3f;
    [SerializeField] private bool typewriterEffect = true;
    [SerializeField] private float typewriterSpeed = 0.05f;

    [Header("Animacion del Core Sprite")]
    [SerializeField] private bool animateCoreScale = true;
    [SerializeField] private Vector3 coreInitialScale = new Vector3(0.2f, 0.2f, 1f);
    [SerializeField] private Vector3 coreFinalScale = Vector3.one;

    private void OnEnable()
    {
        // Al activarse el panel, iniciamos la animación ignorando el Time.timeScale (ya que estará a 0)
        StartCoroutine(AnimatePanelRoutine());
    }

    private IEnumerator AnimatePanelRoutine()
    {
        // 1. Inicializar estados
        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0f;
        if (buttonCanvasGroup != null) buttonCanvasGroup.alpha = 0f;
        
        string originalCompletedText = "";
        if (levelCompletedText != null)
        {
            originalCompletedText = levelCompletedText.text;
            if (typewriterEffect)
            {
                levelCompletedText.text = "";
            }
            else
            {
                levelCompletedText.color = new Color(levelCompletedText.color.r, levelCompletedText.color.g, levelCompletedText.color.b, 0f);
            }
        }

        if (coreImage != null)
        {
            coreImage.color = new Color(coreImage.color.r, coreImage.color.g, coreImage.color.b, 0f);
            if (animateCoreScale)
            {
                coreImage.transform.localScale = coreInitialScale;
            }
        }

        if (descriptionText != null)
        {
            descriptionText.color = new Color(descriptionText.color.r, descriptionText.color.g, descriptionText.color.b, 0f);
        }

        // 2. Fundido a negro/transparencia del panel principal
        if (panelCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                panelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            panelCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(elementDelay);

        // 3. Animación de las letras (Level Completed)
        if (levelCompletedText != null)
        {
            if (typewriterEffect)
            {
                levelCompletedText.color = new Color(levelCompletedText.color.r, levelCompletedText.color.g, levelCompletedText.color.b, 1f);
                for (int i = 0; i <= originalCompletedText.Length; i++)
                {
                    levelCompletedText.text = originalCompletedText.Substring(0, i);
                    yield return new WaitForSecondsRealtime(typewriterSpeed);
                }
            }
            else
            {
                float elapsed = 0f;
                Color txtColor = levelCompletedText.color;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    levelCompletedText.color = new Color(txtColor.r, txtColor.g, txtColor.b, Mathf.Clamp01(elapsed / fadeDuration));
                    yield return null;
                }
                levelCompletedText.color = new Color(txtColor.r, txtColor.g, txtColor.b, 1f);
            }
        }

        yield return new WaitForSecondsRealtime(elementDelay);

        // 4. Fundido y escalado del Core Sprite
        if (coreImage != null)
        {
            float elapsed = 0f;
            Color imgColor = coreImage.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeDuration);
                
                coreImage.color = new Color(imgColor.r, imgColor.g, imgColor.b, progress);
                if (animateCoreScale)
                {
                    coreImage.transform.localScale = Vector3.Lerp(coreInitialScale, coreFinalScale, progress);
                }
                yield return null;
            }
            coreImage.color = new Color(imgColor.r, imgColor.g, imgColor.b, 1f);
            if (animateCoreScale)
            {
                coreImage.transform.localScale = coreFinalScale;
            }
        }

        yield return new WaitForSecondsRealtime(elementDelay);

        // 5. Fundido del texto descriptivo
        if (descriptionText != null)
        {
            float elapsed = 0f;
            Color descColor = descriptionText.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                descriptionText.color = new Color(descColor.r, descColor.g, descColor.b, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            descriptionText.color = new Color(descColor.r, descColor.g, descColor.b, 1f);
        }

        yield return new WaitForSecondsRealtime(elementDelay);

        // 6. Fundido del botón de continuar
        if (buttonCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                buttonCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            buttonCanvasGroup.alpha = 1f;
        }
    }
}
