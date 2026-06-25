using UnityEngine;

public class ScreenJitter2D : MonoBehaviour
{
    [Header("Sincronización")]
    [Tooltip("Arrastra aquí el objeto con el script FlickerLight2D para que vibre sincronizado con la luz")]
    public FlickerLight2D flickerLight;

    [Tooltip("Si está activo, la pantalla vibrará constantemente sin necesidad de estar sincronizada con la luz")]
    public bool jitterConstante = false;

    [Header("Ajustes de Vibración")]
    [Tooltip("Rango máximo de movimiento horizontal (eje X)")]
    public float rangoJitterX = 0.05f;
    [Tooltip("Rango máximo de movimiento vertical (eje Y)")]
    public float rangoJitterY = 0.02f;

    [Tooltip("Frecuencia de actualización de la vibración (segundos)")]
    public float velocidadJitter = 0.05f;

    private Vector3 posicionOriginal;
    private float siguienteCambio = 0f;

    private void Start()
    {
        posicionOriginal = transform.localPosition;
    }

    private void Update()
    {
        // Determinar si debemos vibrar en este momento
        bool debeVibrar = jitterConstante || (flickerLight != null && flickerLight.IsGlitching);

        if (debeVibrar)
        {
            if (Time.time >= siguienteCambio)
            {
                siguienteCambio = Time.time + velocidadJitter;

                // Aplicar un desplazamiento aleatorio
                Vector3 desplazamiento = new Vector3(
                    Random.Range(-rangoJitterX, rangoJitterX),
                    Random.Range(-rangoJitterY, rangoJitterY),
                    0f
                );
                transform.localPosition = posicionOriginal + desplazamiento;
            }
        }
        else
        {
            // Devolver a la posición original si no hay glitch
            if (transform.localPosition != posicionOriginal)
            {
                transform.localPosition = posicionOriginal;
            }
        }
    }
}
