using UnityEngine;

public class LightTrigger : MonoBehaviour
{
    [Header("Ajustes de Luz")]
    [Tooltip("El objeto luminoso (GameObject) que se activará al entrar en la casilla.")]
    public GameObject luzIndicadora;

    [Tooltip("Si marcamos esta casilla, la luz se apagará automáticamente cuando el jugador salga.")]
    public bool apagarAlSalir = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (luzIndicadora != null)
            {
                luzIndicadora.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && apagarAlSalir)
        {
            if (luzIndicadora != null)
            {
                luzIndicadora.SetActive(false);
            }
        }
    }
}
