using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    [Header("SFX")]
    public AudioClip buttonClickClip;
    public AudioClip itemCollectClip;
    public AudioClip portalOpenClip;
    public AudioClip botSoundClip;
    public AudioClip knockbackClip;
    public AudioClip tileWarningClip;
    public AudioClip tileCollapseClip;
    public AudioClip victoryClip;
    public AudioClip gameOverClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.35f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        ApplyVolume();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void EnsureSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolume();

        if (scene.name == "Menu")
        {
            PlayMusic(menuMusic);
        }
        else
        {
            PlayMusic(gameplayMusic);
        }
    }

    public void ApplyVolume()
    {
        float master = GameSettings.masterVolume;

        if (musicSource != null)
        {
            musicSource.volume = musicVolume * master;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * master;
        }

        AudioListener.volume = master;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
        {
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySfx(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, volumeMultiplier);
    }

    public void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * GameSettings.masterVolume * volumeMultiplier);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip, 0.7f);
    }
}
