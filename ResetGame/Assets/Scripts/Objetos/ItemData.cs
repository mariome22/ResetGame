using UnityEngine;

// Esto añade una opción al menú de clic derecho de Unity para crear tus objetos
[CreateAssetMenu(fileName = "NuevoObjeto", menuName = "Inventario/Objeto")]
public class ItemData : ScriptableObject
{
    [Header("Datos Básicos")]
    public string nombreObjeto;
    [TextArea]
    public string descripcion;
    public Sprite iconoObjeto; // La fotito que saldrá en el inventario

    [Header("Clasificación")]
    public TipoObjeto tipo;
    public bool esAcumulable; // Si puedes tener 5 vendas en un solo hueco
    public int cantidadMaxima = 99; // Límite si es acumulable

    [Header("Efectos (Según el Tipo)")]
    [Tooltip("Cantidad de vida que cura, munición que da, etc.")]
    public int valorEfecto;

    // Enum es una lista desplegable que te aparecerá en el Inspector
    public enum TipoObjeto
    {
        Curacion,       // Vendas, Botiquines
        RecursoCrafteo, // Alcohol, Trapos, Cinta
        Municion,       // Balas para el arma de fuego
        ArmaCuerpoCuerpo, // Palos, Tuberías rompibles
        Clave           // Llaves, Tarjetas
    }
}