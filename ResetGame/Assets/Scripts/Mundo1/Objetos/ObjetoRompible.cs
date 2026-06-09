using UnityEngine;

public class ObjetoRompible : MonoBehaviour
{
    [Header("Configuracion de la Caja")]
    public int golpesParaRomper = 3;
    public bool puedeSoltarBalas = true;

    [Header("FeedBack Visual")]
    public float duracionShake = 0.1f;
    public float magnitudShake = 0.1f;
    private bool estaAgitandose = false;

    [Header("Recompensas (Prefabs)")]
    public GameObject prefabVendas;
    public GameObject prefabBotiquin;
    public GameObject prefabBalas;

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
        // 1. Obtener estado del jugador
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            float porcentajeVida = 1f;
            int totalBalas = 0;

            PlayerHealth playerHealth = jugador.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                porcentajeVida = playerHealth.GetPorcentajeVida();
            }

            PlayerController playerController = jugador.GetComponent<PlayerController>();
            if (playerController != null)
            {
                totalBalas += playerController.balasActualesCargador;
            }

            if (InventarioManager.Instance != null)
            {
                totalBalas += InventarioManager.Instance.ContarMunicionTotal();
            }

            // 2. Obtener pesos
            int pesoVendas, pesoBotiquin, pesoBalas, pesoNada;
            CalcularPesos(porcentajeVida, totalBalas, out pesoVendas, out pesoBotiquin, out pesoBalas, out pesoNada);

            // 3. Elegir drop
            GameObject dropElegido = ElegirDrop(pesoVendas, pesoBotiquin, pesoBalas, pesoNada);

            if (dropElegido != null)
            {
                Instantiate(dropElegido, transform.position, Quaternion.identity);
                Debug.Log("La caja se rompio y solto un objeto: " + dropElegido.name);
            }
            else
            {
                Debug.Log("La caja se rompio pero no solto nada (Director IA).");
            }
        }
        else
        {
            Debug.LogWarning("No se encontro al jugador. La caja no suelta nada.");
        }

        Destroy(gameObject);
    }

    private void CalcularPesos(float porcentajeVida, int totalBalas, out int pesoVendas, out int pesoBotiquin, out int pesoBalas, out int pesoNada)
    {
        bool vidaBaja = porcentajeVida <= 0.35f;
        bool vidaAlta = porcentajeVida >= 0.9f;
        bool vidaMediaAlta = porcentajeVida >= 0.5f;

        bool balasAltas = totalBalas >= 15;
        bool balasMedias = totalBalas > 10;

        if (vidaAlta && balasAltas)
        {
            pesoVendas = 10;
            pesoBotiquin = 5;
            pesoBalas = 10;
            pesoNada = 75;
        }
        else if (vidaAlta) 
        {
            pesoVendas = 15;
            pesoBotiquin = 5;
            pesoBalas = 30;
            pesoNada = 50;
        }
        else if (balasAltas && vidaBaja)
        {
            pesoBalas = 0;
            pesoVendas = 50;
            pesoBotiquin = 35;
            pesoNada = 15;
        }
        else if (balasAltas && vidaMediaAlta)
        {
            pesoBalas = 0;
            pesoNada = 35;
            pesoVendas = 50;
            pesoBotiquin = 15;
        }
        else if (porcentajeVida > 0.35f && balasMedias)
        {
            pesoVendas = 50;
            pesoNada = 30;
            pesoBalas = 10;
            pesoBotiquin = 10;
        }
        else if (vidaBaja)
        {
            pesoVendas = 50;
            pesoBotiquin = 30;
            pesoBalas = 10;
            pesoNada = 10;
        }
        else
        {
            pesoVendas = 50;
            pesoBotiquin = 15;
            pesoBalas = 20;
            pesoNada = 15;
        }

        if (!puedeSoltarBalas)
        {
            pesoBalas = 0;
        }
    }

    private GameObject ElegirDrop(int pesoVendas, int pesoBotiquin, int pesoBalas, int pesoNada)
    {
        int totalPeso = pesoVendas + pesoBotiquin + pesoBalas + pesoNada;
        int rng = Random.Range(0, totalPeso);

        if (rng < pesoVendas)
            return prefabVendas;
        rng -= pesoVendas;

        if (rng < pesoBotiquin)
            return prefabBotiquin;
        rng -= pesoBotiquin;

        if (rng < pesoBalas)
            return prefabBalas;
        
        return null; // Nada
    }
}
