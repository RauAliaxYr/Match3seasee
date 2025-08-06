using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AdaptiveBackground : MonoBehaviour
{
    [Header("Background Sprites")]
    [SerializeField] private Sprite mobileBackground;
    [SerializeField] private Sprite desktopBackground;
    [SerializeField] private Sprite webGLBackground;
    
    [Header("Background Settings")]
    [SerializeField] private bool useAspectRatioScaling = true;
    [SerializeField] private bool maintainAspectRatio = true;
    [SerializeField] private ImageType imageType = ImageType.Simple;
    
    [Header("Fallback")]
    [SerializeField] private Sprite fallbackBackground;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onBackgroundChanged;
    
    // Components
    private Image uiImage;
    private SpriteRenderer spriteRenderer;
    private RectTransform rectTransform;
    
    // Current state
    private Sprite currentBackground;
    private bool isInitialized = false;
    
    public enum ImageType
    {
        Simple,
        Sliced,
        Tiled
    }
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void Start()
    {
        ApplyBackground();
        isInitialized = true;
    }
    
    private void InitializeComponents()
    {
        // Get UI Image component
        uiImage = GetComponent<Image>();
        
        // Get SpriteRenderer component (for 2D backgrounds)
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Get RectTransform for UI scaling
        rectTransform = GetComponent<RectTransform>();
        
        // Validate components
        if (uiImage == null && spriteRenderer == null)
        {
            Debug.LogWarning($"AdaptiveBackground on {gameObject.name}: No Image or SpriteRenderer component found!");
        }
    }
    
    public void ApplyBackground()
    {
        Sprite selectedBackground = GetBackgroundForPlatform();
        
        if (selectedBackground != null)
        {
            ApplySprite(selectedBackground);
            ApplyImageSettings();
            
            if (currentBackground != selectedBackground)
            {
                currentBackground = selectedBackground;
                onBackgroundChanged?.Invoke();
                
                Debug.Log($"Background changed to: {selectedBackground.name} for platform: {GetCurrentPlatform()}");
            }
        }
        else
        {
            Debug.LogWarning($"No background sprite assigned for platform: {GetCurrentPlatform()}");
        }
    }
    
    private Sprite GetBackgroundForPlatform()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            return webGLBackground != null ? webGLBackground : GetFallbackBackground();
        #elif UNITY_ANDROID || UNITY_IOS
            return mobileBackground != null ? mobileBackground : GetFallbackBackground();
        #else
            return desktopBackground != null ? desktopBackground : GetFallbackBackground();
        #endif
    }
    
    private Sprite GetFallbackBackground()
    {
        // Try to get any available background as fallback
        if (fallbackBackground != null) return fallbackBackground;
        if (desktopBackground != null) return desktopBackground;
        if (mobileBackground != null) return mobileBackground;
        if (webGLBackground != null) return webGLBackground;
        
        return null;
    }
    
    private void ApplySprite(Sprite sprite)
    {
        if (uiImage != null)
        {
            uiImage.sprite = sprite;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
    
    private void ApplyImageSettings()
    {
        if (uiImage != null)
        {
            uiImage.type = (Image.Type)imageType;
            
            if (useAspectRatioScaling && maintainAspectRatio)
            {
                uiImage.preserveAspect = true;
            }
        }
    }
    
    private string GetCurrentPlatform()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            return "WebGL";
        #elif UNITY_ANDROID
            return "Android";
        #elif UNITY_IOS
            return "iOS";
        #else
            return "Desktop";
        #endif
    }
    
    // Public methods for runtime control
    public void SetMobileBackground(Sprite sprite)
    {
        mobileBackground = sprite;
        if (isInitialized) ApplyBackground();
    }
    
    public void SetDesktopBackground(Sprite sprite)
    {
        desktopBackground = sprite;
        if (isInitialized) ApplyBackground();
    }
    
    public void SetWebGLBackground(Sprite sprite)
    {
        webGLBackground = sprite;
        if (isInitialized) ApplyBackground();
    }
    
    public void SetFallbackBackground(Sprite sprite)
    {
        fallbackBackground = sprite;
        if (isInitialized) ApplyBackground();
    }
    
    public Sprite GetCurrentBackground()
    {
        return currentBackground;
    }
    
    public void RefreshBackground()
    {
        ApplyBackground();
    }
    
    // Method to force specific background (for testing)
    public void ForceBackground(Sprite sprite)
    {
        ApplySprite(sprite);
        currentBackground = sprite;
        onBackgroundChanged?.Invoke();
    }
    
    // Method to restore platform-specific background
    public void RestorePlatformBackground()
    {
        ApplyBackground();
    }
} 