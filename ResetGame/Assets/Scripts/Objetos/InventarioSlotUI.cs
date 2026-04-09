using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventarioSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Componentes Visuales")]
    [Tooltip("La imagen donde se pondrá el icono del objeto")]
    public Image iconoObjeto;
    
    [Tooltip("El texto donde aparecerá el título (Principalmente para Coleccionables)")]
    public TextMeshProUGUI textoTitulo;

    private ItemData currentItem;

    public void ActualizarSlot(ItemData item)
    {
        currentItem = item;
        // Si no hay item para este hueco, lo vaciamos
        if (item == null)
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
        }
        else
        {
            // Hay un item, asignamos el icono y texto
            if (iconoObjeto != null)
            {
                iconoObjeto.sprite = item.iconoObjeto;
                iconoObjeto.color = new Color(1f, 1f, 1f, 1f); // Opaco para hacerlo visible
            }
            if (textoTitulo != null)
            {
                // Mostramos el nombre configurado en su Scriptable Object
                textoTitulo.text = item.nombreObjeto;
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
