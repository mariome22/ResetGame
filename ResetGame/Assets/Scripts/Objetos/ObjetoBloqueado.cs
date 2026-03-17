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

        // ¿Tenemos la llave?
        if (InventarioManager.Instance.TieneObjeto(llaveNecesaria))
        {
            // 1. Mostramos el mensaje de éxito en pantalla
            if (LectorNotas.Instance != null)
            {
                LectorNotas.Instance.MostrarMensajeRapido(textoExito);
            }

            // 2. Gastamos la llave (se borra de la mochila)
            if (gastarLlaveAlAbrir)
            {
                InventarioManager.Instance.GastarObjeto(llaveNecesaria);
            }

            // 3. Dropeamos el núcleo al suelo
            if (prefabNucleo != null)
            {
                Vector3 posicionDrop = (puntoDeDrop != null) ? puntoDeDrop.position : transform.position;
                Instantiate(prefabNucleo, posicionDrop, Quaternion.identity);
            }

            // 4. Apagamos la interacción para que no salga más la 'E'
            InteractableObject interaccion = GetComponent<InteractableObject>();
            if (interaccion != null)
            {
                // Escondemos la E visual si estaba encendida
                if (interaccion.visualCue != null) interaccion.visualCue.SetActive(false);
                // Apagamos el script para que no se pueda volver a pulsar
                interaccion.enabled = false;
            }

            // Opcional: Apagamos el Collider para que el jugador pueda atravesar la puerta abierta
            Collider2D miCollider = GetComponent<Collider2D>();
            if (miCollider != null) miCollider.enabled = false;

        }
        else
        {
            // Mostramos el mensaje de error inmersivo en pantalla
            if (LectorNotas.Instance != null)
            {
                // Aquí el texto dice lo que hayas puesto en el Inspector
                LectorNotas.Instance.MostrarMensajeRapido(textoFaltaLlave);
            }
        }
    }
}