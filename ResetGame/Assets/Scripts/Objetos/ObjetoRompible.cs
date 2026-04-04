using UnityEngine;

public class ObjetoRompible : MonoBehaviour
{
    [Header("Configuracion de la Caja")]
    public int golpesParaRomper = 3;

    [Header("FeedBack Visual")]
    public float duracionShake = 0.1f;
    public float magnitudShake = 0.1f;
    private bool estaAgitandose = false;

    [Header("Recompensas (Drop Aleatorio)")]
    [Tooltip("Arrastra aqui los PREFABS (el objeto fisico del suelo, no la ficha ScriptableObject)")]
    public GameObject[] posiblesDrops;

    public void RecibirDano(int cantidadDano)
    {
        golpesParaRomper -= cantidadDano;

        Debug.Log("Has golpeado la caja. Le quedan: " + golpesParaRomper + " golpes.");

        if (golpesParaRomper <= 0)
        {
            Romper();
        }
        else
        {
            if (!estaAgitandose)
            {
                StartCoroutine(ShakeCoroutine());
            }
        }
    }

    private System.Collections.IEnumerator ShakeCoroutine()
    {
        estaAgitandose = true;
        Vector3 posActual = transform.position;
        float tiempo = 0f;

        while (tiempo < duracionShake)
        {
            float xOffset = Random.Range(-1f, 1f) * magnitudShake;
            float yOffset = Random.Range(-1f, 1f) * magnitudShake;

            transform.position = posActual + new Vector3(xOffset, yOffset, 0f);

            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.position = posActual;
        estaAgitandose = false;
    }

    private void Romper()
    {
        if (posiblesDrops != null && posiblesDrops.Length > 0)
        {
            int indiceAleatorio = Random.Range(0, posiblesDrops.Length);
            GameObject dropElegido = posiblesDrops[indiceAleatorio];

            if (dropElegido != null)
            {
                Instantiate(dropElegido, transform.position, Quaternion.identity);
                Debug.Log("La caja se rompio y solto un objeto.");
            }
        }
        else
        {
            Debug.Log("La caja se rompio pero estaba vacia.");
        }

        Destroy(gameObject);
    }
}
