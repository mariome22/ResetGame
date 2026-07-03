using UnityEngine;

public class PlaySceneMusic : MonoBehaviour
{
    [Header("Música de la Escena")]
    [Tooltip("Arrastra aquí el AudioClip que quieras que suene en esta escena en bucle")]
    [SerializeField] private AudioClip musicaEscena;

    public AudioClip MusicaEscena => musicaEscena;

    private void Start()
    {
        Play();
    }

    public void Play()
    {
        if (musicaEscena == null || AudioManager.Instance == null) return;

        // Si hay una transición activa (pantalla en negro), esperamos a que termine para reproducir
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitionActive)
        {
            StartCoroutine(WaitAndPlayRoutine());
        }
        else
        {
            AudioManager.Instance.PlayMusic(musicaEscena);
            Debug.Log($"[PlaySceneMusic] Reproduciendo música de escena: {musicaEscena.name}");
        }
    }

    private System.Collections.IEnumerator WaitAndPlayRoutine()
    {
        while (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitionActive)
        {
            yield return null;
        }
        AudioManager.Instance.PlayMusic(musicaEscena);
        Debug.Log($"[PlaySceneMusic] Reproduciendo música de escena (tras transición): {musicaEscena.name}");
    }
}
