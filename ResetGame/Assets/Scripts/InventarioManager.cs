using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventarioManager : MonoBehaviour
{
    public static InventarioManager Instance;

    [Header("Almacenamiento")]
    public List<ItemData> objetosGuardados = new List<ItemData>();
    public List<ItemData> coleccionablesGuardados = new List<ItemData>();
    public int capacidadObjetos = 15;

    [Header("Interfaz HUD")]
    [Tooltip("Arrastra aquí el texto del Canvas que mostrará el objeto equipado")]
    public TextMeshProUGUI textoObjetoEquipado;

    [Header("Interfaz Menú Pausa")]
    [Tooltip("Panel que contiene los 15 huecos de objetos (los huecos deben tener InventarioSlotUI)")]
    public Transform panelObjetos;
    [Tooltip("Panel que contiene los huecos de coleccionables (los huecos deben tener InventarioSlotUI)")]
    public Transform panelColeccionables;
    [Tooltip("Opcional: Prefab del hueco para Coleccionables (para crear más si superan la capacidad inicial de la UI)")]
    public GameObject prefabHuecoColeccionable;

    [Header("Detalles de Objeto (Menú Pausa)")]
    public TextMeshProUGUI textoNombreDetalle;
    public TextMeshProUGUI textoDescripcionDetalle;

    private int indiceSeleccionado = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ActualizarUI();
        ActualizarMenuPausa();
    }

    public void AnadirObjeto(ItemData nuevoObjeto)
    {
        if (nuevoObjeto.tipo == ItemData.TipoObjeto.Documento)
        {
            coleccionablesGuardados.Add(nuevoObjeto);
            Debug.Log("Coleccionable guardado: " + nuevoObjeto.nombreObjeto);
            ActualizarUI();
            ActualizarMenuPausa();
        }
        else
        {
            if (objetosGuardados.Count < capacidadObjetos)
            {
                objetosGuardados.Add(nuevoObjeto);
                Debug.Log("Guardado en la mochila: " + nuevoObjeto.nombreObjeto);
                ActualizarUI();
                ActualizarMenuPausa();
            }
            else
            {
                Debug.Log("¡El inventario está lleno!");
            }
        }
    }

    public void CambiarSeleccion()
    {
        if (objetosGuardados.Count == 0) return;

        int intentos = 0;
        do
        {
            indiceSeleccionado++;
            if (indiceSeleccionado >= objetosGuardados.Count)
            {
                indiceSeleccionado = 0;
            }
            intentos++;
        }
        while (objetosGuardados[indiceSeleccionado].tipo != ItemData.TipoObjeto.Curacion && intentos < objetosGuardados.Count);

        ActualizarUI();
    }

    public void UsarObjetoSeleccionado()
    {
        if (objetosGuardados.Count == 0)
        {
            Debug.Log("No tienes nada en el inventario.");
            return;
        }

        // Clip de seguridad si el indiceSeleccionado quedó desfasado
        if (indiceSeleccionado >= objetosGuardados.Count) {
             indiceSeleccionado = 0;
        }

        ItemData objetoAConsumir = objetosGuardados[indiceSeleccionado];

        //Verificamos si es curativo antes de intentar curar
        if (objetoAConsumir.tipo == ItemData.TipoObjeto.Curacion)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                PlayerHealth vidaJugador = jugador.GetComponent<PlayerHealth>();
                if (vidaJugador != null)
                {
                    if (vidaJugador.Curar(objetoAConsumir.valorEfecto))
                    {
                        //Si se curó con éxito, lo borramos de la mochila
                        objetosGuardados.RemoveAt(indiceSeleccionado);
                        Debug.Log("Has consumido: " + objetoAConsumir.nombreObjeto);

                        //Ajustamos el índice por si borramos el último objeto de la lista
                        if (indiceSeleccionado >= objetosGuardados.Count)
                        {
                            indiceSeleccionado = 0;
                        }

                        ActualizarUI();
                        ActualizarMenuPausa();
                    }
                }
            }
        }
        else
        {
            Debug.Log("No puedes usar " + objetoAConsumir.nombreObjeto + " de esta forma.");
        }
    }

    private void ActualizarUI()
    {
        if (textoObjetoEquipado == null) return;

        bool tenemosCuras = false;
        foreach (var item in objetosGuardados)
        {
            if (item.tipo == ItemData.TipoObjeto.Curacion) tenemosCuras = true;
        }

        if (!tenemosCuras)
        {
            textoObjetoEquipado.text = "Cura: Nada";
            return;
        }

        // Protección extra
        if (indiceSeleccionado >= objetosGuardados.Count) 
        {
            indiceSeleccionado = 0;
            if (objetosGuardados.Count == 0) return;
        }

        //Cambio forzado por si apunta a un item distinto a cura en el HUD
        if (objetosGuardados[indiceSeleccionado].tipo != ItemData.TipoObjeto.Curacion)
        {
            CambiarSeleccion();
        }
        else
        {
            textoObjetoEquipado.text = "Cura: " + objetosGuardados[indiceSeleccionado].nombreObjeto;
        }
    }

    public void ActualizarMenuPausa()
    {
        MostrarDetallesObjeto(null); // Limpiar detalles al actualizar

        // 1. Actualizar panel de objetos normales
        if (panelObjetos != null)
        {
            InventarioSlotUI[] slotsObjetos = panelObjetos.GetComponentsInChildren<InventarioSlotUI>();
            for (int i = 0; i < slotsObjetos.Length; i++)
            {
                if (i < objetosGuardados.Count)
                {
                    slotsObjetos[i].ActualizarSlot(objetosGuardados[i]);
                }
                else
                {
                    slotsObjetos[i].ActualizarSlot(null);
                }
            }
        }

        // 2. Actualizar panel de coleccionables
        if (panelColeccionables != null)
        {
            InventarioSlotUI[] slotsCol = panelColeccionables.GetComponentsInChildren<InventarioSlotUI>();
            
            // Si hay más coleccionables que huecos, instanciamos más automáticamente
            while (slotsCol.Length < coleccionablesGuardados.Count && prefabHuecoColeccionable != null)
            {
                Instantiate(prefabHuecoColeccionable, panelColeccionables);
                // Refrescamos la lista después de instanciar uno nuevo
                slotsCol = panelColeccionables.GetComponentsInChildren<InventarioSlotUI>(); 
            }

            for (int i = 0; i < slotsCol.Length; i++)
            {
                if (i < coleccionablesGuardados.Count)
                {
                    slotsCol[i].ActualizarSlot(coleccionablesGuardados[i]);
                }
                else
                {
                    slotsCol[i].ActualizarSlot(null); // Ocultar info pero mantener recuadro visible si queremos
                }
            }
        }
    }

    public bool TieneObjeto(ItemData objetoRequerido)
    {
        return objetosGuardados.Contains(objetoRequerido) || coleccionablesGuardados.Contains(objetoRequerido);
    }

    public void GastarObjeto(ItemData objetoAGastar)
    {
        if (objetosGuardados.Contains(objetoAGastar))
        {
            objetosGuardados.Remove(objetoAGastar);
            ActualizarUI();
            ActualizarMenuPausa();
        }
        else if (coleccionablesGuardados.Contains(objetoAGastar))
        {
            coleccionablesGuardados.Remove(objetoAGastar);
            ActualizarUI();
            ActualizarMenuPausa();
        }
    }

    public void MostrarDetallesObjeto(ItemData objeto)
    {
        if (objeto != null)
        {
            if (textoNombreDetalle != null) textoNombreDetalle.text = objeto.nombreObjeto;
            if (textoDescripcionDetalle != null) textoDescripcionDetalle.text = objeto.descripcion;
        }
        else
        {
            if (textoNombreDetalle != null) textoNombreDetalle.text = "";
            if (textoDescripcionDetalle != null) textoDescripcionDetalle.text = "";
        }
    }
}
