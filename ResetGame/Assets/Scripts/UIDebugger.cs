using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        // Solo comprueba al hacer click izquierdo usando exclusivamente el New Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                string log = "🖱️ CLICK DETECTADO EN:\n";
                for (int i = 0; i < results.Count; i++)
                {
                    Canvas canvasPadre = results[i].gameObject.GetComponentInParent<Canvas>();
                    string nombreCanvas = canvasPadre != null ? canvasPadre.name : "Sin Canvas";
                    log += (i + 1) + ". " + results[i].gameObject.name + " (en el Canvas: " + nombreCanvas + ")\n";
                }
                Debug.Log(log);
            }
            else
            {
                Debug.Log("❌ CLICK AL VACÍO (El Raycast de la UI no golpea nada).");
            }
        }
    }
}
