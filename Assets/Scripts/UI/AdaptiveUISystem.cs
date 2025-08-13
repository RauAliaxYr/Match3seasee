using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class AdaptiveUISystem : MonoBehaviour
{
    [Header("System Components")]
    [SerializeField] private AdaptiveUIManager adaptiveUIManager;
    [SerializeField] private SafeAreaHandler safeAreaHandler;
    [SerializeField] private OrientationManager orientationManager;
    [SerializeField] private PlatformSettings platformSettings;
    
    [Header("Background Management")]
    [SerializeField] private AdaptiveBackground[] adaptiveBackgrounds;
    
    [Header("UI Canvas")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private CanvasScaler mainCanvasScaler;
    [SerializeField] private RenderMode preferredRenderMode = RenderMode.ScreenSpaceCamera;
    
    [Header("Adaptive Elements")]
    [SerializeField] private List<AdaptiveUIElement> adaptiveElements = new List<AdaptiveUIElement>();
    [SerializeField] private List<RectTransform> dynamicElements = new List<RectTransform>();
    
    [Header("Presets")]
    [SerializeField] private AdaptiveUIPreset mobilePreset;
    [SerializeField] private AdaptiveUIPreset desktopPreset;
    [SerializeField] private AdaptiveUIPreset webGLPreset;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool autoFindComponents = true;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onSystemInitialized;
    [SerializeField] private UnityEvent onPresetApplied;
    [SerializeField] private UnityEvent onOrientationChanged;
    [SerializeField] private UnityEvent onDeviceChanged;
    
    private bool isInitialized = false;
    
    public static AdaptiveUISystem Instance { get; private set; }
    
    // Events for other components to subscribe to
    public UnityEvent OnSystemInitialized => onSystemInitialized;
    public UnityEvent OnPresetApplied => onPresetApplied;
    public UnityEvent OnOrientationChanged => onOrientationChanged;
    public UnityEvent OnDeviceChanged => onDeviceChanged;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (autoFindComponents)
        {
            FindComponents();
        }
    }
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            InitializeSystem();
        }
    }
    
    private void FindComponents()
    {
        // Find components if not assigned - use GetComponentInChildren/GetComponentInParent instead of FindObjectOfType
        if (adaptiveUIManager == null)
            adaptiveUIManager = GetComponentInChildren<AdaptiveUIManager>();
            
        if (safeAreaHandler == null)
            safeAreaHandler = GetComponentInChildren<SafeAreaHandler>();
            
        if (orientationManager == null)
            orientationManager = GetComponentInChildren<OrientationManager>();
            
        if (platformSettings == null)
            platformSettings = GetComponentInChildren<PlatformSettings>();
            
        if (mainCanvas == null)
            mainCanvas = GetComponentInChildren<Canvas>();
            
        if (mainCanvasScaler == null && mainCanvas != null)
            mainCanvasScaler = mainCanvas.GetComponent<CanvasScaler>();
            
        // If still not found, try to find in parent
        if (adaptiveUIManager == null)
            adaptiveUIManager = GetComponentInParent<AdaptiveUIManager>();
            
        if (safeAreaHandler == null)
            safeAreaHandler = GetComponentInParent<SafeAreaHandler>();
            
        if (orientationManager == null)
            orientationManager = GetComponentInParent<OrientationManager>();
            
        if (platformSettings == null)
            platformSettings = GetComponentInParent<PlatformSettings>();
            
        if (mainCanvas == null)
            mainCanvas = GetComponentInParent<Canvas>();
            
        // Find adaptive backgrounds
        if (adaptiveBackgrounds == null || adaptiveBackgrounds.Length == 0)
        {
            adaptiveBackgrounds = FindObjectsByType<AdaptiveBackground>(FindObjectsSortMode.None);
        }
    }
    
    public void InitializeSystem()
    {
        if (isInitialized) return;
        
        Debug.Log("Initializing Adaptive UI System...");
        
        // Initialize components
        if (adaptiveUIManager != null)
        {
            // AdaptiveUIManager initializes itself
        }
        
        if (safeAreaHandler != null)
        {
            // SafeAreaHandler initializes itself
        }
        
        if (orientationManager != null)
        {
            // OrientationManager initializes itself
        }
        
        if (platformSettings != null)
        {
            // PlatformSettings initializes itself
        }
        
        // Setup canvas
        SetupCanvas();
        
        // Apply initial settings
        ApplyCurrentPreset();
        
        // Setup adaptive elements
        SetupAdaptiveElements();
        
        isInitialized = true;
        
        // Invoke initialization event
        onSystemInitialized?.Invoke();
        
        Debug.Log("Adaptive UI System initialized successfully");
    }
    
    private void SetupCanvas()
    {
        if (mainCanvas == null || mainCanvasScaler == null) return;
        
        // Set canvas render mode based on preferred setting
        mainCanvas.renderMode = preferredRenderMode;
        
        // Set the camera for the canvas if using ScreenSpaceCamera mode
        if (preferredRenderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCanvas.worldCamera = mainCamera;
                Debug.Log($"Canvas set to ScreenSpaceCamera mode with camera: {mainCamera.name}");
            }
            else
            {
                // Fallback to overlay mode if no camera found
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.LogWarning("No main camera found, falling back to ScreenSpaceOverlay mode");
            }
        }
        else
        {
            Debug.Log($"Canvas set to {preferredRenderMode} mode");
        }
        
        // Setup canvas scaler based on platform
        if (platformSettings != null)
        {
            if (platformSettings.IsMobile())
            {
                mainCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                mainCanvasScaler.referenceResolution = new Vector2(1080, 1920);
                mainCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                mainCanvasScaler.matchWidthOrHeight = 0.5f;
            }
            else if (platformSettings.IsWebGL())
            {
                mainCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                mainCanvasScaler.referenceResolution = new Vector2(1920, 1080);
                mainCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                mainCanvasScaler.matchWidthOrHeight = 0.5f;
            }
            else
            {
                mainCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                mainCanvasScaler.referenceResolution = new Vector2(1920, 1080);
                mainCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                mainCanvasScaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
    
    private void ApplyCurrentPreset()
    {
        AdaptiveUIPreset currentPreset = GetCurrentPreset();
        
        if (currentPreset != null)
        {
            ApplyPreset(currentPreset);
        }
    }
    
    private AdaptiveUIPreset GetCurrentPreset()
    {
        if (platformSettings != null)
        {
            if (platformSettings.IsMobile())
                return mobilePreset;
            else if (platformSettings.IsWebGL())
                return webGLPreset;
            else
                return desktopPreset;
        }
        
        return desktopPreset; // Default fallback
    }
    
    private void ApplyPreset(AdaptiveUIPreset preset)
    {
        if (preset == null) return;
        
        // Apply preset settings to adaptive elements
        foreach (var element in adaptiveElements)
        {
            if (element != null)
            {
                element.SetMobileSize(preset.mobileSize);
                element.SetDesktopSize(preset.desktopSize);
                element.SetWebGLSize(preset.webGLSize);
                element.SetMobileFontSize(preset.mobileFontSize);
                element.SetDesktopFontSize(preset.desktopFontSize);
                element.SetWebGLFontSize(preset.webGLFontSize);
                element.RefreshAdaptiveSettings();
            }
        }
        
        // Apply preset to dynamic elements
        foreach (var element in dynamicElements)
        {
            if (element != null)
            {
                ApplyPresetToElement(element, preset);
            }
        }
        
        // Invoke preset applied event
        onPresetApplied?.Invoke();
    }
    
    private void ApplyPresetToElement(RectTransform element, AdaptiveUIPreset preset)
    {
        if (platformSettings != null)
        {
            Vector2 targetSize = platformSettings.IsMobile() ? preset.mobileSize : 
                                platformSettings.IsWebGL() ? preset.webGLSize : preset.desktopSize;
            
            element.sizeDelta = targetSize;
        }
    }
    
    private void SetupAdaptiveElements()
    {
        // Find all adaptive elements in the scene - use GetComponentsInChildren instead of FindObjectsOfType
        AdaptiveUIElement[] foundElements = GetComponentsInChildren<AdaptiveUIElement>();
        
        foreach (var element in foundElements)
        {
            if (!adaptiveElements.Contains(element))
            {
                adaptiveElements.Add(element);
                // Inject dependencies
                element.SetAdaptiveUISystem(this);
                if (adaptiveUIManager != null)
                {
                    element.SetAdaptiveUIManager(adaptiveUIManager);
                }
            }
        }
        
        // Apply settings to all elements
        foreach (var element in adaptiveElements)
        {
            if (element != null)
            {
                element.ApplyAdaptiveSettings();
            }
        }
    }
    
    // Public methods for runtime control
    public void RefreshAllElements()
    {
        foreach (var element in adaptiveElements)
        {
            if (element != null)
            {
                element.RefreshAdaptiveSettings();
            }
        }
        
        if (safeAreaHandler != null)
            safeAreaHandler.RefreshSafeArea();
            
        if (orientationManager != null)
            orientationManager.RefreshOrientation();
            
        if (adaptiveUIManager != null)
            adaptiveUIManager.UpdateUIForOrientation();
            
        // Refresh adaptive backgrounds
        RefreshAllBackgrounds();
    }
    
    public void AddAdaptiveElement(AdaptiveUIElement element)
    {
        if (!adaptiveElements.Contains(element))
        {
            adaptiveElements.Add(element);
            // Inject dependencies
            element.SetAdaptiveUISystem(this);
            if (adaptiveUIManager != null)
            {
                element.SetAdaptiveUIManager(adaptiveUIManager);
            }
            element.ApplyAdaptiveSettings();
        }
    }
    
    public void RemoveAdaptiveElement(AdaptiveUIElement element)
    {
        adaptiveElements.Remove(element);
    }
    
    public void AddDynamicElement(RectTransform element)
    {
        if (!dynamicElements.Contains(element))
        {
            dynamicElements.Add(element);
            ApplyCurrentPreset();
        }
    }
    
    public void RemoveDynamicElement(RectTransform element)
    {
        dynamicElements.Remove(element);
    }
    
    public void SetPreset(AdaptiveUIPreset preset)
    {
        ApplyPreset(preset);
    }
    
    public void ForceMobileLayout()
    {
        if (mobilePreset != null)
        {
            ApplyPreset(mobilePreset);
        }
    }
    
    public void ForceDesktopLayout()
    {
        if (desktopPreset != null)
        {
            ApplyPreset(desktopPreset);
        }
    }
    
    public void ForceWebGLLayout()
    {
        if (webGLPreset != null)
        {
            ApplyPreset(webGLPreset);
        }
    }
    
    // Helper methods
    public bool IsMobileDevice()
    {
        return platformSettings != null && platformSettings.IsMobile();
    }
    
    public bool IsWebGLBuild()
    {
        return platformSettings != null && platformSettings.IsWebGL();
    }
    
    public bool IsDesktopDevice()
    {
        return platformSettings != null && platformSettings.IsStandalone();
    }
    
    public Vector2 GetCurrentResolution()
    {
        return new Vector2(Screen.width, Screen.height);
    }
    
    public float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }
    
    public bool IsPortrait()
    {
        return Screen.height > Screen.width;
    }
    
    public bool IsLandscape()
    {
        return Screen.width > Screen.height;
    }
    
    // Method to get optimal UI settings for current device
    public AdaptiveUISettings GetOptimalUISettings()
    {
        AdaptiveUISettings settings = new AdaptiveUISettings();
        
        if (IsMobileDevice())
        {
            settings.textScale = 0.8f;
            settings.buttonScale = 0.9f;
            settings.spacing = 10f;
            settings.referenceResolution = new Vector2(1080, 1920);
        }
        else if (IsWebGLBuild())
        {
            settings.textScale = 1.1f;
            settings.buttonScale = 1.05f;
            settings.spacing = 15f;
            settings.referenceResolution = new Vector2(1920, 1080);
        }
        else
        {
            settings.textScale = 1.2f;
            settings.buttonScale = 1.1f;
            settings.spacing = 20f;
            settings.referenceResolution = new Vector2(1920, 1080);
        }
        
        return settings;
    }
    
    // Public methods for dependency injection
    public void SetAdaptiveUIManager(AdaptiveUIManager manager)
    {
        adaptiveUIManager = manager;
        if (isInitialized)
        {
            RefreshAllElements();
        }
    }
    
    public void SetSafeAreaHandler(SafeAreaHandler handler)
    {
        safeAreaHandler = handler;
    }
    
    public void SetOrientationManager(OrientationManager manager)
    {
        orientationManager = manager;
    }
    
    public void SetPlatformSettings(PlatformSettings settings)
    {
        platformSettings = settings;
        if (isInitialized)
        {
            ApplyCurrentPreset();
        }
    }
    
    public void SetMainCanvas(Canvas canvas)
    {
        mainCanvas = canvas;
        if (canvas != null)
        {
            mainCanvasScaler = canvas.GetComponent<CanvasScaler>();
        }
    }
    
    // Canvas render mode management methods
    public void SetCanvasToCameraMode()
    {
        if (mainCanvas != null)
        {
            mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCanvas.worldCamera = mainCamera;
            }
            Debug.Log("Canvas set to ScreenSpaceCamera mode");
        }
    }
    
    public void SetCanvasToOverlayMode()
    {
        if (mainCanvas != null)
        {
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Debug.Log("Canvas set to ScreenSpaceOverlay mode");
        }
    }
    
    public void SetCanvasToWorldSpaceMode()
    {
        if (mainCanvas != null)
        {
            mainCanvas.renderMode = RenderMode.WorldSpace;
            Debug.Log("Canvas set to WorldSpace mode");
        }
    }
    
    public RenderMode GetCurrentCanvasRenderMode()
    {
        return mainCanvas != null ? mainCanvas.renderMode : RenderMode.ScreenSpaceOverlay;
    }
    
    // Background management methods
    public void RefreshAllBackgrounds()
    {
        if (adaptiveBackgrounds != null)
        {
            foreach (var background in adaptiveBackgrounds)
            {
                if (background != null)
                {
                    background.RefreshBackground();
                }
            }
        }
    }
    
    // Method to update backgrounds when orientation changes
    public void UpdateBackgroundsForOrientation()
    {
        if (adaptiveBackgrounds != null)
        {
            foreach (var background in adaptiveBackgrounds)
            {
                if (background != null)
                {
                    background.OnOrientationChanged();
                }
            }
        }
        
        // Invoke orientation changed event
        onOrientationChanged?.Invoke();
    }
    
    public void AddAdaptiveBackground(AdaptiveBackground background)
    {
        if (background != null && !System.Array.Exists(adaptiveBackgrounds, x => x == background))
        {
            System.Array.Resize(ref adaptiveBackgrounds, adaptiveBackgrounds.Length + 1);
            adaptiveBackgrounds[adaptiveBackgrounds.Length - 1] = background;
        }
    }
    
    public void RemoveAdaptiveBackground(AdaptiveBackground background)
    {
        if (adaptiveBackgrounds != null)
        {
            var list = new System.Collections.Generic.List<AdaptiveBackground>(adaptiveBackgrounds);
            list.Remove(background);
            adaptiveBackgrounds = list.ToArray();
        }
    }
    
    public AdaptiveBackground[] GetAllAdaptiveBackgrounds()
    {
        return adaptiveBackgrounds;
    }
    
    [System.Serializable]
    public class AdaptiveUISettings
    {
        public float textScale = 1f;
        public float buttonScale = 1f;
        public float spacing = 15f;
        public Vector2 referenceResolution = new Vector2(1920, 1080);
        public bool useSafeArea = true;
        public bool adaptToOrientation = true;
    }
}

[System.Serializable]
public class AdaptiveUIPreset
{
    public string presetName = "Default";
    public Vector2 mobileSize = new Vector2(100, 100);
    public Vector2 desktopSize = new Vector2(120, 120);
    public Vector2 webGLSize = new Vector2(110, 110);
    public float mobileFontSize = 24f;
    public float desktopFontSize = 28f;
    public float webGLFontSize = 26f;
    public float mobileScale = 1f;
    public float desktopScale = 1f;
    public float webGLScale = 1f;
} 