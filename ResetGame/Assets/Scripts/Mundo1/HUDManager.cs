using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Salud")]
    [Tooltip("La imagen en el Canvas que mostrará el círculo de vida")]
    public Image imagenVidas;
    [Tooltip("Arrastra aquí los 8 sprites del círculo (de 1 vida hasta 8 vidas). El índice 0 debe ser 1 vida, el índice 7 debe ser 8 vidas.")]
    public Sprite[] spritesVidas;

    [Header("Arma y Munición")]
    [Tooltip("Imagen que mostrará el icono del arma (pistola o cuerpo a cuerpo)")]
    public Image imagenIconoArma;
    [Tooltip("Imagen del círculo que bordea al arma (las balas o el círculo normal)")]
    public Image imagenCirculoArma;
    
    [Tooltip("Sprite del icono cuando el jugador lleva los puños/cuerpo a cuerpo")]
    public Sprite iconoArmaCuerpoACuerpo;
    [Tooltip("Sprite del icono cuando el jugador lleva la pistola")]
    public Sprite iconoPistola;
    [Tooltip("Sprite del círculo base cuando lleva el arma cuerpo a cuerpo")]
    public Sprite spriteCirculoArmaMelee;
    [Tooltip("Array de sprites del círculo de balas. Índice 0 = 0 balas, Índice 1 = 1 bala, etc.")]
    public Sprite[] spritesBalas;

    [Header("Objeto de Curación")]
    [Tooltip("Imagen que mostrará la cura o la cruz de vacío")]
    public Image imagenIconoCura;
    [Tooltip("Sprite de la cruz que se muestra cuando no hay cura")]
    public Sprite iconoCuraVacia;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Actualiza la barra de vida usando el arreglo de sprites.
    /// Asume que vidaActual va de 0 a 8.
    /// </summary>
    public void ActualizarVida(int vidaActual)
    {
        if (imagenVidas == null || spritesVidas == null || spritesVidas.Length == 0) return;

        // Limitar la vida para no salirnos de los límites del arreglo
        int indice = Mathf.Clamp(vidaActual - 1, 0, spritesVidas.Length - 1);

        if (vidaActual <= 0)
        {
            imagenVidas.enabled = false;
        }
        else
        {
            imagenVidas.enabled = true;
            imagenVidas.sprite = spritesVidas[indice];
        }
    }

    /// <summary>
    /// Actualiza el icono del arma y el círculo de alrededor (balas o círculo melee).
    /// </summary>
    public void ActualizarArma(bool tieneArma, int balasActuales, int balasMaximas)
    {
        if (tieneArma)
        {
            // Ponemos la pistola
            if (imagenIconoArma != null && iconoPistola != null) 
                imagenIconoArma.sprite = iconoPistola;
            
            // Ponemos el sprite del círculo correspondiente a las balas actuales
            if (imagenCirculoArma != null && spritesBalas != null && spritesBalas.Length > 0)
            {
                int indice = Mathf.Clamp(balasActuales, 0, spritesBalas.Length - 1);
                imagenCirculoArma.sprite = spritesBalas[indice];
            }
        }
        else
        {
            // Ponemos el cuerpo a cuerpo
            if (imagenIconoArma != null && iconoArmaCuerpoACuerpo != null) 
                imagenIconoArma.sprite = iconoArmaCuerpoACuerpo;
            
            // Ponemos el círculo estándar
            if (imagenCirculoArma != null && spriteCirculoArmaMelee != null) 
                imagenCirculoArma.sprite = spriteCirculoArmaMelee;
        }
    }

    /// <summary>
    /// Actualiza el icono del objeto de curación o muestra una cruz vacía.
    /// </summary>
    public void ActualizarCura(bool tieneCura, Sprite iconoCura)
    {
        if (imagenIconoCura != null)
        {
            if (tieneCura && iconoCura != null)
            {
                imagenIconoCura.sprite = iconoCura;
            }
            else if (!tieneCura && iconoCuraVacia != null)
            {
                imagenIconoCura.sprite = iconoCuraVacia;
            }
        }
    }
}
