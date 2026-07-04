using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Global_Managers");
                    if (prefab != null)
                    {
                        GameObject instantiated = Instantiate(prefab);
                        _instance = instantiated.GetComponentInChildren<AudioManager>(true);
                        if (!instantiated.activeSelf) instantiated.SetActive(true);
                        DontDestroyOnLoad(instantiated);
                    }
                    else
                    {
                        GameObject obj = new GameObject("AudioManager");
                        _instance = obj.AddComponent<AudioManager>();
                        DontDestroyOnLoad(obj);
                    }
                }
                else
                {
                    // ¡Si encontramos una instancia preexistente en la escena pero está inactiva, la forzamos a activarse!
                    if (!_instance.gameObject.activeInHierarchy)
                    {
                        // Buscamos el objeto raíz (que probablemente sea Global_Managers) para activarlo completo
                        Transform rootParent = _instance.transform.root;
                        if (rootParent != null)
                        {
                            rootParent.gameObject.SetActive(true);
                        }
                        else
                        {
                            _instance.gameObject.SetActive(true);
                        }
                        Debug.Log("[AudioManager] Encontrada instancia inactiva de AudioManager en la escena. Forzada activación de su jerarquía.");
                    }
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float defaultSFXVolume = 0.7f;

    private float musicVolume;
    private float sfxVolume;

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // Si no se han asignado en el Inspector, crearlos automáticamente
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        Debug.Log($"[AudioManager] LoadVolumeSettings ejecutado. Música: {musicVolume}, SFX: {sfxVolume}");
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource == null) InitializeAudioSources(); // Inicialización perezosa de seguridad
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
        Debug.Log($"[AudioManager] SetMusicVolume llamado. Guardado en PlayerPrefs: {musicVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource == null) InitializeAudioSources(); // Inicialización perezosa de seguridad
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
        Debug.Log($"[AudioManager] SetSFXVolume llamado. Guardado en PlayerPrefs: {sfxVolume}");
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource == null) InitializeAudioSources(); // Inicialización perezosa de seguridad
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) InitializeAudioSources(); // Inicialización perezosa de seguridad
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxSource == null) InitializeAudioSources(); // Inicialización perezosa de seguridad
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;
        if (sfxSource == null) InitializeAudioSources(); // Inicialización perezosa de seguridad
        sfxSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }
}
