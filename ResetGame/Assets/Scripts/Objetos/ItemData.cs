using UnityEngine;

[CreateAssetMenu(fileName = "NuevoObjeto", menuName = "Inventario/Objeto")]
public class ItemData : ScriptableObject
{
    [Header("Datos Básicos")]
    public string nombreObjeto;
    [TextArea]
    public string descripcion;
    public Sprite iconoObjeto;
    [Tooltip("El prefab físico que se instanciará al soltar el objeto del inventario")]
    public GameObject prefabMundo;

    [Header("Clasificación")]
    public TipoObjeto tipo;
    public bool esAcumulable;
    public int cantidadMaxima = 99;

    [Header("Efectos (Según el Tipo)")]
    public int valorEfecto;

    [TextArea(5, 10)]
    [Tooltip("Solo rellena esto si el objeto es de tipo Documento")]
    public string contenidoDocumento;

    public enum TipoObjeto
    {
        Curacion,
        RecursoCrafteo,
        Municion,
        ArmaCuerpoCuerpo,
        Clave,
        Documento,
        ArmaADistancia
    }
}
