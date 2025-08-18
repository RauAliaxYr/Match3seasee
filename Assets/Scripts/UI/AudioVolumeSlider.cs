using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AudioVolumeSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Slider Settings")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI valueText;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color mutedColor = Color.gray;
    
    [Header("Audio Type")]
    [SerializeField] private bool isMusicSlider = true;
    
    private bool isDragging = false;
    private bool isHovering = false;
    private float originalValue;
    
    private void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
        
        SetupSlider();
        UpdateVisuals();
    }
    
    private void SetupSlider()
    {
        if (slider == null) return;
        
        // Set slider properties
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        
        // Set initial value based on audio type
        if (AudioManager.Instance != null)
        {
            if (isMusicSlider)
            {
                slider.value = AudioManager.Instance.GetMusicVolume();
            }
            else
            {
                slider.value = AudioManager.Instance.GetSFXVolume();
            }
        }
        
        // Add listener
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    private void OnSliderValueChanged(float value)
    {
        // Apply volume change
        if (AudioManager.Instance != null)
        {
            if (isMusicSlider)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
            else
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
        }
        
        // Update visuals
        UpdateVisuals();
        
        // Update text if available
        if (valueText != null)
        {
            valueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
    
    private void UpdateVisuals()
    {
        if (slider == null) return;
        
        bool isMuted = false;
        
        // Check if audio is muted
        if (AudioManager.Instance != null)
        {
            if (isMusicSlider)
            {
                isMuted = !AudioManager.Instance.IsMusicEnabled();
            }
            else
            {
                isMuted = !AudioManager.Instance.IsSFXEnabled();
            }
        }
        
        // Update fill image color
        if (fillImage != null)
        {
            if (isMuted)
            {
                fillImage.color = mutedColor;
            }
            else if (isDragging)
            {
                fillImage.color = activeColor;
            }
            else if (isHovering)
            {
                fillImage.color = hoverColor;
            }
            else
            {
                fillImage.color = normalColor;
            }
        }
        
        // Update background image color
        if (backgroundImage != null)
        {
            Color bgColor = isMuted ? mutedColor : normalColor;
            bgColor.a = 0.3f; // Semi-transparent
            backgroundImage.color = bgColor;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateVisuals();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        UpdateVisuals();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        originalValue = slider.value;
        UpdateVisuals();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        UpdateVisuals();
        
        // Check if value changed significantly
        if (Mathf.Abs(slider.value - originalValue) > 0.01f)
        {
            // Value changed, trigger any additional effects
            OnVolumeChanged();
        }
    }
    
    private void OnVolumeChanged()
    {
        // This method can be overridden or used for additional effects
        // For example, playing a sound when volume changes
        if (AudioManager.Instance != null && !isMusicSlider)
        {
            // Play a test sound for SFX slider
            AudioManager.Instance.PlayButtonClick();
        }
    }
    
    // Public methods for external control
    public void SetValue(float value)
    {
        if (slider != null)
        {
            slider.value = Mathf.Clamp01(value);
        }
    }
    
    public float GetValue()
    {
        return slider != null ? slider.value : 0f;
    }
    
    public void RefreshFromAudioManager()
    {
        if (AudioManager.Instance != null && slider != null)
        {
            if (isMusicSlider)
            {
                slider.value = AudioManager.Instance.GetMusicVolume();
            }
            else
            {
                slider.value = AudioManager.Instance.GetSFXVolume();
            }
        }
        
        UpdateVisuals();
    }
    
    public void SetMuted(bool muted)
    {
        if (slider != null)
        {
            slider.interactable = !muted;
        }
        
        UpdateVisuals();
    }
    
    // Method to handle external volume changes
    public void OnExternalVolumeChange()
    {
        RefreshFromAudioManager();
    }
    
    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
} 