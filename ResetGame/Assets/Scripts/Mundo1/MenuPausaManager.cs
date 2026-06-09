using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPausaManager : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    [Tooltip("Arrastra aquí el GameObject de tu Canvas Menu_Pausa u objeto principal que lo contiene")]
    public GameObject canvasPausa;

    private bool estaPausado = false;

    private void Start()
    {
        // Asegurarnos de que empiece cerrado
        if (canvasPausa != null)
        {
            canvasPausa.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Abrir/Cerrar con la tecla I
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        if (canvasPausa == null) return;

        estaPausado = !estaPausado;

        if (estaPausado)
        {
            // Abrir menú y pausar tiempo
            canvasPausa.SetActive(true);
            Time.timeScale = 0f;
            
            // Actualizar inventario visualmente justo al abrir
            if (InventarioManager.Instance != null)
            {
                InventarioManager.Instance.ActualizarMenuPausa();
                InventarioManager.Instance.MostrarPestanaObjetos(); // Fuerza el refresco visual de las pestañas
            }
        }
        else
        {
            // Cerrar menú y reanudar tiempo
            canvasPausa.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void CerrarMenuPausa()
    {
        if (estaPausado)
        {
            AlternarPausa();
        }
    }
}
