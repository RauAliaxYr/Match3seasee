using System.Collections;
using System.Collections.Generic;
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
        
        // Subscribe to scene load events to ensure settings persist
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
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
            Debug.Log("AudioManager: Created new AudioManager instance");
        }
        else
        {
            Debug.Log("AudioManager: Instance already exists, skipping creation");
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
        
        // Apply settings to all audio sources in the scene
        ApplyVolumeSettingsToAllAudioSources();
        
        Debug.Log($"AudioManager: Loaded settings - Music: {musicVolume}, SFX: {sfxVolume}");
    }
    
    /// <summary>
    /// Called when a new scene is loaded to ensure audio settings persist
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Wait a frame to ensure the scene is fully loaded
        StartCoroutine(ApplySettingsAfterSceneLoad());
    }
    
    /// <summary>
    /// Applies audio settings after scene load with a small delay
    /// </summary>
    private System.Collections.IEnumerator ApplySettingsAfterSceneLoad()
    {
        yield return null; // Wait one frame
        
        // Check if we need to reload settings from PlayerPrefs
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        // Only reload if settings have changed or are different from current
        if (Mathf.Abs(savedMusicVolume - musicVolume) > 0.001f || Mathf.Abs(savedSFXVolume - sfxVolume) > 0.001f)
        {
            Debug.Log($"AudioManager: Settings changed, reloading from PlayerPrefs. Current: M={musicVolume}, S={sfxVolume}, Saved: M={savedMusicVolume}, S={savedSFXVolume}");
            LoadSettings();
        }
        else
        {
            Debug.Log("AudioManager: Settings unchanged, just reapplying to scene sources");
            // Reapply volume settings to ensure they persist across scenes
            ApplyVolumeSettingsToAllAudioSources();
        }
        
        Debug.Log("AudioManager: Reapplied audio settings after scene load");
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
        
        // Apply new music volume to all music sources in the scene
        ApplyMusicVolumeToAllSources();
        
        SaveSettings();
        Debug.Log($"AudioManager: Music volume set to {musicVolume}");
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        
        // Apply new SFX volume to all SFX sources in the scene
        ApplySFXVolumeToAllSources();
        
        SaveSettings();
        Debug.Log($"AudioManager: SFX volume set to {sfxVolume}");
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
    
    /// <summary>
    /// Applies current volume settings to all AudioSource components in the scene
    /// </summary>
    public void ApplyVolumeSettingsToAllAudioSources()
    {
        // Get all AudioSource components in the scene
        AudioSource[] allAudioSources = GetComponentsInChildren<AudioSource>();
        
        // Also check for AudioSource components in the current GameObject
        AudioSource[] currentAudioSources = GetComponents<AudioSource>();
        
        // Combine all sources
        List<AudioSource> allSources = new List<AudioSource>();
        allSources.AddRange(allAudioSources);
        allSources.AddRange(currentAudioSources);
        
        foreach (AudioSource audioSource in allSources)
        {
            if (audioSource == musicSource || audioSource == sfxSource)
            {
                // Skip our main sources as they're already set
                continue;
            }
            
            // Determine if this is music or SFX based on common naming patterns
            string sourceName = audioSource.name.ToLower();
            bool isMusic = sourceName.Contains("music") || sourceName.Contains("theme") || sourceName.Contains("background");
            bool isSFX = sourceName.Contains("sfx") || sourceName.Contains("effect") || sourceName.Contains("sound");
            
            if (isMusic)
            {
                audioSource.volume = musicVolume;
                Debug.Log($"AudioManager: Applied music volume {musicVolume} to {audioSource.name}");
            }
            else if (isSFX)
            {
                audioSource.volume = sfxVolume;
                Debug.Log($"AudioManager: Applied SFX volume {sfxVolume} to {audioSource.name}");
            }
            else
            {
                // Default to SFX volume for unknown sources
                audioSource.volume = sfxVolume;
                Debug.Log($"AudioManager: Applied default SFX volume {sfxVolume} to {audioSource.name}");
            }
        }
        
        Debug.Log($"AudioManager: Applied volume settings to {allSources.Count} audio sources");
    }
    
    /// <summary>
    /// Applies current music volume to all music sources in the scene
    /// </summary>
    private void ApplyMusicVolumeToAllSources()
    {
        // Get all AudioSource components in the scene
        AudioSource[] allAudioSources = GetComponentsInChildren<AudioSource>();
        
        // Also check for AudioSource components in the current GameObject
        AudioSource[] currentAudioSources = GetComponents<AudioSource>();
        
        // Combine all sources
        List<AudioSource> allSources = new List<AudioSource>();
        allSources.AddRange(allAudioSources);
        allSources.AddRange(currentAudioSources);
        
        foreach (AudioSource audioSource in allSources)
        {
            if (audioSource == musicSource) continue; // Skip our main source
            
            string sourceName = audioSource.name.ToLower();
            bool isMusic = sourceName.Contains("music") || sourceName.Contains("theme") || sourceName.Contains("background");
            
            if (isMusic)
            {
                audioSource.volume = musicVolume;
                Debug.Log($"AudioManager: Applied music volume {musicVolume} to {audioSource.name}");
            }
        }
    }
    
    /// <summary>
    /// Applies current SFX volume to all SFX sources in the scene
    /// </summary>
    private void ApplySFXVolumeToAllSources()
    {
        // Get all AudioSource components in the scene
        AudioSource[] allAudioSources = GetComponentsInChildren<AudioSource>();
        
        // Also check for AudioSource components in the current GameObject
        AudioSource[] currentAudioSources = GetComponents<AudioSource>();
        
        // Combine all sources
        List<AudioSource> allSources = new List<AudioSource>();
        allSources.AddRange(allAudioSources);
        allSources.AddRange(currentAudioSources);
        
        foreach (AudioSource audioSource in allSources)
        {
            if (audioSource == sfxSource) continue; // Skip our main source
            
            string sourceName = audioSource.name.ToLower();
            bool isSFX = sourceName.Contains("sfx") || sourceName.Contains("effect") || sourceName.Contains("sound");
            
            if (isSFX)
            {
                audioSource.volume = sfxVolume;
                Debug.Log($"AudioManager: Applied SFX volume {sfxVolume} to {audioSource.name}");
            }
        }
    }
    
    /// <summary>
    /// Forces a reload of all settings from PlayerPrefs
    /// </summary>
    public void ForceReloadSettings()
    {
        LoadSettings();
        Debug.Log("AudioManager: Forced reload of all settings");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from scene load events
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion
} 