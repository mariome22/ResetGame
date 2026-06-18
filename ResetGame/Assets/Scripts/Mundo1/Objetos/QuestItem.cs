using UnityEngine;

public class QuestItem : MonoBehaviour
{
    [Tooltip("La puerta a la que pertenece este objeto")]
    public QuestDoor vinculadaAMision;

    [Header("Inventario (Opcional)")]
    [Tooltip("El ItemData (Scriptable Object) para mostrar este objeto en el inventario.")]
    public ItemData datosDelObjeto;

    // Este método se llamará desde el OnInteract del InteractableObject añadido a este item
    public void OnItemInteract()
    {
        if (vinculadaAMision != null)
        {
            // Añadir al inventario si se asignó un ItemData
            if (datosDelObjeto != null && InventarioManager.Instance != null)
            {
                bool recogido = InventarioManager.Instance.AnadirObjeto(datosDelObjeto, 1);
                if (!recogido)
                {
                    if (LectorNotas.Instance != null) LectorNotas.Instance.MostrarMensajeRapido("Inventario lleno.");
                    return; // Abortar recolección si el inventario está lleno
                }
            }

            // Le decimos a la puerta que hemos recogido un objeto
            vinculadaAMision.RegistarObjetoRecogido();
            
            // Opcional: Aquí podrías añadir una partícula de recolección o sonido
            // Instantiate(particulaRecoleccion, transform.position, Quaternion.identity);

            PersistentObject po = GetComponent<PersistentObject>();
            if (po != null) po.RegisterDestruction();

            // Desactivamos el objeto (o lo destruimos con Destroy(gameObject))
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Este QuestItem no tiene una puerta vinculada.");
        }
    }
}
