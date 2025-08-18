using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdaptiveUIManager : MonoBehaviour
{
    [Header("Device Detection")]
    [SerializeField] private bool isMobileDevice;
    [SerializeField] private bool isDesktopDevice;
    [SerializeField] private bool isWebGLBuild;
    
    [Header("Screen Resolution")]
    [SerializeField] private Vector2 currentResolution;
    [SerializeField] private float aspectRatio;
    [SerializeField] private bool isPortrait;
    [SerializeField] private bool isLandscape;
    
    [Header("UI Scaling")]
    [SerializeField] private CanvasScaler canvasScaler;
    
    [Header("Safe Area")]
    [SerializeField] private RectTransform safeAreaRect;
    [SerializeField] private bool useSafeArea = true;
    
    [Header("UI Elements to Adapt")]
    [SerializeField] private RectTransform[] mobileOptimizedElements;
    [SerializeField] private RectTransform[] desktopOptimizedElements;
    [SerializeField] private TextMeshProUGUI[] adaptiveTexts;
    
    [Header("Mobile Settings")]
    [SerializeField] private float mobileTextScale = 0.8f;
    [SerializeField] private float mobileButtonScale = 0.9f;
    [SerializeField] private float mobileSpacing = 10f;
    
    [Header("Desktop Settings")]
    [SerializeField] private float desktopTextScale = 1.2f;
    [SerializeField] private float desktopButtonScale = 1.1f;
    [SerializeField] private float desktopSpacing = 20f;
    
    [Header("WebGL Settings")]
    [SerializeField] private float webGLTextScale = 1.1f;
    [SerializeField] private float webGLButtonScale = 1.05f;
    [SerializeField] private float webGLSpacing = 15f;
    
    private void Awake()
    {
        DetectDevice();
        DetectScreenProperties();
        SetupCanvasScaler();
    }
    
    private void Start()
    {
        ApplyAdaptiveSettings();
        SetupSafeArea();
    }
    
    private void DetectDevice()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            isWebGLBuild = true;
            isMobileDevice = false;
            isDesktopDevice = true;
        #elif UNITY_ANDROID || UNITY_IOS
            isMobileDevice = true;
            isDesktopDevice = false;
            isWebGLBuild = false;
        #else
            isMobileDevice = false;
            isDesktopDevice = true;
            isWebGLBuild = false;
        #endif
        
        // Additional mobile detection for WebGL
        if (isWebGLBuild)
        {
            string userAgent = Application.genuineCheckAvailable ? "Unknown" : "Unknown";
            isMobileDevice = userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone");
            isDesktopDevice = !isMobileDevice;
        }
    }
    
    private void DetectScreenProperties()
    {
        currentResolution = new Vector2(Screen.width, Screen.height);
        aspectRatio = currentResolution.x / currentResolution.y;
        isPortrait = currentResolution.y > currentResolution.x;
        isLandscape = currentResolution.x > currentResolution.y;
    }
    
    private void SetupCanvasScaler()
    {
        if (canvasScaler == null)
            canvasScaler = GetComponent<CanvasScaler>();
            
        if (canvasScaler == null) return;
        
        // Set reference resolution based on device
        if (isMobileDevice)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // Mobile reference
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }
        else if (isWebGLBuild)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080); // Desktop reference
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
        else
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080); // Desktop reference
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
    }
    
    private void ApplyAdaptiveSettings()
    {
        float textScale = 1f;
        float buttonScale = 1f;
        float spacing = 15f;
        
        if (isMobileDevice)
        {
            textScale = mobileTextScale;
            buttonScale = mobileButtonScale;
            spacing = mobileSpacing;
        }
        else if (isWebGLBuild)
        {
            textScale = webGLTextScale;
            buttonScale = webGLButtonScale;
            spacing = webGLSpacing;
        }
        else
        {
            textScale = desktopTextScale;
            buttonScale = desktopButtonScale;
            spacing = desktopSpacing;
        }
        
        // Apply text scaling
        foreach (var text in adaptiveTexts)
        {
            if (text != null)
            {
                text.fontSize *= textScale;
            }
        }
        
        // Apply button scaling
        foreach (var element in mobileOptimizedElements)
        {
            if (element != null && isMobileDevice)
            {
                element.localScale = Vector3.one * buttonScale;
            }
        }
        
        foreach (var element in desktopOptimizedElements)
        {
            if (element != null && isDesktopDevice)
            {
                element.localScale = Vector3.one * buttonScale;
            }
        }
    }
    
    private void SetupSafeArea()
    {
        if (!useSafeArea || safeAreaRect == null) return;
        
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        safeAreaRect.anchorMin = anchorMin;
        safeAreaRect.anchorMax = anchorMax;
    }
    
    public void UpdateUIForOrientation()
    {
        DetectScreenProperties();
        ApplyAdaptiveSettings();
        SetupSafeArea();
    }
    
    public bool IsMobileDevice()
    {
        return isMobileDevice;
    }
    
    public bool IsDesktopDevice()
    {
        return isDesktopDevice;
    }
    
    public bool IsWebGLBuild()
    {
        return isWebGLBuild;
    }
    
    public bool IsPortrait()
    {
        return isPortrait;
    }
    
    public bool IsLandscape()
    {
        return isLandscape;
    }
    
    public Vector2 GetCurrentResolution()
    {
        return currentResolution;
    }
    
    public float GetAspectRatio()
    {
        return aspectRatio;
    }
    
    // Helper method to get optimal font size for current device
    public float GetOptimalFontSize(float baseSize)
    {
        if (isMobileDevice)
            return baseSize * mobileTextScale;
        else if (isWebGLBuild)
            return baseSize * webGLTextScale;
        else
            return baseSize * desktopTextScale;
    }
    
    // Helper method to get optimal button scale for current device
    public float GetOptimalButtonScale()
    {
        if (isMobileDevice)
            return mobileButtonScale;
        else if (isWebGLBuild)
            return webGLButtonScale;
        else
            return desktopButtonScale;
    }
    
    // Helper method to get optimal spacing for current device
    public float GetOptimalSpacing()
    {
        if (isMobileDevice)
            return mobileSpacing;
        else if (isWebGLBuild)
            return webGLSpacing;
        else
            return desktopSpacing;
    }
} 