using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidaMaxima = 3;
    private int vidaActual;

    [Header("Invulnerabilidad (I-Frames)")]
    public float tiempoInvulnerable = 1f;
    private bool esInvulnerable = false;

    [Header("UI y Feedback")]
    public TextMeshProUGUI textoVida;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

        ActualizarHUD();
    }

    public void RecibirDano(int cantidadDano)
    {
        if (esInvulnerable) return;

        vidaActual -= cantidadDano;
        ActualizarHUD();

        // Temblor de cámara al recibir daño
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(1f);

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(RutinaInvulnerabilidad());
        }
    }

    // --- NUEVO: FUNCIÓN DE CURACIÓN ---
    // AHORA DEVUELVE UN BOOL (Verdadero o Falso)
    public bool Curar(int cantidadCuracion)
    {
        if (vidaActual >= vidaMaxima)
        {
            Debug.Log("Vida al máximo. No se puede curar más.");
            return false; // <-- Avisamos de que NO nos hemos curado
        }

        vidaActual += cantidadCuracion;

        if (vidaActual > vidaMaxima)
        {
            vidaActual = vidaMaxima;
        }

        ActualizarHUD();
        StartCoroutine(RutinaCuracionVisual());

        return true;
    }

    // --- NUEVO: FEEDBACK VISUAL ---
    private IEnumerator RutinaCuracionVisual()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.green;
        yield return new WaitForSeconds(0.15f);
        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
    }
    // ----------------------------------

    private IEnumerator RutinaInvulnerabilidad()
    {
        esInvulnerable = true;

        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);

        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;

        yield return new WaitForSeconds(tiempoInvulnerable - 0.15f);
        esInvulnerable = false;
    }

    private void ActualizarHUD()
    {
        if (textoVida != null) textoVida.text = "Vidas: " + vidaActual;
    }

    private void Morir()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}