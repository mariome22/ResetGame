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

    [Header("Ajustes del Parpadeo / Variación")]
    [Tooltip("Frecuencia o velocidad con la que cambia la intensidad")]
    public float velocidadParpadeo = 0.08f;

    [Tooltip("Probabilidad de que ocurra un micro-apagón (glitch) por cambio (solo modo parpadeo errático)")]
    [Range(0f, 1f)]
    public float probabilidadGlitch = 0.1f;

    [Header("Variación Suave (Nubes/Ambiente)")]
    [Tooltip("Si está activo, la intensidad cambiará de forma suave y continua usando ruido de Perlin en lugar de parpadeos bruscos")]
    public bool variacionSuave = false;

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
        float nuevaIntensidad;

        if (variacionSuave)
        {
            // Variación suave usando ruido de Perlin (se ejecuta cada frame para máxima fluidez)
            // Se usa la posición del objeto como desfase (offset) para que las luces no pulsen idénticamente al mismo tiempo
            float offset = (transform.position.x * 12.3f) + (transform.position.y * 7.7f);
            float t = (Time.unscaledTime * velocidadParpadeo) + offset;
            float ruido = Mathf.PerlinNoise(t, 0f);
            
            nuevaIntensidad = Mathf.Lerp(intensidadMinima, intensidadMaxima, ruido);
            IsGlitching = false;
        }
        else
        {
            // Parpadeo errático clásico (neon, pantallas, glitches)
            // IMPORTANTE: Se usa Time.unscaledTime para que funcione incluso si el juego está pausado (Time.timeScale = 0)
            if (Time.unscaledTime < siguienteCambio) return;

            siguienteCambio = Time.unscaledTime + velocidadParpadeo;

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
