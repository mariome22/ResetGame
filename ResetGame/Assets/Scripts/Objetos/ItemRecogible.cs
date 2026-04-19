using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    [Header("Identificador del Objeto")]
    [Tooltip("Arrastra aquí el DNI (Scriptable Object) que creaste en la carpeta")]
    public ItemData datosDelObjeto;

        [Tooltip("Cantidad que da al recogerlo (ej. 5 balas).")]
    public int cantidadOtorga = 1;

    public void RecogerObjeto()
    {
        if (datosDelObjeto == null) return;

        if (InventarioManager.Instance != null)
        {
            InventarioManager.Instance.AnadirObjeto(datosDelObjeto, cantidadOtorga);
        }

        if (datosDelObjeto.tipo == ItemData.TipoObjeto.Documento)
        {
            if (LectorNotas.Instance != null)
            {
                LectorNotas.Instance.ActivarAviso(datosDelObjeto.nombreObjeto, datosDelObjeto.contenidoDocumento);
            }
        }

        Destroy(gameObject);
    }
}
