using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float fuerza = 1f)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(fuerza);
        }
    }
}