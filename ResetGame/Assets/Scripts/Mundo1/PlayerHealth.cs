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

    [Header("Knockback")]
    [Tooltip("Fuerza del retroceso que recibe el jugador al ser dañado.")]
    public float fuerzaKnockback = 8f;
    [Tooltip("Duración en segundos del estado de retroceso/aturdimiento.")]
    public float duracionKnockback = 0.2f;

    [Header("UI Muerte")]
    [Tooltip("Panel UI que se muestra al morir")]
    public GameObject panelMuerte;

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

        ActualizarHUD();
    }

    public void RecibirDano(int cantidadDano, Vector2? posicionOrigen = null)
    {
        if (esInvulnerable) return;

        vidaActual -= cantidadDano;
        ActualizarHUD();

        //Temblor de cámara al recibir daño
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(1f);

        if (posicionOrigen.HasValue)
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                Vector2 direccionKnockback = ((Vector2)transform.position - posicionOrigen.Value).normalized;
                if (direccionKnockback == Vector2.zero)
                {
                    direccionKnockback = Vector2.up;
                }
                pc.AplicarKnockback(direccionKnockback, fuerzaKnockback, duracionKnockback);
            }
        }

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
        StartCoroutine(MorirRoutine());
    }

    private IEnumerator MorirRoutine()
    {
        // Desactivar controles del jugador en Mundo 1
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.enabled = false;
        }

        // Intentar activar trigger de muerte en el Animator si existe
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Muerte");
            animator.SetFloat("Velocidad", 0f);
        }

        yield return new WaitForSeconds(1.0f);

        if (panelMuerte == null)
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            yield break;
        }

        // Hacer fundido a negro antes de mostrar el panel de muerte
        if (SceneTransitionManager.Instance != null)
        {
            bool fadeDone = false;
            SceneTransitionManager.Instance.FadeOut(0.5f, () => {
                fadeDone = true;
            });
            yield return new WaitUntil(() => fadeDone);
        }

        panelMuerte.SetActive(true);
        Time.timeScale = 0f; // Pausar el juego al morir

        // Fundido de vuelta a transparente revelando el panel
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.FadeIn(0.5f, null);
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
