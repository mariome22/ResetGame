using UnityEngine;

public class ArmaRecogible : MonoBehaviour
{
    [Header("Datos del Arma")]
    [Tooltip("Arrastra aquí el ItemData (Scriptable Object) de tu arma")]
    public ItemData datosDelArma;

    /// <summary>
    /// Esta función debe ser llamada desde el evento de OnInteract 
    /// en el componente InteractableObject.
    /// </summary>
    public void RecogerArma()
    {
        // 1. Añadimos el objeto al inventario visual/lógico
        if (datosDelArma != null && InventarioManager.Instance != null)
        {
            InventarioManager.Instance.AnadirObjeto(datosDelArma);
        }

        // 2. Buscamos al jugador y le indicamos que ya puede disparar
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            PlayerController player = jugador.GetComponent<PlayerController>();
            if (player != null)
            {
                player.DesbloquearArmaADistancia();
            }
        }

        // 3. Destruimos el objeto del suelo (desaparece)
        Destroy(gameObject);
    }
}
