using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adaptively changes the height of UI objects for different platforms
/// </summary>
public class AdaptiveHeightController : MonoBehaviour
{
    [Header("UI Objects")]
    [Tooltip("LevelRoot RectTransform")]
    public RectTransform levelRoot;
    
    [Tooltip("LevelGrid RectTransform")]
    public RectTransform levelGrid;
    
    [Tooltip("ChapterNavigation RectTransform")]
    public RectTransform chapterNavigation;
    
    [Header("Mobile Settings")]
    [Tooltip("LevelRoot height for mobile")]
    public float mobileLevelRootHeight = 640f;
    
    [Tooltip("LevelGrid height for mobile")]
    public float mobileLevelGridHeight = 480f;
    
    [Tooltip("ChapterNavigation height for mobile")]
    public float mobileChapterNavigationHeight = 160f;
    
    [Header("Desktop Settings")]
    [Tooltip("LevelRoot height for desktop")]
    public float desktopLevelRootHeight = 1080f;
    
    [Tooltip("LevelGrid height for desktop")]
    public float desktopLevelGridHeight = 800f;
    
    [Tooltip("ChapterNavigation height for desktop")]
    public float desktopChapterNavigationHeight = 300f;
    
    [Header("WebGL Settings")]
    [Tooltip("LevelRoot height for WebGL")]
    public float webGLLevelRootHeight = 1080f;
    
    [Tooltip("LevelGrid height for WebGL")]
    public float webGLLevelGridHeight = 800f;
    
    [Tooltip("ChapterNavigation height for WebGL")]
    public float webGLChapterNavigationHeight = 300f;
    
    [Header("Settings")]
    [Tooltip("Whether to apply settings immediately on Start")]
    public bool applyOnStart = true;
    
    [Tooltip("Whether to automatically find UI objects")]
    public bool autoFindObjects = true;
    
    private void Start()
    {
        if (applyOnStart)
        {
            ApplyHeightSettings();
        }
    }
    
    /// <summary>
    /// Applies the appropriate height settings for the current platform
    /// </summary>
    public void ApplyHeightSettings()
    {
        // Auto-find objects if enabled
        if (autoFindObjects)
        {
            FindUIObjects();
        }
        
        // Apply settings based on platform
#if UNITY_ANDROID || UNITY_IOS
        ApplyMobileSettings();
#elif UNITY_WEBGL
        ApplyWebGLSettings();
#else
        ApplyDesktopSettings();
#endif
        
        Debug.Log($"AdaptiveHeightController: Applied {GetCurrentPlatform()} height settings");
    }
    
    /// <summary>
    /// Automatically finds UI objects
    /// </summary>
    private void FindUIObjects()
    {
        if (levelRoot == null)
        {
            levelRoot = GetComponent<RectTransform>();
        }
        
        if (levelGrid == null)
        {
            levelGrid = GetComponentInChildren<RectTransform>();
        }
        
        if (chapterNavigation == null)
        {
            RectTransform[] children = GetComponentsInChildren<RectTransform>();
            if (children.Length > 1)
            {
                chapterNavigation = children[1];
            }
        }
    }
    
    /// <summary>
    /// Applies mobile-specific height settings
    /// </summary>
    private void ApplyMobileSettings()
    {
        ApplyHeightSettings(mobileLevelRootHeight, mobileLevelGridHeight, mobileChapterNavigationHeight);
    }
    
    /// <summary>
    /// Applies desktop-specific height settings
    /// </summary>
    private void ApplyDesktopSettings()
    {
        ApplyHeightSettings(desktopLevelRootHeight, desktopLevelGridHeight, desktopChapterNavigationHeight);
    }
    
    /// <summary>
    /// Applies WebGL-specific height settings
    /// </summary>
    private void ApplyWebGLSettings()
    {
        ApplyHeightSettings(webGLLevelRootHeight, webGLLevelGridHeight, webGLChapterNavigationHeight);
    }
    
    /// <summary>
    /// Applies height settings for the specified platform
    /// </summary>
    private void ApplyHeightSettings(float levelRootHeight, float levelGridHeight, float chapterNavigationHeight)
    {
        // Apply LevelRoot height
        if (levelRoot != null)
        {
            Vector2 sizeDelta = levelRoot.sizeDelta;
            sizeDelta.y = levelRootHeight;
            levelRoot.sizeDelta = sizeDelta;
        }
        
        // Apply LevelGrid height
        if (levelGrid != null)
        {
            Vector2 sizeDelta = levelGrid.sizeDelta;
            sizeDelta.y = levelGridHeight;
            levelGrid.sizeDelta = sizeDelta;
        }
        
        // Apply ChapterNavigation height
        if (chapterNavigation != null)
        {
            Vector2 sizeDelta = chapterNavigation.sizeDelta;
            sizeDelta.y = chapterNavigationHeight;
            chapterNavigation.sizeDelta = sizeDelta;
        }
        
        // Force layout update
        if (levelRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelRoot);
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
    /// Refreshes the height settings (useful when platform changes)
    /// </summary>
    public void RefreshHeights()
    {
        ApplyHeightSettings();
    }
    
    /// <summary>
    /// Sets custom heights for current platform
    /// </summary>
    public void SetHeights(float levelRootHeight, float levelGridHeight, float chapterNavigationHeight)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileLevelRootHeight = levelRootHeight;
        mobileLevelGridHeight = levelGridHeight;
        mobileChapterNavigationHeight = chapterNavigationHeight;
#elif UNITY_WEBGL
        webGLLevelRootHeight = levelRootHeight;
        webGLLevelGridHeight = levelGridHeight;
        webGLChapterNavigationHeight = chapterNavigationHeight;
#else
        desktopLevelRootHeight = levelRootHeight;
        desktopLevelGridHeight = levelGridHeight;
        desktopChapterNavigationHeight = chapterNavigationHeight;
#endif
        
        ApplyHeightSettings();
    }
    
    /// <summary>
    /// Sets custom LevelRoot height for current platform
    /// </summary>
    public void SetLevelRootHeight(float height)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileLevelRootHeight = height;
#elif UNITY_WEBGL
        webGLLevelRootHeight = height;
#else
        desktopLevelRootHeight = height;
#endif
        
        ApplyHeightSettings();
    }
    
    /// <summary>
    /// Sets custom LevelGrid height for current platform
    /// </summary>
    public void SetLevelGridHeight(float height)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileLevelGridHeight = height;
#elif UNITY_WEBGL
        webGLLevelGridHeight = height;
#else
        desktopLevelGridHeight = height;
#endif
        
        ApplyHeightSettings();
    }
    
    /// <summary>
    /// Sets custom ChapterNavigation height for current platform
    /// </summary>
    public void SetChapterNavigationHeight(float height)
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileChapterNavigationHeight = height;
#elif UNITY_WEBGL
        webGLChapterNavigationHeight = height;
#else
        desktopChapterNavigationHeight = height;
#endif
        
        ApplyHeightSettings();
    }
} 