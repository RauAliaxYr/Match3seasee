using UnityEngine;
using UnityEngine.UI;

public class SafeAreaHandler : MonoBehaviour
{
    [Header("Safe Area Settings")]
    [SerializeField] private bool useSafeArea = true;
    [SerializeField] private RectTransform safeAreaRect;
    [SerializeField] private Canvas canvas;
    
    [Header("Padding")]
    [SerializeField] private float topPadding = 0f;
    [SerializeField] private float bottomPadding = 0f;
    [SerializeField] private float leftPadding = 0f;
    [SerializeField] private float rightPadding = 0f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private Rect lastSafeArea = Rect.zero;
    private Vector2Int lastScreenSize = Vector2Int.zero;
    
    private void Awake()
    {
        if (safeAreaRect == null)
            safeAreaRect = GetComponent<RectTransform>();
            
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }
    
    private void Start()
    {
        ApplySafeArea();
    }
    
    private void Update()
    {
        // Check if safe area or screen size changed
        if (useSafeArea && (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height))
        {
            ApplySafeArea();
        }
    }
    
    private void ApplySafeArea()
    {
        if (!useSafeArea || safeAreaRect == null) return;
        
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        
        // Convert safe area to anchor points
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        // Apply padding
        anchorMin.x += leftPadding / Screen.width;
        anchorMin.y += bottomPadding / Screen.height;
        anchorMax.x -= rightPadding / Screen.width;
        anchorMax.y -= topPadding / Screen.height;
        
        // Clamp values
        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);
        
        // Apply to rect transform
        safeAreaRect.anchorMin = anchorMin;
        safeAreaRect.anchorMax = anchorMax;
        
        if (showDebugInfo)
        {
            Debug.Log($"Safe Area Applied: {safeArea} | Screen: {Screen.width}x{Screen.height} | Anchors: {anchorMin} to {anchorMax}");
        }
    }
    
    public void SetSafeAreaEnabled(bool enabled)
    {
        useSafeArea = enabled;
        if (enabled)
        {
            ApplySafeArea();
        }
        else
        {
            // Reset to full screen
            if (safeAreaRect != null)
            {
                safeAreaRect.anchorMin = Vector2.zero;
                safeAreaRect.anchorMax = Vector2.one;
            }
        }
    }
    
    public void SetPadding(float top, float bottom, float left, float right)
    {
        topPadding = top;
        bottomPadding = bottom;
        leftPadding = left;
        rightPadding = right;
        ApplySafeArea();
    }
    
    public Rect GetCurrentSafeArea()
    {
        return Screen.safeArea;
    }
    
    public bool HasNotch()
    {
        return Screen.safeArea.y > 0 || Screen.safeArea.height < Screen.height;
    }
    
    public float GetNotchHeight()
    {
        return Screen.safeArea.y;
    }
    
    public float GetBottomSafeAreaHeight()
    {
        return Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
    }
    
    // Helper method to get safe area insets
    public Vector4 GetSafeAreaInsets()
    {
        Rect safeArea = Screen.safeArea;
        return new Vector4(
            safeArea.x,                    // Left
            safeArea.y,                    // Bottom
            Screen.width - safeArea.xMax,  // Right
            Screen.height - safeArea.yMax  // Top
        );
    }
    
    // Method to check if device has significant safe area
    public bool HasSignificantSafeArea()
    {
        Vector4 insets = GetSafeAreaInsets();
        float threshold = 20f; // Minimum safe area to consider significant
        
        return insets.x > threshold || insets.y > threshold || 
               insets.z > threshold || insets.w > threshold;
    }
    
    // Method to get recommended UI positioning
    public Vector2 GetRecommendedUIPosition(Vector2 originalPosition, bool isTopUI = false, bool isBottomUI = false)
    {
        Vector4 insets = GetSafeAreaInsets();
        Vector2 adjustedPosition = originalPosition;
        
        if (isTopUI)
        {
            adjustedPosition.y -= insets.w; // Adjust for top safe area
        }
        else if (isBottomUI)
        {
            adjustedPosition.y += insets.y; // Adjust for bottom safe area
        }
        
        return adjustedPosition;
    }
    
    // Method to get recommended UI size
    public Vector2 GetRecommendedUISize(Vector2 originalSize, bool isFullWidth = false, bool isFullHeight = false)
    {
        Vector4 insets = GetSafeAreaInsets();
        Vector2 adjustedSize = originalSize;
        
        if (isFullWidth)
        {
            adjustedSize.x = Screen.width - insets.x - insets.z;
        }
        
        if (isFullHeight)
        {
            adjustedSize.y = Screen.height - insets.y - insets.w;
        }
        
        return adjustedSize;
    }
    
    // Method to force refresh safe area
    public void RefreshSafeArea()
    {
        ApplySafeArea();
    }
    
    // Method to get device-specific safe area settings
    public SafeAreaSettings GetDeviceSafeAreaSettings()
    {
        SafeAreaSettings settings = new SafeAreaSettings();
        
        // Detect device type and apply specific settings
        #if UNITY_IOS
            settings.topPadding = 44f; // iPhone X and newer
            settings.bottomPadding = 34f;
        #elif UNITY_ANDROID
            settings.topPadding = 24f; // Android status bar
            settings.bottomPadding = 0f; // Navigation bar handled by system
        #else
            settings.topPadding = 0f;
            settings.bottomPadding = 0f;
        #endif
        
        return settings;
    }
    
    [System.Serializable]
    public class SafeAreaSettings
    {
        public float topPadding = 0f;
        public float bottomPadding = 0f;
        public float leftPadding = 0f;
        public float rightPadding = 0f;
    }
} 