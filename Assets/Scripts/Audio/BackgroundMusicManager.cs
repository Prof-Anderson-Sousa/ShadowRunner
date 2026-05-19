using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    private const string ManagerName = "BackgroundMusicManager";
    private const string DefaultMusicResourcePath = "Audio/Music/shadow_runner_dark_race_loop";
    private static BackgroundMusicManager instance;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float initialVolume = 0.4f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool keepPlayingBetweenScenes = true;

    private AudioSource audioSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureMusicManager()
    {
        if (instance != null)
            return;

        BackgroundMusicManager existingManager = FindFirstObjectByType<BackgroundMusicManager>();

        if (existingManager != null)
        {
            instance = existingManager;
            instance.ConfigureAudioSource();
            return;
        }

        GameObject managerObject = new GameObject(ManagerName);
        instance = managerObject.AddComponent<BackgroundMusicManager>();
        instance.ConfigureAudioSource();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ConfigureAudioSource();

        if (keepPlayingBetweenScenes)
            DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // DIAGNOSTICO: lista todos os clips encontrados na pasta
        AudioClip[] allClips = Resources.LoadAll<AudioClip>("Audio/Music");
        Debug.Log($"[BGM] Clips encontrados em Resources/Audio/Music: {allClips.Length}");
        foreach (AudioClip c in allClips)
            Debug.Log($"[BGM] clip encontrado: '{c.name}'");

        if (playOnAwake)
            PlayMusic();
    }

    public void PlayMusic()
    {
        ConfigureAudioSource();

        if (audioSource.clip == null)
        {
            Debug.LogWarning("BackgroundMusicManager: nenhum AudioClip atribuido. Converta shadow_runner_dark_race_loop.mid para .ogg/.wav/.mp3 e atribua ao AudioSource ou coloque em Resources/Audio/Music.");
            return;
        }

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    public void PauseMusic()
    {
        if (audioSource != null)
            audioSource.Pause();
    }

    public void SetVolume(float value)
    {
        initialVolume = Mathf.Clamp01(value);

        if (audioSource != null)
            audioSource.volume = initialVolume;
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (musicClip == null)
        {
            musicClip = Resources.Load<AudioClip>(DefaultMusicResourcePath);

            if (musicClip == null)
                Debug.LogError($"[BGM] Resources.Load retornou NULL para: '{DefaultMusicResourcePath}'");
            else
                Debug.Log($"[BGM] Clip carregado: '{musicClip.name}' ({musicClip.length:F1}s)");
        }

        audioSource.clip = musicClip;
        audioSource.playOnAwake = playOnAwake;
        audioSource.loop = true;
        audioSource.volume = initialVolume;
        audioSource.priority = 128;
        audioSource.spatialBlend = 0f;
        audioSource.dopplerLevel = 0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }
}