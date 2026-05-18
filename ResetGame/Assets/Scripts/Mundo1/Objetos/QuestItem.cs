using UnityEngine;

public class QuestItem : MonoBehaviour
{
    [Tooltip("La puerta a la que pertenece este objeto")]
    public QuestDoor vinculadaAMision;

    // Este método se llamará desde el OnInteract del InteractableObject añadido a este item
    public void OnItemInteract()
    {
        if (vinculadaAMision != null)
        {
            // Le decimos a la puerta que hemos recogido un objeto
            vinculadaAMision.RegistarObjetoRecogido();
            
            // Opcional: Aquí podrías añadir una partícula de recolección o sonido
            // Instantiate(particulaRecoleccion, transform.position, Quaternion.identity);

            // Desactivamos el objeto (o lo destruimos con Destroy(gameObject))
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Este QuestItem no tiene una puerta vinculada.");
        }
    }
}
