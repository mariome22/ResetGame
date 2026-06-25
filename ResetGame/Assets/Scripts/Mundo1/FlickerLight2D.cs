using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FlickerLight2D : MonoBehaviour
{
    private Light2D light2D;
    private SpriteRenderer spriteRenderer;
    private Image uiImage;

    [Header("Rango de Intensidad")]
    public float intensidadMinima = 0.4f;
    public float intensidadMaxima = 1.6f;

    [Header("Ajustes del Parpadeo")]
    [Tooltip("Frecuencia con la que cambia la intensidad (segundos)")]
    public float velocidadParpadeo = 0.08f;

    [Tooltip("Probabilidad de que ocurra un micro-apagón (glitch) por cambio")]
    [Range(0f, 1f)]
    public float probabilidadGlitch = 0.1f;

    public bool IsGlitching { get; private set; }

    private float siguienteCambio = 0f;

    private void Start()
    {
        light2D = GetComponent<Light2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();

        if (light2D == null && spriteRenderer == null && uiImage == null)
        {
            Debug.LogError("FlickerLight2D requiere un componente Light2D, SpriteRenderer o UI Image en el mismo GameObject.", this);
        }
    }

    private void Update()
    {
        if (Time.time < siguienteCambio) return;

        siguienteCambio = Time.time + velocidadParpadeo;

        float nuevaIntensidad;

        // Decidir si hay un mini "apagón" (glitch errático)
        if (Random.value < probabilidadGlitch)
        {
            nuevaIntensidad = Random.Range(0.05f, intensidadMinima);
            IsGlitching = true;
        }
        else
        {
            nuevaIntensidad = Random.Range(intensidadMinima, intensidadMaxima);
            IsGlitching = false;
        }

        // Aplicar al componente que esté presente
        if (light2D != null)
        {
            light2D.intensity = nuevaIntensidad;
        }
        else if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = nuevaIntensidad; // Usamos la transparencia para simular intensidad de brillo
            spriteRenderer.color = c;
        }
        else if (uiImage != null)
        {
            Color c = uiImage.color;
            c.a = nuevaIntensidad;
            uiImage.color = c;
        }
    }
}
