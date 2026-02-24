using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 1. OBLIGATORIO PARA USAR TEXTOS

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidaMaxima = 3;
    private int vidaActual;

    [Header("UI")]
    public TextMeshProUGUI textoVida; // 2. EL HUECO PARA EL TEXTO

    private void Start()
    {
        vidaActual = vidaMaxima;
        ActualizarHUD(); // Llamamos a esto al empezar
    }

    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;
        ActualizarHUD(); // Actualizamos el texto al recibir daño

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    // 3. NUEVA FUNCIÓN PARA CAMBIAR EL TEXTO
    private void ActualizarHUD()
    {
        if (textoVida != null)
        {
            textoVida.text = "Vidas: " + vidaActual;
        }
    }

    private void Morir()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}