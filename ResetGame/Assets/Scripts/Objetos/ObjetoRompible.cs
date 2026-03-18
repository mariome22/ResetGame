using UnityEngine;

public class ObjetoRompible : MonoBehaviour
{
    [Header("Configuración de la Caja")]
    public int golpesParaRomper = 3;

    [Header("Recompensas (Drop Aleatorio)")]
    [Tooltip("Arrastra aquí los PREFABS (el objeto físico del suelo, no la ficha ScriptableObject)")]
    public GameObject[] posiblesDrops;

    public void RecibirDano(int cantidadDano)
    {
        golpesParaRomper -= cantidadDano;

        Debug.Log("Has golpeado la caja. Le quedan: " + golpesParaRomper + " golpes.");

        if (golpesParaRomper <= 0)
        {
            Romper();
        }
    }

    private void Romper()
    {
        if (posiblesDrops != null && posiblesDrops.Length > 0)
        {
            //Random.Range con números enteros incluye el primero, pero EXCLUYE el último. 
            //Por eso ponemos (0, Length).
            int indiceAleatorio = Random.Range(0, posiblesDrops.Length);
            GameObject dropElegido = posiblesDrops[indiceAleatorio];

            if (dropElegido != null)
            {
                Instantiate(dropElegido, transform.position, Quaternion.identity);
                Debug.Log("La caja se rompió y soltó un objeto.");
            }
        }
        else
        {
            Debug.Log("La caja se rompió pero estaba vacía (no pusiste drops en el Inspector).");
        }

        Destroy(gameObject);
    }
}