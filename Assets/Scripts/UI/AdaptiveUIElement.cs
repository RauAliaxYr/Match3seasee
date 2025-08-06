using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class AdaptiveUIElement : MonoBehaviour
{
    [Header("Element Type")]
    [SerializeField] private UIElementType elementType = UIElementType.Button;
    
    // Public property to access element type
    public UIElementType ElementType
    {
        get { return elementType; }
        set { elementType = value; }
    }
    
    [Header("Dependencies")]
    [SerializeField] private AdaptiveUIManager adaptiveUIManager;
    [SerializeField] private AdaptiveUISystem adaptiveUISystem;
    
    [Header("Mobile Settings")]
    [SerializeField] private Vector2 mobileSize = new Vector2(100, 100);
    [SerializeField] private float mobileFontSize = 24f;
    [SerializeField] private Vector2 mobilePosition = Vector2.zero;
    [SerializeField] private float mobileScale = 1f;
    
    [Header("Desktop Settings")]
    [SerializeField] private Vector2 desktopSize = new Vector2(120, 120);
    [SerializeField] private float desktopFontSize = 28f;
    [SerializeField] private Vector2 desktopPosition = Vector2.zero;
    [SerializeField] private float desktopScale = 1f;
    
    [Header("WebGL Settings")]
    [SerializeField] private Vector2 webGLSize = new Vector2(110, 110);
    [SerializeField] private float webGLFontSize = 26f;
    [SerializeField] private Vector2 webGLPosition = Vector2.zero;
    [SerializeField] private float webGLScale = 1f;
    
    [Header("Orientation Settings")]
    [SerializeField] private bool adaptToOrientation = true;
    [SerializeField] private Vector2 portraitSize = new Vector2(100, 100);
    [SerializeField] private Vector2 landscapeSize = new Vector2(120, 80);
    
    [Header("Events")]
    [SerializeField] private UnityEvent onAdaptiveSettingsApplied;
    [SerializeField] private UnityEvent onOrientationChanged;
    
    private RectTransform rectTransform;
    private TextMeshProUGUI textComponent;
    private Image imageComponent;
    private Button buttonComponent;
    
    public enum UIElementType
    {
        Button,
        Text,
        Image,
        Panel,
        Custom
    }
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TextMeshProUGUI>();
        imageComponent = GetComponent<Image>();
        buttonComponent = GetComponent<Button>();
        
        // Try to find dependencies if not assigned
        if (adaptiveUIManager == null)
        {
            adaptiveUIManager = GetComponentInParent<AdaptiveUIManager>();
        }
        
        if (adaptiveUISystem == null)
        {
            adaptiveUISystem = AdaptiveUISystem.Instance;
        }
        
        // Subscribe to system events if available
        SubscribeToSystemEvents();
    }
    
    private void Start()
    {
        ApplyAdaptiveSettings();
    }
    
    private void OnEnable()
    {
        SubscribeToSystemEvents();
        
        if (adaptiveUIManager != null)
        {
            // Subscribe to orientation changes if needed
            InvokeRepeating(nameof(CheckForOrientationChange), 0.5f, 0.5f);
        }
    }
    
    private void OnDisable()
    {
        CancelInvoke(nameof(CheckForOrientationChange));
        UnsubscribeFromSystemEvents();
    }
    
    private void SubscribeToSystemEvents()
    {
        if (adaptiveUISystem != null)
        {
            // Subscribe to system events when they become available
            StartCoroutine(SubscribeWhenReady());
        }
    }
    
    private System.Collections.IEnumerator SubscribeWhenReady()
    {
        // Wait for system to be ready
        while (adaptiveUISystem == null)
        {
            adaptiveUISystem = AdaptiveUISystem.Instance;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Now subscribe to events
        // Note: We'll implement these events in AdaptiveUISystem
    }
    
    private void UnsubscribeFromSystemEvents()
    {
        // Unsubscribe from events when implemented
    }
    
    private void CheckForOrientationChange()
    {
        if (adaptToOrientation && adaptiveUIManager != null)
        {
            bool currentIsPortrait = Screen.height > Screen.width;
            bool previousIsPortrait = adaptiveUIManager.IsPortrait();
            
            if (currentIsPortrait != previousIsPortrait)
            {
                ApplyAdaptiveSettings();
                onOrientationChanged?.Invoke();
            }
        }
    }
    
    public void ApplyAdaptiveSettings()
    {
        if (adaptiveUIManager == null) return;
        
        Vector2 targetSize = GetTargetSize();
        float targetFontSize = GetTargetFontSize();
        Vector2 targetPosition = GetTargetPosition();
        float targetScale = GetTargetScale();
        
        // Apply size
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = targetSize;
            rectTransform.anchoredPosition = targetPosition;
            rectTransform.localScale = Vector3.one * targetScale;
        }
        
        // Apply font size
        if (textComponent != null)
        {
            textComponent.fontSize = targetFontSize;
        }
        
        // Apply specific element settings
        ApplyElementSpecificSettings();
        
        // Invoke event
        onAdaptiveSettingsApplied?.Invoke();
    }
    
    private Vector2 GetTargetSize()
    {
        if (adaptiveUIManager.IsMobileDevice())
        {
            return adaptToOrientation && adaptiveUIManager.IsPortrait() ? portraitSize : mobileSize;
        }
        else if (adaptiveUIManager.IsWebGLBuild())
        {
            return adaptToOrientation && adaptiveUIManager.IsPortrait() ? portraitSize : webGLSize;
        }
        else
        {
            return adaptToOrientation && adaptiveUIManager.IsPortrait() ? portraitSize : desktopSize;
        }
    }
    
    private float GetTargetFontSize()
    {
        if (adaptiveUIManager.IsMobileDevice())
        {
            return mobileFontSize;
        }
        else if (adaptiveUIManager.IsWebGLBuild())
        {
            return webGLFontSize;
        }
        else
        {
            return desktopFontSize;
        }
    }
    
    private Vector2 GetTargetPosition()
    {
        if (adaptiveUIManager.IsMobileDevice())
        {
            return mobilePosition;
        }
        else if (adaptiveUIManager.IsWebGLBuild())
        {
            return webGLPosition;
        }
        else
        {
            return desktopPosition;
        }
    }
    
    private float GetTargetScale()
    {
        if (adaptiveUIManager.IsMobileDevice())
        {
            return mobileScale;
        }
        else if (adaptiveUIManager.IsWebGLBuild())
        {
            return webGLScale;
        }
        else
        {
            return desktopScale;
        }
    }
    
    private void ApplyElementSpecificSettings()
    {
        switch (elementType)
        {
            case UIElementType.Button:
                ApplyButtonSettings();
                break;
            case UIElementType.Text:
                ApplyTextSettings();
                break;
            case UIElementType.Image:
                ApplyImageSettings();
                break;
            case UIElementType.Panel:
                ApplyPanelSettings();
                break;
        }
    }
    
    private void ApplyButtonSettings()
    {
        if (buttonComponent != null)
        {
            // Adjust button navigation for mobile
            if (adaptiveUIManager.IsMobileDevice())
            {
                buttonComponent.navigation = new Navigation
                {
                    mode = Navigation.Mode.None
                };
            }
            else
            {
                buttonComponent.navigation = new Navigation
                {
                    mode = Navigation.Mode.Automatic
                };
            }
        }
    }
    
    private void ApplyTextSettings()
    {
        if (textComponent != null)
        {
            // Adjust text alignment for different orientations
            if (adaptiveUIManager.IsPortrait())
            {
                textComponent.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                textComponent.alignment = TextAlignmentOptions.Left;
            }
        }
    }
    
    private void ApplyImageSettings()
    {
        if (imageComponent != null)
        {
            // Adjust image type based on device
            if (adaptiveUIManager.IsMobileDevice())
            {
                imageComponent.type = Image.Type.Simple;
            }
            else
            {
                imageComponent.type = Image.Type.Sliced;
            }
        }
    }
    
    private void ApplyPanelSettings()
    {
        // Panel-specific settings
        if (rectTransform != null)
        {
            // Adjust panel padding for different devices
            float padding = adaptiveUIManager.GetOptimalSpacing();
            
            // Apply padding to panel
            Vector2 currentSize = rectTransform.sizeDelta;
            rectTransform.sizeDelta = new Vector2(currentSize.x + padding * 2, currentSize.y + padding * 2);
        }
    }
    
    // Public methods for runtime adjustments
    public void SetMobileSize(Vector2 size)
    {
        mobileSize = size;
        ApplyAdaptiveSettings();
    }
    
    public void SetDesktopSize(Vector2 size)
    {
        desktopSize = size;
        ApplyAdaptiveSettings();
    }
    
    public void SetWebGLSize(Vector2 size)
    {
        webGLSize = size;
        ApplyAdaptiveSettings();
    }
    
    public void SetMobileFontSize(float fontSize)
    {
        mobileFontSize = fontSize;
        ApplyAdaptiveSettings();
    }
    
    public void SetDesktopFontSize(float fontSize)
    {
        desktopFontSize = fontSize;
        ApplyAdaptiveSettings();
    }
    
    public void SetWebGLFontSize(float fontSize)
    {
        webGLFontSize = fontSize;
        ApplyAdaptiveSettings();
    }
    
    // Method to force refresh
    public void RefreshAdaptiveSettings()
    {
        ApplyAdaptiveSettings();
    }
    
    // Public methods for dependency injection
    public void SetAdaptiveUIManager(AdaptiveUIManager manager)
    {
        adaptiveUIManager = manager;
        ApplyAdaptiveSettings();
    }
    
    public void SetAdaptiveUISystem(AdaptiveUISystem system)
    {
        adaptiveUISystem = system;
        SubscribeToSystemEvents();
    }
} 