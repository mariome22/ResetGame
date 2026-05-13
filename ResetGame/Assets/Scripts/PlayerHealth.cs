using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidaMaxima = 8;
    private int vidaActual;

    [Header("Invulnerabilidad (I-Frames)")]
    public float tiempoInvulnerable = 1f;
    private bool esInvulnerable = false;

    [Header("Feedback Visual")]
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

        //Temblor de cámara al recibir daño
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

    public bool Curar(int cantidadCuracion)
    {
        if (vidaActual >= vidaMaxima)
        {
            Debug.Log("Vida al máximo. No se puede curar más.");
            return false;
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

    private IEnumerator RutinaCuracionVisual()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.green;
        yield return new WaitForSeconds(0.15f);
        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
    }

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
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ActualizarVida(vidaActual);
        }
    }

    public float GetPorcentajeVida()
    {
        return (float)vidaActual / vidaMaxima;
    }

    private void Morir()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
