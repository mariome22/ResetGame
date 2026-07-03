using UnityEngine;
using System.Collections;

public class DashCrystal : MonoBehaviour
{
    [Header("Ajustes del Cristal")]
    [Tooltip("Tiempo en segundos que tarda en reaparecer tras ser recolectado.")]
    public float respawnTime = 3f;

    [Header("Efectos Visuales y Sonoros")]
    [Tooltip("Efecto de partículas opcional que se instanciará al romperse el cristal.")]
    public GameObject breakEffectPrefab;
    [Tooltip("Sonido a reproducir al recolectar el cristal.")]
    [SerializeField] private AudioClip collectSound;

    private Collider2D crystalCollider;
    private SpriteRenderer crystalSprite;
    private bool isCollected = false;

    void Start()
    {
        crystalCollider = GetComponent<Collider2D>();
        crystalSprite = GetComponentInChildren<SpriteRenderer>();
        if (crystalSprite == null)
        {
            crystalSprite = GetComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        PlayerPlatformerController player = other.GetComponent<PlayerPlatformerController>();
        if (player != null)
        {
            // Intentar recargar el dash. Solo consumimos el cristal si el jugador realmente necesitaba recargarlo.
            bool recharged = player.RechargeDash();
            if (recharged)
            {
                StartCoroutine(CollectRoutine());
            }
        }
    }

    private IEnumerator CollectRoutine()
    {
        isCollected = true;

        // Desactivar colisiones y parte visual
        if (crystalCollider != null) crystalCollider.enabled = false;
        if (crystalSprite != null) crystalSprite.enabled = false;

        // Instanciar efecto de partículas si está asignado
        if (breakEffectPrefab != null)
        {
            GameObject effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Destruir partículas tras 2 segundos
        }

        // Reproducir sonido de recolección
        if (collectSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(collectSound);
        }

        // Esperar el cooldown de reaparición
        yield return new WaitForSeconds(respawnTime);

        // Reaparecer
        if (crystalSprite != null) crystalSprite.enabled = true;
        if (crystalCollider != null) crystalCollider.enabled = true;
        isCollected = false;
    }
}
