using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

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
    public bool EstaLeyendo => leyendo;
    public bool CerradoEsteFrame { get; private set; } // Indica si la nota se ha cerrado en este frame exacto
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
        if (leyendo)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) CerrarNota();
            return;
        }

        //Interaccion de mantener la E para leer
        if (esperandoLectura)
        {
            if (Keyboard.current.eKey.isPressed)
            {
                tiempoMantenido += Time.deltaTime;

                if (tiempoMantenido > 1.0f)
                {
                    AbrirNota();
                }
            }
            else
            {
                tiempoMantenido = 0f; //Si suelta la tecla antes de tiempo, el contador vuelve a 0
            }
        }
    }

    private void AbrirNota()
    {
        if (rutinaAviso != null) StopCoroutine(rutinaAviso);
        panelAviso.SetActive(false);
        esperandoLectura = false;
        tiempoMantenido = 0f;

        juegoPausado = (Time.timeScale == 0f); //Si esta parado antes de abrir la nota o no, es decir, si abrimos desde el inventario o desde acceso rapido

        panelNotaGrande.SetActive(true);
        textoNotaGrande.text = notaPendiente;
        Time.timeScale = 0f;
        leyendo = true;
    }

    public void CerrarNota()
    {
        panelNotaGrande.SetActive(false);
        leyendo = false;
        CerradoEsteFrame = true;

        if (juegoPausado)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void LateUpdate()
    {
        CerradoEsteFrame = false;
    }
    public void LeerNotaDesdeInventario(string textoDeLaNota)
    {
        notaPendiente = textoDeLaNota;
        AbrirNota();
    }

    public void MostrarMensajeRapido(string mensaje)
    {
        textoAviso.text = mensaje;
        esperandoLectura = false;

        if (rutinaAviso != null) StopCoroutine(rutinaAviso);
        rutinaAviso = StartCoroutine(RutinaMensajeRapidoTimer());
    }

    private IEnumerator RutinaMensajeRapidoTimer()
    {
        panelAviso.SetActive(true);
        yield return new WaitForSeconds(3f);
        panelAviso.SetActive(false);
    }
}
