using UnityEngine;
using UnityEngine.UI;

public class PlatformSettings : MonoBehaviour
{
    [Header("Platform Detection")]
    [SerializeField] private bool isWebGLBuild;
    [SerializeField] private bool isAndroidBuild;
    [SerializeField] private bool isIOSBuild;
    [SerializeField] private bool isStandaloneBuild;
    
    [Header("Quality Settings")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool vsyncEnabled = true;
    [SerializeField] private int antiAliasing = 4;
    
    [Header("WebGL Specific")]
    [SerializeField] private bool webGLFullscreen = false;
    [SerializeField] private Vector2 webGLResolution = new Vector2(1920, 1080);
    
    [Header("Mobile Specific")]
    [SerializeField] private bool mobileFullscreen = true;
    [SerializeField] private bool mobileSleepTimeout = true;
    [SerializeField] private Vector2 mobileResolution = new Vector2(1080, 1920);
    
    [Header("UI Settings")]
    [SerializeField] private float mobileUIScale = 0.8f;
    [SerializeField] private float desktopUIScale = 1.2f;
    [SerializeField] private float webGLUIScale = 1.1f;
    
    [Header("Performance")]
    [SerializeField] private bool enableAdaptivePerformance = true;
    
    private void Awake()
    {
        DetectPlatform();
        ApplyPlatformSettings();
    }
    
    private void Start()
    {
        ApplyQualitySettings();
        ApplyUISettings();
    }
    
    private void DetectPlatform()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            isWebGLBuild = true;
            isAndroidBuild = false;
            isIOSBuild = false;
            isStandaloneBuild = false;
        #elif UNITY_ANDROID
            isAndroidBuild = true;
            isWebGLBuild = false;
            isIOSBuild = false;
            isStandaloneBuild = false;
        #elif UNITY_IOS
            isIOSBuild = true;
            isWebGLBuild = false;
            isAndroidBuild = false;
            isStandaloneBuild = false;
        #else
            isStandaloneBuild = true;
            isWebGLBuild = false;
            isAndroidBuild = false;
            isIOSBuild = false;
        #endif
    }
    
    private void ApplyPlatformSettings()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Set vsync
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
        
        // Set anti-aliasing
        QualitySettings.antiAliasing = antiAliasing;
        
