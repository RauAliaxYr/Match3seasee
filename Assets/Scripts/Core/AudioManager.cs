using System.Collections;
using UnityEngine;

/// <summary>
/// Manages all sounds in the game
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuTheme;
    [SerializeField] private AudioClip gameplayTheme;
    [SerializeField] private AudioClip victoryTheme;
    [SerializeField] private AudioClip loseTheme;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip tileSelect;
    [SerializeField] private AudioClip tileSwap;
    [SerializeField] private AudioClip match3;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool musicEnabled = true;
    [SerializeField] private bool sfxEnabled = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create AudioSource components if they don't exist
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        LoadSettings();
    }

    /// <summary>
    /// Creates AudioManager in the scene if it doesn't exist
    /// </summary>
    public static void CreateAudioManagerIfNeeded()
    {
        if (Instance == null)
        {
            GameObject audioManagerGO = new GameObject("AudioManager");
            audioManagerGO.AddComponent<AudioManager>();
        }
    }

    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", sfxEnabled ? 1 : 0);
    }

    #region Music Methods
    public void PlayMainMenuTheme()
    {
        PlayMusic(mainMenuTheme);
    }

    public void PlayGameplayTheme()
    {
        PlayMusic(gameplayTheme);
    }

    public void PlayVictoryTheme()
    {
        PlayMusic(victoryTheme, false); // Play once, no loop
    }

    public void PlayLoseTheme()
    {
        PlayMusic(loseTheme, false); // Play once, no loop
    }

    private void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!musicEnabled || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // Оставить старую PlayMusic для обратной совместимости
    private void PlayMusic(AudioClip clip)
    {
        PlayMusic(clip, true);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void PlayMainMenuThemeIfNotPlaying()
    {
        PlayMusicIfNotPlaying(mainMenuTheme);
    }

    private void PlayMusicIfNotPlaying(AudioClip clip)
    {
        if (!musicEnabled || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        PlayMusic(clip);
    }
    #endregion

    #region SFX Methods
    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayTileSelect()
    {
        PlaySFX(tileSelect);
    }

    public void PlayTileSwap()
    {
        PlaySFX(tileSwap);
    }

    public void PlayMatch3()
    {
        PlaySFX(match3);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (!sfxEnabled || clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    #endregion

    #region Settings Methods
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        SaveSettings();
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;
        if (!enabled)
        {
            StopMusic();
        }
        SaveSettings();
    }

    public void SetSFXEnabled(bool enabled)
    {
        sfxEnabled = enabled;
        SaveSettings();
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMusicEnabled() => musicEnabled;
    public bool IsSFXEnabled() => sfxEnabled;
    #endregion
} 