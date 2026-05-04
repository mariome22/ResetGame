using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class InventarioSlot
{
    public ItemData objeto;
    public int cantidad;

    public InventarioSlot(ItemData obj, int cant)
    {
        objeto = obj;
        cantidad = cant;
    }
}

public class InventarioManager : MonoBehaviour
{
    public static InventarioManager Instance;

    [Header("Almacenamiento")]
    public List<InventarioSlot> objetosGuardados = new List<InventarioSlot>();
    public List<InventarioSlot> coleccionablesGuardados = new List<InventarioSlot>();
    public int capacidadObjetos = 3;

    [Header("Interfaz HUD")]
    [Tooltip("Arrastra aqui el texto del Canvas que mostrara el objeto equipado")]
    public TextMeshProUGUI textoObjetoEquipado;

    [Header("Interfaz Menu Pausa")]
    [Tooltip("Panel que contiene los 15 huecos de objetos (los huecos deben tener InventarioSlotUI)")]
    public Transform panelObjetos;
    [Tooltip("Panel que contiene los huecos de coleccionables (los huecos deben tener InventarioSlotUI)")]
    public Transform panelColeccionables;
    [Tooltip("Opcional: Prefab del hueco para Coleccionables (para crear mas si superan la capacidad inicial de la UI)")]
    public GameObject prefabHuecoColeccionable;

    [Header("Detalles de Objeto (Menu Pausa)")]
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

    public bool AnadirObjeto(ItemData nuevoObjeto, int cantidad = 1)
    {
        if (nuevoObjeto.tipo == ItemData.TipoObjeto.Documento)
        {
            coleccionablesGuardados.Add(new InventarioSlot(nuevoObjeto, cantidad));
            Debug.Log("Coleccionable guardado: " + nuevoObjeto.nombreObjeto);
            ActualizarUI();
            ActualizarMenuPausa();
            return true;
        }
        else
        {
            if (nuevoObjeto.esAcumulable)
            {
                foreach (InventarioSlot slot in objetosGuardados)
                {
                    if (slot.objeto == nuevoObjeto)
                    {
                        if (slot.cantidad < nuevoObjeto.cantidadMaxima)
                        {
                            int espacioLibre = nuevoObjeto.cantidadMaxima - slot.cantidad;
                            if (cantidad <= espacioLibre)
                            {
                                slot.cantidad += cantidad;
                                ActualizarUI();
                                ActualizarMenuPausa();
                                return true;
                            }
                            else
                            {
                                slot.cantidad += espacioLibre;
                                cantidad -= espacioLibre;
                            }
                        }
                    }
                }
            }

            if (objetosGuardados.Count < capacidadObjetos && cantidad > 0)
            {
                objetosGuardados.Add(new InventarioSlot(nuevoObjeto, cantidad));
                Debug.Log("Guardado en la mochila: " + nuevoObjeto.nombreObjeto);
                ActualizarUI();
                ActualizarMenuPausa();
                return true;
            }
            else if (cantidad > 0)
            {
                Debug.Log("Equipamiento lleno, no caben mas objetos.");
                return false;
            }
        }
        return false;
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
        while (objetosGuardados[indiceSeleccionado].objeto.tipo != ItemData.TipoObjeto.Curacion && intentos < objetosGuardados.Count);

        ActualizarUI();
    }

