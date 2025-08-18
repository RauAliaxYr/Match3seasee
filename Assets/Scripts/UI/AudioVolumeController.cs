using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class AudioVolumeController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject volumePanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button musicIconButton;
    [SerializeField] private Button sfxIconButton;
    
    [Header("Settings")]
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.3f;
    
    [Header("Icons")]
    [SerializeField] private Sprite musicIcon;
    [SerializeField] private Sprite musicMutedIcon;
    [SerializeField] private Sprite sfxIcon;
    [SerializeField] private Sprite sfxMutedIcon;
    
    private CanvasGroup volumePanelCanvasGroup;
    private bool isVolumePanelVisible = false;
    private Coroutine hideVolumePanelCoroutine;
    
    private void Start()
    {
        // Ensure this GameObject is active
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("AudioVolumeController: GameObject is inactive! Activating...");
            gameObject.SetActive(true);
        }
        
        InitializeUI();
        LoadCurrentSettings();
        SetupEventListeners();
        
        // Ensure panel is active and visible (icons must be visible at start)
        if (volumePanelCanvasGroup != null)
        {
            volumePanelCanvasGroup.alpha = 1f;
            volumePanelCanvasGroup.interactable = true;
            volumePanelCanvasGroup.blocksRaycasts = true;
        }
        
        // Hide sliders initially
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.gameObject.SetActive(false);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.gameObject.SetActive(false);
        }
        
        // Setup hover events for icons
        SetupHoverEvents();
        
        Debug.Log("AudioVolumeController: Initialized successfully");
    }
    
    private void InitializeUI()
    {
        // Get or create CanvasGroup for volume panel
        if (volumePanel != null)
        {
            volumePanelCanvasGroup = volumePanel.GetComponent<CanvasGroup>();
            if (volumePanelCanvasGroup == null)
            {
                volumePanelCanvasGroup = volumePanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Setup sliders with proper ranges
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            // Load saved music volume
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            musicVolumeSlider.value = savedMusicVolume;
            Debug.Log($"AudioVolumeController: Loaded saved music volume: {savedMusicVolume}");
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            // Load saved SFX volume
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.value = savedSFXVolume;
            Debug.Log($"AudioVolumeController: Loaded saved SFX volume: {savedSFXVolume}");
        }
    }
    
    private void LoadCurrentSettings()
    {
        if (AudioManager.Instance == null) return;
        
        // Update slider values from AudioManager (which loads from PlayerPrefs)
        if (musicVolumeSlider != null)
        {
            float currentMusicVolume = AudioManager.Instance.GetMusicVolume();
            musicVolumeSlider.value = currentMusicVolume;
            Debug.Log($"AudioVolumeController: Synced music slider with AudioManager: {currentMusicVolume}");
        }
        
        if (sfxVolumeSlider != null)
        {
            float currentSFXVolume = AudioManager.Instance.GetSFXVolume();
            sfxVolumeSlider.value = currentSFXVolume;
            Debug.Log($"AudioVolumeController: Synced SFX slider with AudioManager: {currentSFXVolume}");
        }
        
        // Update icon states
        UpdateIconStates();
    }
    
    private void SetupEventListeners()
    {
        // Music icon button
        if (musicIconButton != null)
        {
            musicIconButton.onClick.AddListener(OnMusicIconClicked);
        }
        
        // SFX icon button
        if (sfxIconButton != null)
        {
            sfxIconButton.onClick.AddListener(OnSFXIconClicked);
        }
        
        // Volume sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }
    
    private void SetupHoverEvents()
    {
        // Add hover events for music icon
        if (musicIconButton != null)
        {
            var musicEventTrigger = musicIconButton.GetComponent<EventTrigger>();
            if (musicEventTrigger == null)
            {
                musicEventTrigger = musicIconButton.gameObject.AddComponent<EventTrigger>();
            }
            
            // OnPointerEnter - show music slider
            var enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowMusicSlider(); });
            musicEventTrigger.triggers.Add(enterEntry);
            
            // OnPointerExit - hide music slider with delay
            var exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { 
                musicSliderHideCoroutine = StartCoroutine(HideMusicSliderWithDelay()); 
            });
            musicEventTrigger.triggers.Add(exitEntry);
        }
        
        // Add hover events for SFX icon
        if (sfxIconButton != null)
        {
            var sfxEventTrigger = sfxIconButton.GetComponent<EventTrigger>();
            if (sfxEventTrigger == null)
            {
                sfxEventTrigger = sfxIconButton.gameObject.AddComponent<EventTrigger>();
            }
            
            // OnPointerEnter - show SFX slider
            var enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowSFXSlider(); });
            sfxEventTrigger.triggers.Add(enterEntry);
            
            // OnPointerExit - hide SFX slider with delay
            var exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { 
                sfxSliderHideCoroutine = StartCoroutine(HideSFXSliderWithDelay()); 
            });
            sfxEventTrigger.triggers.Add(exitEntry);
        }
        
        // Add hover events for sliders to prevent hiding
        SetupSliderHoverEvents();
    }
    
    private void SetupSliderHoverEvents()
    {
        // Music slider hover events
        if (musicVolumeSlider != null)
        {
            var musicSliderEventTrigger = musicVolumeSlider.GetComponent<EventTrigger>();
            if (musicSliderEventTrigger == null)
            {
                musicSliderEventTrigger = musicVolumeSlider.gameObject.AddComponent<EventTrigger>();
            }
            
            // OnPointerEnter - cancel hide coroutine
            var enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { CancelMusicSliderHide(); });
            musicSliderEventTrigger.triggers.Add(enterEntry);
            
            // OnPointerExit - start hide coroutine
            var exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { 
                musicSliderHideCoroutine = StartCoroutine(HideMusicSliderWithDelay()); 
            });
            musicSliderEventTrigger.triggers.Add(exitEntry);
        }
        
        // SFX slider hover events
        if (sfxVolumeSlider != null)
        {
            var sfxSliderEventTrigger = sfxVolumeSlider.GetComponent<EventTrigger>();
            if (sfxSliderEventTrigger == null)
            {
                sfxSliderEventTrigger = sfxVolumeSlider.gameObject.AddComponent<EventTrigger>();
            }
            
            // OnPointerEnter - cancel hide coroutine
            var enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { CancelSFXSliderHide(); });
            sfxSliderEventTrigger.triggers.Add(enterEntry);
            
            // OnPointerExit - start hide coroutine
            var exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { 
                sfxSliderHideCoroutine = StartCoroutine(HideSFXSliderWithDelay()); 
            });
            sfxSliderEventTrigger.triggers.Add(exitEntry);
        }
    }
    
    public void OnMusicIconClicked()
    {
        // Music button should only show slider, not control music playback
        // Music should always play, only volume can be adjusted
        Debug.Log("Music icon clicked - showing volume slider");
    }
    
    public void OnSFXIconClicked()
    {
        // SFX button should only show slider, not control SFX playback
        // SFX should always play, only volume can be adjusted
        Debug.Log("SFX icon clicked - showing volume slider");
    }
    
    // Methods for hover events
    private void ShowMusicSlider()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.gameObject.SetActive(true);
            CancelMusicSliderHide();
        }
    }
    
    private void HideMusicSlider()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.gameObject.SetActive(false);
        }
    }
    
    private Coroutine musicSliderHideCoroutine;
    
    private IEnumerator HideMusicSliderWithDelay()
    {
        yield return new WaitForSeconds(2f);
        HideMusicSlider();
    }
    
    private void CancelMusicSliderHide()
    {
        if (musicSliderHideCoroutine != null)
        {
            StopCoroutine(musicSliderHideCoroutine);
            musicSliderHideCoroutine = null;
        }
    }
    
    private void ShowSFXSlider()
    {
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.gameObject.SetActive(true);
            CancelSFXSliderHide();
        }
    }
    
    private void HideSFXSlider()
    {
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.gameObject.SetActive(false);
        }
    }
    
    private Coroutine sfxSliderHideCoroutine;
    
    private IEnumerator HideSFXSliderWithDelay()
    {
        yield return new WaitForSeconds(2f);
        HideSFXSlider();
    }
    
    private void CancelSFXSliderHide()
    {
        if (sfxSliderHideCoroutine != null)
        {
            StopCoroutine(sfxSliderHideCoroutine);
            sfxSliderHideCoroutine = null;
        }
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance == null) return;
        
        // Set volume in AudioManager (this automatically saves to PlayerPrefs)
        AudioManager.Instance.SetMusicVolume(value);
        
        // Log the change for debugging
        Debug.Log($"AudioVolumeController: Music volume changed to {value} and saved to PlayerPrefs");
        
        // Icons don't change anymore, so no need to update them
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance == null) return;
        
        // Set volume in AudioManager (this automatically saves to PlayerPrefs)
        AudioManager.Instance.SetSFXVolume(value);
        
        // Log the change for debugging
        Debug.Log($"AudioVolumeController: SFX volume changed to {value} and saved to PlayerPrefs");
        
        // Icons don't change anymore, so no need to update them
    }
    
    private void UpdateIconStates()
    {
        if (AudioManager.Instance == null) return;
        
        // Icons are now static - they don't change based on mute state
        // Music and SFX always play, only volume can be adjusted
        if (musicIconButton != null)
        {
            Image musicIconImage = musicIconButton.GetComponent<Image>();
            if (musicIconImage != null && musicIcon != null)
            {
                musicIconImage.sprite = musicIcon; // Always show normal music icon
            }
        }
        
        if (sfxIconButton != null)
        {
            Image sfxIconImage = sfxIconButton.GetComponent<Image>();
            if (sfxIconImage != null && sfxIcon != null)
            {
                sfxIconImage.sprite = sfxIcon; // Always show normal SFX icon
            }
        }
    }
    
    private void ShowVolumePanel()
    {
        if (volumePanel == null) return;
        
        // Cancel previous hide coroutine
        if (hideVolumePanelCoroutine != null)
        {
            StopCoroutine(hideVolumePanelCoroutine);
        }
        
        // Show sliders (panel stays active always)
        isVolumePanelVisible = true;
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.gameObject.SetActive(true);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.gameObject.SetActive(true);
        }

        // Auto-hide after delay
        hideVolumePanelCoroutine = StartCoroutine(HideVolumePanelAfterDelay());
    }
    
    private IEnumerator HideVolumePanelAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        HideVolumePanel();
    }
    
    public void HideVolumePanel()
    {
        if (volumePanel == null || !isVolumePanelVisible) return;
        
        // Hide sliders (keep panel/icons visible)
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.gameObject.SetActive(false);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.gameObject.SetActive(false);
        }
        isVolumePanelVisible = false;
    }
    
    private IEnumerator FadeVolumePanel(float fromAlpha, float toAlpha, float duration, System.Action onComplete = null)
    {
        if (volumePanelCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            volumePanelCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            
            yield return null;
        }
        
        volumePanelCanvasGroup.alpha = toAlpha;
        onComplete?.Invoke();
    }
    
    // Public methods for external control
    public void ShowVolumePanelTemporarily()
    {
        ShowVolumePanel();
    }
    
    public void ForceHideVolumePanel()
    {
        if (hideVolumePanelCoroutine != null)
        {
            StopCoroutine(hideVolumePanelCoroutine);
        }
        HideVolumePanel();
    }
    
    // Method to refresh UI when settings change externally
    public void RefreshUI()
    {
        LoadCurrentSettings();
        Debug.Log("AudioVolumeController: UI refreshed from external source");
    }
    
    // Method to force reload settings from PlayerPrefs
    public void ForceReloadSettings()
    {
        if (AudioManager.Instance == null) return;
        
        // Force AudioManager to reload settings
        // This will update all audio sources with saved volumes
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        AudioManager.Instance.SetMusicVolume(savedMusicVolume);
        AudioManager.Instance.SetSFXVolume(savedSFXVolume);
        
        // Update UI sliders
        LoadCurrentSettings();
        
        Debug.Log($"AudioVolumeController: Forced reload of settings - Music: {savedMusicVolume}, SFX: {savedSFXVolume}");
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (musicIconButton != null)
        {
            musicIconButton.onClick.RemoveListener(OnMusicIconClicked);
        }
        
        if (sfxIconButton != null)
        {
            sfxIconButton.onClick.RemoveListener(OnSFXIconClicked);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
        
        // Clean up coroutines
        if (musicSliderHideCoroutine != null)
        {
            StopCoroutine(musicSliderHideCoroutine);
        }
        if (sfxSliderHideCoroutine != null)
        {
            StopCoroutine(sfxSliderHideCoroutine);
        }
    }
} 