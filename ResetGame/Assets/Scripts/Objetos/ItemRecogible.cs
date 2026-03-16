using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    [Header("Identificador del Objeto")]
    [Tooltip("Arrastra aquí el DNI (Scriptable Object) que creaste en la carpeta")]
    public ItemData datosDelObjeto;

    // Esta función es la que llamaremos desde el Inspector
    public void RecogerObjeto()
    {
        if (datosDelObjeto == null)
        {
            Debug.LogWarning("¡A este objeto le falta su ItemData en el Inspector!");
            return;
        }

        // Aquí en el futuro llamaremos a tu inventario:
        // InventarioManager.Instance.AnadirObjeto(datosDelObjeto);

        Debug.Log("Has recogido del suelo: " + datosDelObjeto.nombreObjeto);

        // Destruimos el objeto del suelo
        Destroy(gameObject);
    }
}