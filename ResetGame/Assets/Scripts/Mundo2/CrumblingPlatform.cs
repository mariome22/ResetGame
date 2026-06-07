using UnityEngine;
using System.Collections;

public class CrumblingPlatform : MonoBehaviour
{
    [Header("Tiempos y Cooldowns")]
    [Tooltip("Tiempo de advertencia (temblor) antes de que la plataforma se caiga/desaparezca.")]
    public float shakeDuration = 0.5f;

    [Tooltip("Tiempo que la plataforma permanece invisible antes de reaparecer.")]
    public float respawnTime = 3f;

    [Header("Efecto de Temblor")]
    [Tooltip("Intensidad de la vibración de advertencia.")]
    public float shakeIntensity = 0.05f;

    [Header("Efectos Visuales")]
    [Tooltip("Efecto de partículas opcional que se instanciará al colapsar la plataforma.")]
    public GameObject crumbleEffectPrefab;

    private Collider2D platformCollider;
    private Renderer platformRenderer;
    private Vector3 originalPosition;
    private bool isCrumbling = false;

    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
        platformRenderer = GetComponentInChildren<Renderer>();
        if (platformRenderer == null)
        {
            platformRenderer = GetComponent<Renderer>();
        }
        originalPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCrumbling) return;

        // Comprobar si el que colisiona es el jugador
        PlayerPlatformerController player = collision.gameObject.GetComponent<PlayerPlatformerController>();
        if (player != null)
        {
            // Validar que la colisión sea desde arriba (el jugador pisa la plataforma)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Un vector normal con Y menor a -0.7 indica contacto en la parte superior
                if (contact.normal.y < -0.7f)
                {
                    StartCoroutine(CrumbleRoutine());
                    break;
                }
            }
        }
    }

    private IEnumerator CrumbleRoutine()
    {
        isCrumbling = true;

        // --- 1. FASE DE TEMBLOR ---
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float randomX = Random.Range(-shakeIntensity, shakeIntensity);
            float randomY = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = originalPosition + new Vector3(randomX, randomY, 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }

        // Restaurar posición base antes de desaparecer
        transform.position = originalPosition;

        // --- 2. FASE DE DESAPARICIÓN ---
        if (platformCollider != null) platformCollider.enabled = false;
        if (platformRenderer != null) platformRenderer.enabled = false;

        // Instanciar efectos visuales de colapso
        if (crumbleEffectPrefab != null)
        {
            GameObject effect = Instantiate(crumbleEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // --- 3. FASE DE COOLDOWN (ESPERA) ---
        yield return new WaitForSeconds(respawnTime);

        // --- 4. FASE DE REAPARICIÓN ---
        if (platformRenderer != null) platformRenderer.enabled = true;
        if (platformCollider != null) platformCollider.enabled = true;
        isCrumbling = false;
    }

    // Asegurarse de que si el objeto se destruye o desactiva en el editor, vuelve a su posición original
    private void OnDisable()
    {
        transform.position = originalPosition;
        isCrumbling = false;
    }
}
