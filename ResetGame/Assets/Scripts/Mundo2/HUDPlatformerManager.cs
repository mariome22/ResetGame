using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDPlatformerManager : MonoBehaviour
{
    public static HUDPlatformerManager Instance;

    [Header("UI Vidas")]
    [Tooltip("Texto para mostrar las vidas del jugador (ej. 'x3')")]
    public TextMeshProUGUI livesText;

    [Header("UI Monedas")]
    [Tooltip("Texto para mostrar las monedas normales recogidas (ej. 'x100')")]
    public TextMeshProUGUI coinsText;

    [Header("UI Monedas Ocultas (Galletas)")]
    [Tooltip("Las 3 imágenes de los huecos de galletas en el HUD")]
    public Image[] secretCookieSlots = new Image[3];
    
    [Tooltip("Sprite de la galleta vacía (el contorno/hueco vacío)")]
    public Sprite emptyCookieSprite;
    
    [Tooltip("Sprite de la galleta llena (recolectada)")]
    public Sprite filledCookieSprite;

    [Tooltip("Opacidad (Alpha) de la galleta vacía, entre 0 (invisible) y 1 (totalmente opaco)")]
    [Range(0f, 1f)]
    public float emptyCookieAlpha = 0.3f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Actualizar el HUD al iniciar la escena para mostrar los valores correctos
        UpdateHUD();
    }

    /// <summary>
    /// Actualiza todos los elementos del HUD usando las variables estáticas de PlayerPlatformerController.
    /// </summary>
    public void UpdateHUD()
    {
        // 1. Actualizar texto de vidas
        if (livesText != null)
        {
            livesText.text = "x" + PlayerPlatformerController.lives.ToString();
        }

        // 2. Actualizar texto de monedas
        if (coinsText != null)
        {
            coinsText.text = "x" + PlayerPlatformerController.totalCoins.ToString();
        }

        // 3. Actualizar los 3 huecos de galletas secretas con su respectiva opacidad
        int secretCount = PlayerPlatformerController.secretCoinsCollected;
        for (int i = 0; i < secretCookieSlots.Length; i++)
        {
            if (secretCookieSlots[i] != null)
            {
                Color c = secretCookieSlots[i].color;
                if (i < secretCount)
                {
                    // Si ya se ha recolectado esta galleta, mostramos el sprite lleno con opacidad al 100%
                    if (filledCookieSprite != null)
                    {
                        secretCookieSlots[i].sprite = filledCookieSprite;
                    }
                    c.a = 1f;
                    secretCookieSlots[i].color = c;
                    secretCookieSlots[i].enabled = true;
                }
                else
                {
                    // Si no se ha recolectado aún, mostramos el sprite vacío con opacidad reducida
                    if (emptyCookieSprite != null)
                    {
                        secretCookieSlots[i].sprite = emptyCookieSprite;
                    }
                    else if (filledCookieSprite != null)
                    {
                        // Si no asignaron un sprite vacío, usamos el lleno pero transparente
                        secretCookieSlots[i].sprite = filledCookieSprite;
                    }
                    
                    c.a = emptyCookieAlpha;
                    secretCookieSlots[i].color = c;
                    secretCookieSlots[i].enabled = true;
                }
            }
        }
    }
}
