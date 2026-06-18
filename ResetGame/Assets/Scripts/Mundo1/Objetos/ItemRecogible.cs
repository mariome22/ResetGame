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

        bool recogido = true;

        if (InventarioManager.Instance != null)
        {
            recogido = InventarioManager.Instance.AnadirObjeto(datosDelObjeto, cantidadOtorga);
        }

        if (!recogido)
        {
            if (LectorNotas.Instance != null)
            {
                LectorNotas.Instance.MostrarMensajeRapido("Inventario lleno.");
            }
            return; // Abortamos, no destruimos el objeto
        }

        if (datosDelObjeto.tipo == ItemData.TipoObjeto.Documento)
        {
            if (LectorNotas.Instance != null)
            {
                LectorNotas.Instance.ActivarAviso(datosDelObjeto.nombreObjeto, datosDelObjeto.contenidoDocumento);
            }
        }

        PersistentObject po = GetComponent<PersistentObject>();
        if (po != null) po.RegisterDestruction();

        Destroy(gameObject);
    }
}
