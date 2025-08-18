using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes the level grid adaptive for different platforms
/// </summary>
public class AdaptiveLevelGrid : MonoBehaviour
{
    [Header("Grid Layout Group")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    
    [Header("Mobile Settings")]
    [Tooltip("Cell size for mobile devices")]
    public Vector2 mobileCellSize = new Vector2(80f, 80f);
    
    [Tooltip("Spacing for mobile devices")]
    public Vector2 mobileSpacing = new Vector2(10f, 10f);
    
    [Tooltip("Padding for mobile devices")]
    public RectOffset mobilePadding;
    
    [Header("Desktop Settings")]
    [Tooltip("Cell size for desktop")]
    public Vector2 desktopCellSize = new Vector2(120f, 120f);
    
    [Tooltip("Spacing for desktop")]
    public Vector2 desktopSpacing = new Vector2(20f, 20f);
    
    [Tooltip("Padding for desktop")]
    public RectOffset desktopPadding;
    
    [Header("WebGL Settings")]
    [Tooltip("Cell size for WebGL")]
    public Vector2 webGLCellSize = new Vector2(100f, 100f);
    
    [Tooltip("Spacing for WebGL")]
    public Vector2 webGLSpacing = new Vector2(15f, 15f);
    
    [Tooltip("Padding for WebGL")]
    public RectOffset webGLPadding;
    
    [Header("Settings")]
    [Tooltip("Whether to apply settings immediately on Start")]
    public bool applyOnStart = true;
    
    [Tooltip("Whether to automatically find GridLayoutGroup")]
    public bool autoFindGridLayoutGroup = true;
    
    private void Awake()
    {
        // Initialize RectOffset objects here instead of in field initializers
        mobilePadding = new RectOffset(10, 10, 10, 10);
        desktopPadding = new RectOffset(20, 20, 20, 20);
        webGLPadding = new RectOffset(15, 15, 15, 15);
    }
    
    private void Start()
    {
        if (applyOnStart)
        {
            ApplyGridSettings();
        }
    }
    
    /// <summary>
    /// Applies the appropriate grid settings for the current platform
    /// </summary>
    public void ApplyGridSettings()
    {
        // Auto-find GridLayoutGroup if enabled
        if (autoFindGridLayoutGroup)
        {
            FindGridLayoutGroup();
        }
        
        // Apply settings based on platform
#if UNITY_ANDROID || UNITY_IOS
        ApplyMobileSettings();
#elif UNITY_WEBGL
        ApplyWebGLSettings();
#else
        ApplyDesktopSettings();
#endif
        
        Debug.Log($"AdaptiveLevelGrid: Applied {GetCurrentPlatform()} grid settings");
    }
    
    /// <summary>
    /// Automatically finds GridLayoutGroup component
    /// </summary>
    private void FindGridLayoutGroup()
    {
        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
        }
    }
    
    /// <summary>
    /// Applies mobile-specific grid settings
    /// </summary>
    private void ApplyMobileSettings()
    {
        ApplyGridSettings(mobileCellSize, mobileSpacing, mobilePadding);
    }
    
    /// <summary>
    /// Applies desktop-specific grid settings
    /// </summary>
    private void ApplyDesktopSettings()
    {
        ApplyGridSettings(desktopCellSize, desktopSpacing, desktopPadding);
    }
    
    /// <summary>
    /// Applies WebGL-specific grid settings
    /// </summary>
    private void ApplyWebGLSettings()
    {
        ApplyGridSettings(webGLCellSize, webGLSpacing, webGLPadding);
    }
    
    /// <summary>
    /// Applies grid settings for the specified platform
    /// </summary>
    private void ApplyGridSettings(Vector2 cellSize, Vector2 spacing, RectOffset padding)
    {
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.cellSize = cellSize;
            gridLayoutGroup.spacing = spacing;
            gridLayoutGroup.padding = padding;
            
            // Force layout update
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayoutGroup.GetComponent<RectTransform>());
        }
    }
    
    /// <summary>
    /// Gets the current platform name for logging
    /// </summary>
    private string GetCurrentPlatform()
    {
#if UNITY_ANDROID || UNITY_IOS
        return "Mobile";
#elif UNITY_WEBGL
        return "WebGL";
#else
        return "Desktop";
#endif
    }
    
    /// <summary>
    /// Refreshes the grid settings (useful when platform changes)
    /// </summary>
    public void RefreshGrid()
    {
        ApplyGridSettings();
    }
    
    /// <summary>
    /// Sets custom grid settings for current platform
    /// </summary>
    public void SetGridSettings(Vector2 cellSize, Vector2 spacing, RectOffset padding)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileCellSize = cellSize;
        mobileSpacing = spacing;
        mobilePadding = padding;
#elif UNITY_WEBGL
        webGLCellSize = cellSize;
        webGLSpacing = spacing;
        webGLPadding = padding;
#else
        desktopCellSize = cellSize;
        desktopSpacing = spacing;
        desktopPadding = padding;
#endif
        
        ApplyGridSettings();
    }
    
    /// <summary>
    /// Sets custom cell size for current platform
    /// </summary>
    public void SetCellSize(Vector2 cellSize)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileCellSize = cellSize;
#elif UNITY_WEBGL
        webGLCellSize = cellSize;
#else
        desktopCellSize = cellSize;
#endif
        
        ApplyGridSettings();
    }
    
    /// <summary>
    /// Sets custom spacing for current platform
    /// </summary>
    public void SetSpacing(Vector2 spacing)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileSpacing = spacing;
#elif UNITY_WEBGL
        webGLSpacing = spacing;
#else
        desktopSpacing = spacing;
#endif
        
        ApplyGridSettings();
    }
    
    /// <summary>
    /// Sets custom padding for current platform
    /// </summary>
    public void SetPadding(RectOffset padding)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobilePadding = padding;
#elif UNITY_WEBGL
        webGLPadding = padding;
#else
        desktopPadding = padding;
#endif
        
        ApplyGridSettings();
    }
} 