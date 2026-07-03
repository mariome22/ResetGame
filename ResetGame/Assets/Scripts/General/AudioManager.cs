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
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Configurar latencia de audio al mínimo para respuestas instantáneas (Best Latency)
            AudioConfiguration config = AudioSettings.GetConfiguration();
            if (config.dspBufferSize > 256)
            {
                config.dspBufferSize = 256;
                AudioSettings.Reset(config);
                Debug.Log("[AudioManager] DSP Buffer ajustado a 256 para reducir la latencia al mínimo.");
            }

            InitializeAudioSources();
            LoadVolumeSettings();
        }
        else if (_instance != this)
        {
            // Destruimos solo este componente para no romper el GameObject padre (ej. Global_Managers)
            Destroy(this);
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
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }
}