    public void UsarObjetoSeleccionado()
    {
        if (objetosGuardados.Count == 0)
        {
            Debug.Log("No tienes nada en el inventario.");
            return;
        }

        if (indiceSeleccionado >= objetosGuardados.Count) {
             indiceSeleccionado = 0;
        }

        InventarioSlot slot = objetosGuardados[indiceSeleccionado];

        if (slot.objeto.tipo == ItemData.TipoObjeto.Curacion)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                PlayerHealth vidaJugador = jugador.GetComponent<PlayerHealth>();
                if (vidaJugador != null)
                {
                    if (vidaJugador.Curar(slot.objeto.valorEfecto))
                    {
                        slot.cantidad--;
                        if (slot.cantidad <= 0)
                        {
                            objetosGuardados.RemoveAt(indiceSeleccionado);
                        }

                        if (indiceSeleccionado >= objetosGuardados.Count)
                        {
                            indiceSeleccionado = 0;
                        }

                        Debug.Log("Has consumido: " + slot.objeto.nombreObjeto);
                        ActualizarUI();
                        ActualizarMenuPausa();
                    }
                }
            }
        }
        else
        {
            Debug.Log("No puedes usar " + slot.objeto.nombreObjeto + " de esta forma.");
        }
    }

    public int ContarMunicionTotal()
    {
        int total = 0;
        foreach (var slot in objetosGuardados)
        {
            if (slot.objeto.tipo == ItemData.TipoObjeto.Municion)
            {
                total += slot.cantidad;
            }
        }
        return total;
    }

    public bool ExtraerMunicion(int cantidadNecesaria, out int cantidadExtraida)
    {
        cantidadExtraida = 0;
        for (int i = 0; i < objetosGuardados.Count; i++)
        {
            if (objetosGuardados[i].objeto.tipo == ItemData.TipoObjeto.Municion)
            {
                InventarioSlot municionSlot = objetosGuardados[i];
                if (municionSlot.cantidad >= cantidadNecesaria)
                {
                    municionSlot.cantidad -= cantidadNecesaria;
                    cantidadExtraida += cantidadNecesaria;
                    
                    if (municionSlot.cantidad <= 0) objetosGuardados.RemoveAt(i);
                    
                    ActualizarUI();
                    ActualizarMenuPausa();
                    return true;
                }
                else
                {
                    cantidadExtraida += municionSlot.cantidad;
                    cantidadNecesaria -= municionSlot.cantidad;
                    objetosGuardados.RemoveAt(i);
                    i--; // Ajustar indice despues de borrar
                }
            }
            if (cantidadNecesaria <= 0) break;
        }
        
        ActualizarUI();
        ActualizarMenuPausa();
        return cantidadExtraida > 0;
    }

    private void ActualizarUI()
    {
        if (textoObjetoEquipado == null) return;

        bool tenemosCuras = false;
        foreach (var slot in objetosGuardados)
        {
            if (slot.objeto.tipo == ItemData.TipoObjeto.Curacion) tenemosCuras = true;
        }

        if (!tenemosCuras)
        {
            textoObjetoEquipado.text = "Cura: Nada";
            return;
        }

        if (indiceSeleccionado >= objetosGuardados.Count) 
        {
            indiceSeleccionado = 0;
            if (objetosGuardados.Count == 0) return;
        }

        if (objetosGuardados[indiceSeleccionado].objeto.tipo != ItemData.TipoObjeto.Curacion)
        {
            CambiarSeleccion();
        }
        else
        {
            textoObjetoEquipado.text = "Cura: " + objetosGuardados[indiceSeleccionado].objeto.nombreObjeto + " (x" + objetosGuardados[indiceSeleccionado].cantidad + ")";
        }
    }

    public void ActualizarMenuPausa()
    {
        MostrarDetallesObjeto(null);

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

        if (panelColeccionables != null)
        {
            InventarioSlotUI[] slotsCol = panelColeccionables.GetComponentsInChildren<InventarioSlotUI>();
            
            while (slotsCol.Length < coleccionablesGuardados.Count && prefabHuecoColeccionable != null)
            {
                Instantiate(prefabHuecoColeccionable, panelColeccionables);
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
                    slotsCol[i].ActualizarSlot(null);
                }
            }
        }
    }

    public bool TieneObjeto(ItemData objetoRequerido)
    {
        foreach(var slot in objetosGuardados) { if (slot.objeto == objetoRequerido) return true; }
        foreach(var slot in coleccionablesGuardados) { if (slot.objeto == objetoRequerido) return true; }
        return false;
    }

    public void GastarObjeto(ItemData objetoAGastar)
    {
        for(int i = 0; i < objetosGuardados.Count; i++) {
            if (objetosGuardados[i].objeto == objetoAGastar) {
                objetosGuardados[i].cantidad--;
                if(objetosGuardados[i].cantidad <= 0) objetosGuardados.RemoveAt(i);
                ActualizarUI();
                ActualizarMenuPausa();
                return;
            }
        }
        for(int i = 0; i < coleccionablesGuardados.Count; i++) {
            if (coleccionablesGuardados[i].objeto == objetoAGastar) {
                coleccionablesGuardados.RemoveAt(i);
                ActualizarUI();
                ActualizarMenuPausa();
                return;
            }
        }
    }

    private ItemData objetoViendoDetalles;

    public void MostrarDetallesObjeto(ItemData objeto)
    {
        objetoViendoDetalles = objeto;
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

    public void BotonSoltarObjeto()
    {
        if (objetoViendoDetalles == null) return;
        
        for(int i = 0; i < objetosGuardados.Count; i++)
        {
            if (objetosGuardados[i].objeto == objetoViendoDetalles)
            {
                if (objetoViendoDetalles.prefabMundo != null)
                {
                    GameObject jugador = GameObject.FindGameObjectWithTag("Player");
                    Vector3 posDrop = jugador != null ? jugador.transform.position : Vector3.zero;
                    
                    GameObject dropObj = Instantiate(objetoViendoDetalles.prefabMundo, posDrop, Quaternion.identity);
                    ItemRecogible rec = dropObj.GetComponent<ItemRecogible>();
                    if (rec != null) rec.cantidadOtorga = 1;
                }
                else
                {
                    Debug.LogWarning("Este objeto no tiene prefabMundo asignado, se perderá al soltarlo.");
                }

                if (objetoViendoDetalles.tipo == ItemData.TipoObjeto.ArmaADistancia)
                {
                    GameObject jugador = GameObject.FindGameObjectWithTag("Player");
                    if (jugador != null)
                    {
                        PlayerController pc = jugador.GetComponent<PlayerController>();
                        if (pc != null) pc.PerderArmaADistancia();
                    }
                }

                objetosGuardados[i].cantidad--;
                if(objetosGuardados[i].cantidad <= 0)
                {
                    objetosGuardados.RemoveAt(i);
                    MostrarDetallesObjeto(null);
                }
                
                ActualizarUI();
                ActualizarMenuPausa();
                return;
            }
        }
    }

    public void BotonUsarObjeto()
    {
        if (objetoViendoDetalles == null) return;

        if (objetoViendoDetalles.tipo == ItemData.TipoObjeto.Curacion)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                PlayerHealth vidaJugador = jugador.GetComponent<PlayerHealth>();
                if (vidaJugador != null)
                {
                    if (vidaJugador.Curar(objetoViendoDetalles.valorEfecto))
                    {
                        GastarObjeto(objetoViendoDetalles);
                        Debug.Log("Has consumido: " + objetoViendoDetalles.nombreObjeto);
                    }
                }
            }
        }
        else
        {
            Debug.Log("No se puede usar este objeto desde el inventario.");
        }
    }

    public void MostrarPestanaObjetos()
    {
        if (panelObjetos != null) panelObjetos.gameObject.SetActive(true);
        if (panelColeccionables != null) panelColeccionables.gameObject.SetActive(false);
        MostrarDetallesObjeto(null);
    }

    public void MostrarPestanaColeccionables()
    {
        if (panelObjetos != null) panelObjetos.gameObject.SetActive(false);
        if (panelColeccionables != null) panelColeccionables.gameObject.SetActive(true);
        MostrarDetallesObjeto(null);
    }
}
