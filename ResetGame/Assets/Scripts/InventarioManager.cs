using System.Collections.Generic;
using UnityEngine;
using TMPro; // Necesario para el texto del Canvas

public class InventarioManager : MonoBehaviour
{
    public static InventarioManager Instance;

    public List<ItemData> objetosGuardados = new List<ItemData>();

    [Header("Interfaz (UI)")]
    [Tooltip("Arrastra aquí el texto del Canvas que mostrará el objeto equipado")]
    public TextMeshProUGUI textoObjetoEquipado;

    // Este es el "dedo" que apunta al objeto seleccionado
    private int indiceSeleccionado = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ActualizarUI();
    }

    public void AnadirObjeto(ItemData nuevoObjeto)
    {
        objetosGuardados.Add(nuevoObjeto);
        Debug.Log("Guardado en la mochila: " + nuevoObjeto.nombreObjeto);
        ActualizarUI(); // Actualizamos la pantalla al recoger algo
    }

    // --- NUEVO: FUNCIÓN PARA CAMBIAR DE OBJETO ---
    public void CambiarSeleccion()
    {
        if (objetosGuardados.Count == 0) return;

        int intentos = 0;
        // Movemos el dedo en bucle HASTA encontrar un objeto curativo (o dar la vuelta entera a la mochila)
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

    // --- MODIFICADO: AHORA SOLO USA EL SELECCIONADO ---
    public void UsarObjetoSeleccionado()
    {
        if (objetosGuardados.Count == 0)
        {
            Debug.Log("No tienes nada en el inventario.");
            return;
        }

        ItemData objetoAConsumir = objetosGuardados[indiceSeleccionado];

        // Verificamos si es curativo antes de intentar curar
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
                        // Si se curó con éxito, lo borramos de la mochila
                        objetosGuardados.RemoveAt(indiceSeleccionado);
                        Debug.Log("Has consumido: " + objetoAConsumir.nombreObjeto);

                        // Ajustamos el índice por si borramos el último objeto de la lista
                        if (indiceSeleccionado >= objetosGuardados.Count)
                        {
                            indiceSeleccionado = 0;
                        }

                        ActualizarUI();
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

        // Primero, comprobamos si llevamos AL MENOS una cura en toda la mochila
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

        // Si acabamos de recoger una nota y el dedo se ha quedado apuntando a ella, lo forzamos a cambiar
        if (objetosGuardados[indiceSeleccionado].tipo != ItemData.TipoObjeto.Curacion)
        {
            CambiarSeleccion();
        }
        else
        {
            textoObjetoEquipado.text = "Cura: " + objetosGuardados[indiceSeleccionado].nombreObjeto;
        }
    }

    // 1. El inventario comprueba si tienes la tarjeta roja
    public bool TieneObjeto(ItemData objetoRequerido)
    {
        return objetosGuardados.Contains(objetoRequerido);
    }

    // 2. El inventario borra la tarjeta roja de tu mochila al usarla en la puerta
    public void GastarObjeto(ItemData objetoAGastar)
    {
        if (objetosGuardados.Contains(objetoAGastar))
        {
            objetosGuardados.Remove(objetoAGastar);
            ActualizarUI(); // Actualiza el texto de la pantalla para que no se quede "Equipado: Tarjeta Roja" si ya no la tienes
        }
    }
}