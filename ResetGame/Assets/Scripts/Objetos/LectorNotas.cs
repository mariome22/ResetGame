using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // Usamos el nuevo sistema para leer el teclado directamente aquí

public class LectorNotas : MonoBehaviour
{
    public static LectorNotas Instance;

    [Header("UI: Aviso Temporal (5 seg)")]
    public GameObject panelAviso;
    public TextMeshProUGUI textoAviso;

    [Header("UI: Lectura Completa")]
    public GameObject panelNotaGrande;
    public TextMeshProUGUI textoNotaGrande;

    private bool leyendo = false;
    private bool esperandoLectura = false;
    private string notaPendiente = "";
    private float tiempoMantenido = 0f;
    private Coroutine rutinaAviso;
    private bool juegoPausado = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        panelAviso.SetActive(false);
        panelNotaGrande.SetActive(false);
    }

    public void ActivarAviso(string nombreNota, string textoNota)
    {
        notaPendiente = textoNota;
        textoAviso.text = "Mantén [E] para leer: " + nombreNota;

        if (rutinaAviso != null) StopCoroutine(rutinaAviso);
        rutinaAviso = StartCoroutine(RutinaAvisoTimer());
    }

    private IEnumerator RutinaAvisoTimer()
    {
        esperandoLectura = true;
        panelAviso.SetActive(true);
        tiempoMantenido = 0f;

        yield return new WaitForSeconds(5f);

        panelAviso.SetActive(false);
        esperandoLectura = false;
    }

    private void Update()
    {
        // 1. Si ya estamos leyendo la nota grande, pulsar Escape la cierra
        if (leyendo)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) CerrarNota();
            return; // Cortamos el Update aquí para no hacer nada más
        }

        // 2. Si el aviso de 5 segundos está en pantalla...
        if (esperandoLectura)
        {
            // Comprobamos si el jugador está manteniendo pulsada la 'E'
            if (Keyboard.current.eKey.isPressed)
            {
                tiempoMantenido += Time.deltaTime;

                // Si la mantiene más de 0.4 segundos (para evitar que se abra sola al recogerla con un toque rápido)
                if (tiempoMantenido > 1.0f)
                {
                    AbrirNota();
                }
            }
            else
            {
                tiempoMantenido = 0f; // Si suelta la tecla antes de tiempo, el contador vuelve a 0
            }
        }
    }

    private void AbrirNota()
    {
        if (rutinaAviso != null) StopCoroutine(rutinaAviso); // Cancelamos la cuenta atrás de 5s
        panelAviso.SetActive(false);
        esperandoLectura = false;
        tiempoMantenido = 0f;

        juegoPausado = (Time.timeScale == 0f); //Si esta parado antes de abrir la nota o no, es decir, si abrimos desde el inventario o desde acceso rapido

        panelNotaGrande.SetActive(true);
        textoNotaGrande.text = notaPendiente;
        Time.timeScale = 0f; // Pausamos el juego
        leyendo = true;
    }

    public void CerrarNota()
    {
        panelNotaGrande.SetActive(false);
        leyendo = false;

        // --- NUEVO: RESTAURAMOS EL TIEMPO CORRECTO ---
        if (juegoPausado)
        {
            // Si venías del inventario, el tiempo SE QUEDA en 0 (pausado)
            Time.timeScale = 0f;
        }
        else
        {
            // Si recogiste la nota del suelo en mitad del combate, el tiempo VUELVE a 1 (normal)
            Time.timeScale = 1f;
        }
    }
    // Esta función la llamará tu Menú de Inventario cuando hagas clic en una carta
    public void LeerNotaDesdeInventario(string textoDeLaNota)
    {
        notaPendiente = textoDeLaNota;
        AbrirNota();
    }

}