using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventarioSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Componentes Visuales")]
    [Tooltip("La imagen donde se pondrÃ¡ el icono del objeto")]
    public Image iconoObjeto;
    
    [Tooltip("El texto donde aparecerÃ¡ el tÃ­tulo (Principalmente para Coleccionables)")]
    public TextMeshProUGUI textoTitulo;
    [Tooltip("El texto donde aparecera el numero de objetos acumulados")]
    public TextMeshProUGUI textoCantidad;

    [Header("Selección")]
    [Tooltip("La imagen de fondo o borde que indica que el slot esta seleccionado")]
    public GameObject imagenSeleccion;

    private ItemData currentItem;

    public ItemData GetCurrentItem()
    {
        return currentItem;
    }

    public void SetSeleccionado(bool seleccionado)
    {
        if (imagenSeleccion != null) imagenSeleccion.SetActive(seleccionado);
        
        if (iconoObjeto != null && currentItem != null)
        {
            // Ilumina el icono si está seleccionado, oscurece un poco si no.
            iconoObjeto.color = seleccionado ? new Color(1f, 1f, 1f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
        }
    }

    public void ActualizarSlot(InventarioSlot slot)
    {
                if (slot == null) currentItem = null;
        else currentItem = slot.objeto;
        // Si no hay item para este hueco, lo vaciamos
        if (slot == null || slot.objeto == null)
        {
            if (iconoObjeto != null)
            {
                iconoObjeto.sprite = null;
                iconoObjeto.color = new Color(1f, 1f, 1f, 0f); // Transparente para ocultar el recuadro blanco
            }
            if (textoTitulo != null)
            {
                textoTitulo.text = "";
            }
            if (textoCantidad != null)
            {
                textoCantidad.text = "";
            }
        }
        else
        {
            // Hay un item, asignamos el icono y texto
            if (iconoObjeto != null)
            {
                iconoObjeto.sprite = slot.objeto.iconoObjeto;
                iconoObjeto.color = new Color(1f, 1f, 1f, 1f); // Opaco para hacerlo visible
            }
            if (textoTitulo != null)
            {
                // Mostramos el nombre configurado en su Scriptable Object
                textoTitulo.text = slot.objeto.nombreObjeto;
            }
            if (textoCantidad != null) {
                if (slot.objeto.esAcumulable && slot.cantidad > 1) textoCantidad.text = "" + slot.cantidad;
                else textoCantidad.text = "";
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem != null && InventarioManager.Instance != null)
        {
            InventarioManager.Instance.MostrarDetallesObjeto(currentItem);
        }
    }
}
