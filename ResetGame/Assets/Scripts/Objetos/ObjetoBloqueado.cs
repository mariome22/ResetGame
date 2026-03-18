using UnityEngine;

public class ObjetoBloqueado : MonoBehaviour
{
    [Header("Requisitos")]
    public ItemData llaveNecesaria;

    [Tooltip("¿La llave desaparece de la mochila al usarla?")]
    public bool gastarLlaveAlAbrir = true;

    [Header("Textos Inmersivos")]
    [Tooltip("Lo que dice si intentas abrir sin la llave")]
    public string textoFaltaLlave = "Está bloqueado. Necesito algo para abrirlo.";

    [Tooltip("Lo que dice justo cuando se abre")]
    public string textoExito = "¡Abierto!";

    [Header("Recompensa")]
    [Tooltip("El Prefab del núcleo (o el objeto que haya dentro)")]
    public GameObject prefabNucleo;

    public Transform puntoDeDrop;

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

            //Apagar interaccion y la cue
            InteractableObject interaccion = GetComponent<InteractableObject>();
            if (interaccion != null)
            {
                if (interaccion.visualCue != null) interaccion.visualCue.SetActive(false);
                interaccion.enabled = false;
            }

            //Apagamos el Collider
            Collider2D miCollider = GetComponent<Collider2D>();
            if (miCollider != null) miCollider.enabled = false;

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