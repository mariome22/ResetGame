using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPausaManager : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    [Tooltip("Arrastra aquí el GameObject de tu Canvas Menu_Pausa u objeto principal que lo contiene")]
    public GameObject canvasPausa;

    private bool estaPausado = false;
    private bool canvasEsElMismoObjeto = false;

    private void Start()
    {
        canvasEsElMismoObjeto = (canvasPausa == gameObject);

        // Asegurarnos de que empiece cerrado
        if (canvasPausa != null)
        {
            if (canvasEsElMismoObjeto)
            {
                SetChildrenActive(canvasPausa, false);
            }
            else
            {
                canvasPausa.SetActive(false);
            }
        }
    }

    private void Update()
    {
        bool iPressed = false;
        bool escPressed = false;

        if (Keyboard.current != null)
        {
            iPressed = Keyboard.current.iKey.wasPressedThisFrame;
            escPressed = Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame;
        }
        else
        {
            // Fallback al Input System antiguo si Keyboard.current es nulo
            iPressed = Input.GetKeyDown(KeyCode.I);
            escPressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
        }

        // Log temporal de depuración para la tecla ESC/I
        if (iPressed || escPressed)
        {
            Debug.Log($"[MenuPausaManager] Se pulsó ESC o I. estaPausado: {estaPausado}, IsSelectorOpen: {LevelSelectorController.IsSelectorOpen}, CerradoEsteFrame: {LevelSelectorController.CerradoEsteFrame}");
        }

        if (LevelSelectorController.IsSelectorOpen || LevelSelectorController.CerradoEsteFrame) return;

        // Abrir/Cerrar con la tecla I
        if (iPressed)
        {
            AlternarPausa();
        }

        // Abrir Opciones o Cerrar con la tecla ESC
        if (escPressed)
        {
            // Evitar conflicto si el jugador está leyendo una nota o la acaba de cerrar en este frame
            if (LectorNotas.Instance != null && (LectorNotas.Instance.EstaLeyendo || LectorNotas.Instance.CerradoEsteFrame))
            {
                return;
            }

            if (estaPausado)
            {
                AlternarPausa();
            }
            else
            {
                AlternarPausa();
                if (InventarioManager.Instance != null)
                {
                    InventarioManager.Instance.MostrarPestanaOpciones();
                }
            }
        }
    }

    private void SetChildrenActive(GameObject parent, bool active)
    {
        if (parent == null) return;
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(active);
        }
    }

    public void AlternarPausa()
    {
        if (canvasPausa == null) return;

        estaPausado = !estaPausado;

        if (estaPausado)
        {
            // Abrir menú y pausar tiempo
            if (canvasEsElMismoObjeto)
            {
                SetChildrenActive(canvasPausa, true);
            }
            else
            {
                canvasPausa.SetActive(true);
            }
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
            if (canvasEsElMismoObjeto)
            {
                SetChildrenActive(canvasPausa, false);
            }
            else
            {
                canvasPausa.SetActive(false);
            }
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
