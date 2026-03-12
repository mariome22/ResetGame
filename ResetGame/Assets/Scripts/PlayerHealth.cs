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
    private Color colorOriginal; // 1. Variable para memorizar tu color

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. Guardamos el color que le hayas puesto en el Inspector
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

        ActualizarHUD();
    }

    public void RecibirDano(int cantidadDano)
    {
        if (esInvulnerable) return;

        vidaActual -= cantidadDano;
        ActualizarHUD();

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(RutinaInvulnerabilidad());
        }
    }

    private IEnumerator RutinaInvulnerabilidad()
    {
        esInvulnerable = true;

        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);

        // 3. Volvemos al color original en lugar de a Color.white
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