        // Platform-specific settings
        if (isWebGLBuild)
        {
            ApplyWebGLSettings();
        }
        else if (isAndroidBuild || isIOSBuild)
        {
            ApplyMobileSettings();
        }
        else
        {
            ApplyStandaloneSettings();
        }
    }
    
    private void ApplyWebGLSettings()
    {
        // WebGL specific settings
        Screen.fullScreen = webGLFullscreen;
        
        // Don't force resolution - let Unity handle it automatically
        // This prevents viewport issues when switching scenes
        
        // WebGL performance settings
        QualitySettings.shadowDistance = 50f;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.pixelLightCount = 2;
        
        Debug.Log("WebGL settings applied (resolution auto-handled)");
    }
    
    private void ApplyMobileSettings()
    {
        // Mobile specific settings
        Screen.fullScreen = mobileFullscreen;
        
        // Prevent sleep
        if (mobileSleepTimeout)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        
        // Don't force resolution - let Unity handle it automatically
        // This prevents viewport issues when switching scenes
        
        // Mobile performance settings
        QualitySettings.shadowDistance = 30f;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.pixelLightCount = 1;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
        
        // Mobile texture settings
        QualitySettings.globalTextureMipmapLimit = 1; // Half resolution textures
        
        Debug.Log("Mobile settings applied (resolution auto-handled)");
    }
    
    private void ApplyStandaloneSettings()
    {
        // Standalone specific settings
        Screen.fullScreen = false;
        
        // Standalone performance settings
        QualitySettings.shadowDistance = 100f;
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.pixelLightCount = 4;
        QualitySettings.softParticles = true;
        QualitySettings.realtimeReflectionProbes = true;
        
        Debug.Log("Standalone settings applied");
    }
    
    private void ApplyQualitySettings()
    {
        // Apply adaptive performance if available
        if (enableAdaptivePerformance)
        {
            #if UNITY_ANDROID
                // Adaptive Performance is not available in this Unity version
                // Consider using Unity's built-in performance monitoring instead
                Debug.Log("Adaptive Performance not available in this Unity version");
            #endif
        }
        
        // Set texture quality
        QualitySettings.globalTextureMipmapLimit = GetOptimalTextureLimit();
        
        // Set shadow quality
        QualitySettings.shadowCascades = GetOptimalShadowCascades();
        
        // Set LOD bias
        QualitySettings.lodBias = GetOptimalLODBias();
    }
    
    private void ApplyUISettings()
    {
        // Find and apply UI scaling
        CanvasScaler[] canvasScalers = FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
        
        foreach (var scaler in canvasScalers)
        {
            float targetScale = GetOptimalUIScale();
            
            if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                // Adjust reference resolution based on platform
                Vector2 referenceResolution = GetOptimalReferenceResolution();
                scaler.referenceResolution = referenceResolution;
            }
        }
    }
    
    private int GetOptimalTextureLimit()
    {
        if (isWebGLBuild)
            return 0; // Full resolution for WebGL
        else if (isAndroidBuild || isIOSBuild)
            return 1; // Half resolution for mobile
        else
            return 0; // Full resolution for desktop
    }
    
    private int GetOptimalShadowCascades()
    {
        if (isWebGLBuild)
            return 2; // Medium cascades for WebGL
        else if (isAndroidBuild || isIOSBuild)
            return 0; // No cascades for mobile
        else
            return 4; // Full cascades for desktop
    }
    
    private float GetOptimalLODBias()
    {
        if (isWebGLBuild)
            return 0.5f; // Medium LOD for WebGL
        else if (isAndroidBuild || isIOSBuild)
            return 0.3f; // Low LOD for mobile
        else
            return 1.0f; // Full LOD for desktop
    }
    
    private float GetOptimalUIScale()
    {
        if (isWebGLBuild)
            return webGLUIScale;
        else if (isAndroidBuild || isIOSBuild)
            return mobileUIScale;
        else
            return desktopUIScale;
    }
    
    private Vector2 GetOptimalReferenceResolution()
    {
        if (isWebGLBuild)
            return new Vector2(1920, 1080);
        else if (isAndroidBuild || isIOSBuild)
            return new Vector2(1080, 1920);
        else
            return new Vector2(1920, 1080);
    }
    
    // Public methods for runtime control
    public void SetTargetFrameRate(int frameRate)
    {
        targetFrameRate = frameRate;
        Application.targetFrameRate = frameRate;
    }
    
    public void SetVSync(bool enabled)
    {
        vsyncEnabled = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
    
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }
    
    public void SetResolution(int width, int height, bool fullscreen)
    {
        Screen.SetResolution(width, height, fullscreen);
    }
    
    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
    }
    
    // Platform-specific helper methods
    public bool IsWebGL()
    {
        return isWebGLBuild;
    }
    
    public bool IsMobile()
    {
        return isAndroidBuild || isIOSBuild;
    }
    
    public bool IsAndroid()
    {
        return isAndroidBuild;
    }
    
    public bool IsIOS()
    {
        return isIOSBuild;
    }
    
    public bool IsStandalone()
    {
        return isStandaloneBuild;
    }
    
    // Method to get optimal settings for current platform
    public PlatformOptimizationSettings GetOptimalSettings()
    {
        PlatformOptimizationSettings settings = new PlatformOptimizationSettings();
        
        if (isWebGLBuild)
        {
            settings.targetFrameRate = 60;
            settings.shadowDistance = 50f;
            settings.pixelLightCount = 2;
            settings.antiAliasing = 4;
            settings.textureQuality = 0;
        }
        else if (isAndroidBuild || isIOSBuild)
        {
            settings.targetFrameRate = 60;
            settings.shadowDistance = 30f;
            settings.pixelLightCount = 1;
            settings.antiAliasing = 2;
            settings.textureQuality = 1;
        }
        else
        {
            settings.targetFrameRate = 60;
            settings.shadowDistance = 100f;
            settings.pixelLightCount = 4;
            settings.antiAliasing = 8;
            settings.textureQuality = 0;
        }
        
        return settings;
    }
    
    [System.Serializable]
    public class PlatformOptimizationSettings
    {
        public int targetFrameRate = 60;
        public float shadowDistance = 50f;
        public int pixelLightCount = 2;
        public int antiAliasing = 4;
        public int textureQuality = 0;
        public bool vsync = true;
        public bool fullscreen = false;
    }
} 