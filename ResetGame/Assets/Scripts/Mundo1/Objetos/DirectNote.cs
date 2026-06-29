using UnityEngine;

public class DirectNote : MonoBehaviour
{
    [TextArea(5, 15)]
    [Tooltip("El texto que se mostrará en pantalla al interactuar con este cartel, póster o pintada.")]
    public string textoDelCartel;

    // Este método se vincula al evento OnInteract() del InteractableObject en el editor
    public void MostrarTextoEnPantalla()
    {
        if (string.IsNullOrEmpty(textoDelCartel))
        {
            Debug.LogWarning("No hay texto asignado para mostrar en el cartel " + gameObject.name);
            return;
        }

        if (LectorNotas.Instance != null)
        {
            // Abre el panel de lectura directamente con el texto de este cartel
            LectorNotas.Instance.LeerNotaDesdeInventario(textoDelCartel);
        }
        else
        {
            Debug.LogError("No se encontró LectorNotas en la escena. Asegúrate de tener el manager.");
        }
    }
}
