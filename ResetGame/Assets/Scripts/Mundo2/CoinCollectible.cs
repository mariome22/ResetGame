using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinCollectible : MonoBehaviour
{
    [Header("Configuración de la Moneda")]
    [Tooltip("¿Es una moneda secreta (especial)?")]
    [SerializeField] private bool isSecretCoin = false;

    [Header("Efectos (Opcional)")]
    [Tooltip("Prefab de partículas a instanciar al recolectar la moneda.")]
    [SerializeField] private GameObject collectEffectPrefab;
    [Tooltip("Sonido a reproducir al recolectar la moneda.")]
    [SerializeField] private AudioClip collectSound;

    private bool isCollected = false;

    private string GetUniqueKey()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + transform.position.ToString("F3");
    }

    private void Awake()
    {
        // Forzamos a que el colisionador sea Trigger para que el jugador pase a través de él al recogerlo
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        // Si esta moneda ya se recolectó de forma persistente (guardada en checkpoint), se destruye al cargar la escena
        string key = GetUniqueKey();
        if (PlayerPlatformerController.collectedCoinsActive.Contains(key))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Evitamos doble recolección en el mismo frame
        if (isCollected) return;

        if (collision.CompareTag("Player"))
        {
            PlayerPlatformerController player = collision.GetComponent<PlayerPlatformerController>();
            if (player != null)
            {
                isCollected = true;

                // Registramos esta moneda como recolectada en la sesión activa
                PlayerPlatformerController.RegisterCollectedCoin(GetUniqueKey());

                // Lógica del sistema de monedas
                if (isSecretCoin)
                {
                    player.CollectSecretCoin();
                }
                else
                {
                    player.AddCoins(1);
                }

                // Reproducimos los efectos
                PlayEffects();

                // Destruimos la moneda de la escena
                Destroy(gameObject);
            }
        }
    }

    private void PlayEffects()
    {
        // Instanciamos partículas si existen
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // Reproducimos sonido si existe
        if (collectSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(collectSound);
            }
            else
            {
                // Reproduce el clip de forma segura en la posición 3D
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
        }
    }
}
