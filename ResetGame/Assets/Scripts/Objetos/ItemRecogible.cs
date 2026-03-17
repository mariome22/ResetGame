using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    [Header("Identificador del Objeto")]
    [Tooltip("Arrastra aquí el DNI (Scriptable Object) que creaste en la carpeta")]
    public ItemData datosDelObjeto;

    // Esta función es la que llamaremos desde el Inspector
    public void RecogerObjeto()
    {
        if (datosDelObjeto == null) return;

        if (InventarioManager.Instance != null)
        {
            InventarioManager.Instance.AnadirObjeto(datosDelObjeto);
        }

        if (datosDelObjeto.tipo == ItemData.TipoObjeto.Documento)
        {
            if (LectorNotas.Instance != null)
            {
                // Disparamos el cartel de 5 segundos
                LectorNotas.Instance.ActivarAviso(datosDelObjeto.nombreObjeto, datosDelObjeto.contenidoDocumento);
            }
        }
        // ------------------------------------------

        Destroy(gameObject);
    }
}