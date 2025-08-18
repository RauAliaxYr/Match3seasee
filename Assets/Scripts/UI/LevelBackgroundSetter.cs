using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically sets the correct background for level scenes based on platform and chapter data
/// </summary>
public class LevelBackgroundSetter : MonoBehaviour
{
    [Header("Background Components")]
    [Tooltip("UI Image component for the background (if using UI)")]
    public Image backgroundImage;
    
    [Tooltip("SpriteRenderer component for the background (if using 2D)")]
    public SpriteRenderer backgroundSpriteRenderer;
    
    [Tooltip("AdaptiveBackground component (optional, for advanced features)")]
    public AdaptiveBackground adaptiveBackground;
    
    [Header("Chapter Data")]
    [Tooltip("Current chapter data - will be set automatically if not assigned")]
    public LevelChapterData currentChapter;
    
    [Header("Dependencies")]
    [Tooltip("GameManager reference (optional, for automatic chapter detection)")]
    public GameManager gameManager;
    
    [Header("Settings")]
    [Tooltip("Whether to automatically find background components in children")]
    public bool autoFindComponents = true;
    
    [Tooltip("Whether to apply background immediately on Start")]
    public bool applyOnStart = true;
    
    private void Start()
    {
        if (applyOnStart)
        {
            ApplyBackground();
        }
    }
    
    /// <summary>
    /// Applies the correct background for the current platform and chapter
    /// </summary>
    public void ApplyBackground()
    {
        // Auto-find components if enabled
        if (autoFindComponents)
        {
            FindBackgroundComponents();
        }
        
        // Get chapter data if not set
        if (currentChapter == null)
        {
            FindCurrentChapter();
        }
        
        if (currentChapter == null)
        {
            Debug.LogWarning("LevelBackgroundSetter: No chapter data found! Please assign currentChapter in inspector or set it via SetChapterData()");
            return;
        }
        
        // Get the correct background for current platform
        Sprite correctBackground = currentChapter.GetBackgroundForCurrentPlatform();
        
        if (correctBackground == null)
        {
            Debug.LogWarning($"LevelBackgroundSetter: No background found for chapter '{currentChapter.chapterName}' on current platform!");
            return;
        }
        
        // Apply to UI Image
        if (backgroundImage != null)
        {
            backgroundImage.sprite = correctBackground;
            Debug.Log($"LevelBackgroundSetter: Applied background '{correctBackground.name}' to UI Image for chapter '{currentChapter.chapterName}'");
        }
        
        // Apply to SpriteRenderer
        if (backgroundSpriteRenderer != null)
        {
            backgroundSpriteRenderer.sprite = correctBackground;
            Debug.Log($"LevelBackgroundSetter: Applied background '{correctBackground.name}' to SpriteRenderer for chapter '{currentChapter.chapterName}'");
        }
        
        // Apply to AdaptiveBackground if available
        if (adaptiveBackground != null)
        {
            adaptiveBackground.SetMobileBackground(currentChapter.backgroundMobile);
            adaptiveBackground.SetDesktopBackground(currentChapter.backgroundDesktop);
            adaptiveBackground.ApplyBackground();
            Debug.Log($"LevelBackgroundSetter: Applied adaptive background for chapter '{currentChapter.chapterName}'");
        }
    }
    
    /// <summary>
    /// Automatically finds background components in children using GetComponent
    /// </summary>
    private void FindBackgroundComponents()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponentInChildren<Image>();
        }
        
        if (backgroundSpriteRenderer == null)
        {
            backgroundSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (adaptiveBackground == null)
        {
            adaptiveBackground = GetComponentInChildren<AdaptiveBackground>();
        }
    }
    
    /// <summary>
    /// Attempts to find the current chapter data from assigned dependencies
    /// </summary>
    private void FindCurrentChapter()
    {
        // Try to get from assigned GameManager
        if (gameManager != null)
        {
            // This depends on your GameManager implementation
            // You might need to adjust this based on your actual GameManager structure
            Debug.Log("LevelBackgroundSetter: Found GameManager, but chapter data access depends on your implementation");
        }
        
        // Try to find in parent objects using GetComponentInParent
        if (currentChapter == null)
        {
            var parentGameManager = GetComponentInParent<GameManager>();
            if (parentGameManager != null)
            {
                Debug.Log("LevelBackgroundSetter: Found GameManager in parent, but chapter data access depends on your implementation");
            }
        }
    }
    
    /// <summary>
    /// Sets the current chapter data manually
    /// </summary>
    public void SetChapterData(LevelChapterData chapter)
    {
        currentChapter = chapter;
        ApplyBackground();
    }
    
    /// <summary>
    /// Sets the GameManager reference
    /// </summary>
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }
    
    /// <summary>
    /// Sets the background Image component
    /// </summary>
    public void SetBackgroundImage(Image image)
    {
        backgroundImage = image;
    }
    
    /// <summary>
    /// Sets the background SpriteRenderer component
    /// </summary>
    public void SetBackgroundSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        backgroundSpriteRenderer = spriteRenderer;
    }
    
    /// <summary>
    /// Sets the AdaptiveBackground component
    /// </summary>
    public void SetAdaptiveBackground(AdaptiveBackground adaptive)
    {
        adaptiveBackground = adaptive;
    }
    
    /// <summary>
    /// Refreshes the background (useful when platform changes)
    /// </summary>
    public void RefreshBackground()
    {
        ApplyBackground();
    }
} 