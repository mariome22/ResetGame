using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public int capacidadObjetos = 4;

    [Header("Interfaz Menu Pausa - Paneles")]
    [Tooltip("Panel que contiene los 15 huecos de objetos (los huecos deben tener InventarioSlotUI)")]
    public Transform panelObjetos;
    [Tooltip("Panel completo de la pestaña de coleccionables")]
    public Transform panelColeccionables;
    [Tooltip("Panel de las opciones")]
    public Transform panelOpciones;
    [Tooltip("El objeto dentro del panel que tiene el VerticalLayoutGroup para apilar la lista")]
    public Transform contenedorListaColeccionables;
    [Tooltip("Opcional: Prefab del hueco para Coleccionables (para crear mas si superan la capacidad inicial de la UI)")]
    public GameObject prefabHuecoColeccionable;

    [Header("Interfaz Menu Pausa - Pestañas (Botones)")]
    public TextMeshProUGUI textoBtnObjetos;
    public TextMeshProUGUI textoBtnColeccionables;
    public TextMeshProUGUI textoBtnOpciones;
    public GameObject lineaObjetos;
    public GameObject lineaColeccionables;
    public GameObject lineaOpciones;

    [Header("Detalles de Objeto (Menu Pausa)")]
    public TextMeshProUGUI textoNombreDetalle;
    public TextMeshProUGUI textoDescripcionDetalle;

    [Header("Detalles Coleccionables (Menu Pausa)")]
    public TextMeshProUGUI textoNombreColDetalle;
    public Image iconoColDetalle;

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
            if (nuevoObjeto.tipo == ItemData.TipoObjeto.Municion)
            {
                PlayerController pc = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
                int balasEnCargador = pc != null ? pc.balasActualesCargador : 0;
                int balasTotales = balasEnCargador + ContarMunicionTotal();
                int maxBalasTotales = 20;

                if (balasTotales >= maxBalasTotales)
                {
                    Debug.Log("Límite de 20 balas alcanzado.");
                    return false;
                }

                int espacioDisp = maxBalasTotales - balasTotales;
                if (cantidad > espacioDisp)
                {
                    cantidad = espacioDisp;
                }
            }

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

    public void ActualizarUI()
    {
        bool tenemosCuras = false;
        foreach (var slot in objetosGuardados)
        {
            if (slot.objeto.tipo == ItemData.TipoObjeto.Curacion) tenemosCuras = true;
        }

        if (!tenemosCuras)
        {
            if (HUDManager.Instance != null) HUDManager.Instance.ActualizarCura(false, null);
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
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ActualizarCura(true, objetosGuardados[indiceSeleccionado].objeto.iconoObjeto);
            }
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
            Transform contenedor = contenedorListaColeccionables != null ? contenedorListaColeccionables : panelColeccionables;
            InventarioSlotUI[] slotsCol = contenedor.GetComponentsInChildren<InventarioSlotUI>();
            
            int proteccionBucle = 0;
            while (slotsCol.Length < coleccionablesGuardados.Count && prefabHuecoColeccionable != null)
            {
                GameObject nuevoSlot = Instantiate(prefabHuecoColeccionable, contenedor);
                
                // Si el prefab que el usuario arrastró no tiene el script, salimos para no colgar Unity
                if (nuevoSlot.GetComponentInChildren<InventarioSlotUI>() == null)
                {
                    Debug.LogError("¡ERROR CRITICO! El PrefabHuecoColeccionable que has asignado NO tiene el script 'InventarioSlotUI' puesto. ¡Se ha abortado para evitar que Unity explote!");
                    break;
                }

                slotsCol = contenedor.GetComponentsInChildren<InventarioSlotUI>(); 
                
                proteccionBucle++;
                if (proteccionBucle > 50) break; // Seguro extra
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

            // Forzamos a Unity a recalcular el tamaño del contenedor al instante para que la Scrollbar se entere
            LayoutRebuilder.ForceRebuildLayoutImmediate(contenedor.GetComponent<RectTransform>());
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
            if (objeto.tipo == ItemData.TipoObjeto.Documento)
            {
                if (textoNombreColDetalle != null) textoNombreColDetalle.text = objeto.nombreObjeto;
                if (iconoColDetalle != null) 
                {
                    iconoColDetalle.sprite = objeto.iconoObjeto;
                    iconoColDetalle.enabled = objeto.iconoObjeto != null;
                }
            }
            else
            {
                if (textoNombreDetalle != null) textoNombreDetalle.text = objeto.nombreObjeto;
                if (textoDescripcionDetalle != null) textoDescripcionDetalle.text = objeto.descripcion;
                if (iconoColDetalle != null) iconoColDetalle.enabled = false;
            }
        }
        else
        {
            if (textoNombreDetalle != null) textoNombreDetalle.text = "";
            if (textoDescripcionDetalle != null) textoDescripcionDetalle.text = "";
            if (textoNombreColDetalle != null) textoNombreColDetalle.text = "";
            if (iconoColDetalle != null) 
            {
                iconoColDetalle.sprite = null;
                iconoColDetalle.enabled = false;
            }
        }

        ActualizarSeleccionSlots();
    }

    private void ActualizarSeleccionSlots()
    {
        if (panelObjetos != null)
        {
            InventarioSlotUI[] slotsObjetos = panelObjetos.GetComponentsInChildren<InventarioSlotUI>();
            foreach (var slot in slotsObjetos)
            {
                slot.SetSeleccionado(slot.GetCurrentItem() == objetoViendoDetalles && objetoViendoDetalles != null);
            }
        }

        if (panelColeccionables != null)
        {
            Transform contenedor = contenedorListaColeccionables != null ? contenedorListaColeccionables : panelColeccionables;
            InventarioSlotUI[] slotsCol = contenedor.GetComponentsInChildren<InventarioSlotUI>();
            foreach (var slot in slotsCol)
            {
                slot.SetSeleccionado(slot.GetCurrentItem() == objetoViendoDetalles && objetoViendoDetalles != null);
            }
        }
    }

    public void BotonLeerColeccionable()
    {
        if (objetoViendoDetalles != null && objetoViendoDetalles.tipo == ItemData.TipoObjeto.Documento)
        {
            if (LectorNotas.Instance != null)
            {
                LectorNotas.Instance.LeerNotaDesdeInventario(objetoViendoDetalles.contenidoDocumento);
            }
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
        if (panelOpciones != null) panelOpciones.gameObject.SetActive(false);
        MostrarDetallesObjeto(null);
        ActualizarEstiloPestanas(0);
    }

    public void MostrarPestanaColeccionables()
    {
        if (panelObjetos != null) panelObjetos.gameObject.SetActive(false);
        if (panelColeccionables != null) panelColeccionables.gameObject.SetActive(true);
        if (panelOpciones != null) panelOpciones.gameObject.SetActive(false);
        MostrarDetallesObjeto(null);
        ActualizarEstiloPestanas(1);
    }

    public void MostrarPestanaOpciones()
    {
        if (panelObjetos != null) panelObjetos.gameObject.SetActive(false);
        if (panelColeccionables != null) panelColeccionables.gameObject.SetActive(false);
        if (panelOpciones != null) panelOpciones.gameObject.SetActive(true);
        MostrarDetallesObjeto(null);
        ActualizarEstiloPestanas(2);
    }

    private void ActualizarEstiloPestanas(int indiceActiva)
    {
        Color colorActivo = Color.white;
        Color colorInactivo = new Color(0.6f, 0.6f, 0.6f, 1f); // Gris oscuro

        if (textoBtnObjetos != null) textoBtnObjetos.color = (indiceActiva == 0) ? colorActivo : colorInactivo;
        if (textoBtnColeccionables != null) textoBtnColeccionables.color = (indiceActiva == 1) ? colorActivo : colorInactivo;
        if (textoBtnOpciones != null) textoBtnOpciones.color = (indiceActiva == 2) ? colorActivo : colorInactivo;

        if (lineaObjetos != null) lineaObjetos.SetActive(indiceActiva == 0);
        if (lineaColeccionables != null) lineaColeccionables.SetActive(indiceActiva == 1);
        if (lineaOpciones != null) lineaOpciones.SetActive(indiceActiva == 2);
    }
}
