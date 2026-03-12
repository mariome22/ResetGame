using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    [Header("Estadisticas")]
    public int vidaMaxima = 3;
    private int vidaActual;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal; // 1. Variable para memorizar el color

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. Guardamos el color
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
    }

    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;

        StartCoroutine(EfectoDano());

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private IEnumerator EfectoDano()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f); // Un tono rojizo/blanco de impacto
            yield return new WaitForSeconds(0.1f);

            // 3. Restauramos su color original
            spriteRenderer.color = colorOriginal;
        }
    }

    private void Morir()
    {
        Destroy(gameObject);
    }
}