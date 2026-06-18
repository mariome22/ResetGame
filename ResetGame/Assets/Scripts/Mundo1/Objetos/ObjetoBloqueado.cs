using UnityEngine;

public class ObjetoBloqueado : MonoBehaviour
{
    [Header("Requisitos")]
    public ItemData llaveNecesaria;

    [Tooltip("�La llave desaparece de la mochila al usarla?")]
    public bool gastarLlaveAlAbrir = true;

    [Header("Textos Inmersivos")]
    [Tooltip("Lo que dice si intentas abrir sin la llave")]
    public string textoFaltaLlave = "Est� bloqueado. Necesito algo para abrirlo.";

    [Tooltip("Lo que dice justo cuando se abre")]
    public string textoExito = "�Abierto!";

    [Header("Recompensa")]
    [Tooltip("El Prefab del n�cleo (o el objeto que haya dentro)")]
    public GameObject prefabNucleo;

    public Transform puntoDeDrop;

    private void Start()
    {
        PersistentObject po = GetComponent<PersistentObject>();
        if (po != null)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string uniqueId = string.IsNullOrEmpty(po.uniqueId) ? 
                $"{sceneName}_{gameObject.name}_{transform.position.x:F2}_{transform.position.y:F2}" : 
                po.uniqueId;

            if (SaveManager.Instance != null && SaveManager.Instance.IsObjectDestroyed(uniqueId))
            {
                DesactivarInteraccionYColision();
            }
        }
    }

    private void DesactivarInteraccionYColision()
    {
        InteractableObject interaccion = GetComponent<InteractableObject>();
        if (interaccion != null)
        {
            interaccion.enabled = false;
        }

        Collider2D miCollider = GetComponent<Collider2D>();
        if (miCollider != null)
        {
            miCollider.enabled = false;
        }
    }

    public void IntentarAbrir()
    {
        if (InventarioManager.Instance == null) return;

        if (InventarioManager.Instance.TieneObjeto(llaveNecesaria))
        {
            if (LectorNotas.Instance != null)
            {
                LectorNotas.Instance.MostrarMensajeRapido(textoExito);
            }

            if (gastarLlaveAlAbrir)
            {
                InventarioManager.Instance.GastarObjeto(llaveNecesaria);
            }

            if (prefabNucleo != null)
            {
                Vector3 posicionDrop = (puntoDeDrop != null) ? puntoDeDrop.position : transform.position;
                Instantiate(prefabNucleo, posicionDrop, Quaternion.identity);
            }

            PersistentObject po = GetComponent<PersistentObject>();
            if (po != null) po.RegisterDestruction();

            DesactivarInteraccionYColision();
        }
        else
        {
            if (LectorNotas.Instance != null)
            {
                LectorNotas.Instance.MostrarMensajeRapido(textoFaltaLlave);
            }
        }
    }
}