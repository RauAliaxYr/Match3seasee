using UnityEngine;
using System.Collections.Generic;

public class OrientationManager : MonoBehaviour
{
    [Header("Orientation Settings")]
    [SerializeField] private bool allowPortrait = true;
    [SerializeField] private bool allowLandscape = true;
    [SerializeField] private bool forcePortrait = false;
    [SerializeField] private bool forceLandscape = false;
    
    [Header("Auto Rotation")]
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private float rotationCheckInterval = 0.5f;
    
    [Header("UI Adaptation")]
    [SerializeField] private List<GameObject> portraitOnlyElements = new List<GameObject>();
    [SerializeField] private List<GameObject> landscapeOnlyElements = new List<GameObject>();
    [SerializeField] private List<RectTransform> adaptiveElements = new List<RectTransform>();
    
    [Header("Layout Settings")]
    [SerializeField] private Vector2 portraitLayout = new Vector2(1080, 1920);
    [SerializeField] private Vector2 landscapeLayout = new Vector2(1920, 1080);
    
    private ScreenOrientation currentOrientation;
    private Vector2 lastScreenSize;
    private AdaptiveUIManager adaptiveUIManager;
    private SafeAreaHandler safeAreaHandler;
    
    public enum OrientationType
    {
        Portrait,
        Landscape,
        Auto
    }
    
    private void Awake()
    {
        adaptiveUIManager = FindFirstObjectByType<AdaptiveUIManager>();
        safeAreaHandler = FindFirstObjectByType<SafeAreaHandler>();
        
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        currentOrientation = Screen.orientation;
        
        SetupOrientation();
    }
    
    private void Start()
    {
        ApplyOrientationSettings();
        
        if (autoRotate)
        {
            InvokeRepeating(nameof(CheckOrientationChange), rotationCheckInterval, rotationCheckInterval);
        }
    }
    
    private void OnDestroy()
    {
        CancelInvoke(nameof(CheckOrientationChange));
    }
    
    private void SetupOrientation()
    {
        // Set allowed orientations
        if (forcePortrait)
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else if (forceLandscape)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        else
        {
            Screen.autorotateToPortrait = allowPortrait;
            Screen.autorotateToPortraitUpsideDown = allowPortrait;
            Screen.autorotateToLandscapeLeft = allowLandscape;
            Screen.autorotateToLandscapeRight = allowLandscape;
        }
    }
    
    private void CheckOrientationChange()
    {
        if (Screen.orientation != currentOrientation || 
            Screen.width != lastScreenSize.x || 
            Screen.height != lastScreenSize.y)
        {
            currentOrientation = Screen.orientation;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            
            ApplyOrientationSettings();
        }
    }
    
    private void ApplyOrientationSettings()
    {
        bool isPortrait = IsPortrait();
        
        // Show/hide orientation-specific elements
        foreach (var element in portraitOnlyElements)
        {
            if (element != null)
                element.SetActive(isPortrait);
        }
        
        foreach (var element in landscapeOnlyElements)
        {
            if (element != null)
                element.SetActive(!isPortrait);
        }
        
        // Adapt UI elements
        AdaptUIElements(isPortrait);
        
        // Update safe area
        if (safeAreaHandler != null)
        {
            safeAreaHandler.RefreshSafeArea();
        }
        
        // Update adaptive UI manager
        if (adaptiveUIManager != null)
        {
            adaptiveUIManager.UpdateUIForOrientation();
        }
        
        // Update backgrounds for orientation change
        UpdateBackgroundsForOrientation();
        
        Debug.Log($"Orientation changed to: {(isPortrait ? "Portrait" : "Landscape")} ({Screen.width}x{Screen.height})");
    }
    
    private void AdaptUIElements(bool isPortrait)
    {
        Vector2 targetLayout = isPortrait ? portraitLayout : landscapeLayout;
        float scaleFactor = Mathf.Min(Screen.width / targetLayout.x, Screen.height / targetLayout.y);
        
        foreach (var element in adaptiveElements)
        {
            if (element != null)
            {
                // Adjust element size and position based on orientation
                AdaptElement(element, isPortrait, scaleFactor);
            }
        }
    }
    
    private void AdaptElement(RectTransform element, bool isPortrait, float scaleFactor)
    {
        // Get current anchors
        Vector2 anchorMin = element.anchorMin;
        Vector2 anchorMax = element.anchorMax;
        Vector2 anchoredPosition = element.anchoredPosition;
        Vector2 sizeDelta = element.sizeDelta;
        
        if (isPortrait)
        {
            // Portrait adjustments
            if (anchorMin.y > 0.5f) // Top elements
            {
                // Adjust for notch/safe area
                anchoredPosition.y -= GetTopSafeAreaOffset();
            }
            else if (anchorMax.y < 0.5f) // Bottom elements
            {
                // Adjust for home indicator
                anchoredPosition.y += GetBottomSafeAreaOffset();
            }
        }
        else
        {
            // Landscape adjustments
            if (anchorMin.x < 0.5f) // Left elements
            {
                // Adjust for notch/safe area
                anchoredPosition.x += GetLeftSafeAreaOffset();
            }
            else if (anchorMax.x > 0.5f) // Right elements
            {
                // Adjust for notch/safe area
                anchoredPosition.x -= GetRightSafeAreaOffset();
            }
        }
        
        // Apply changes
        element.anchoredPosition = anchoredPosition;
        element.sizeDelta = sizeDelta * scaleFactor;
    }
    
    private float GetTopSafeAreaOffset()
    {
        if (safeAreaHandler != null)
        {
            return safeAreaHandler.GetNotchHeight();
        }
        return 0f;
    }
    
    private float GetBottomSafeAreaOffset()
    {
        if (safeAreaHandler != null)
        {
            return safeAreaHandler.GetBottomSafeAreaHeight();
        }
        return 0f;
    }
    
    private float GetLeftSafeAreaOffset()
    {
        if (safeAreaHandler != null)
        {
            Vector4 insets = safeAreaHandler.GetSafeAreaInsets();
            return insets.x;
        }
        return 0f;
    }
    
    private float GetRightSafeAreaOffset()
    {
        if (safeAreaHandler != null)
        {
            Vector4 insets = safeAreaHandler.GetSafeAreaInsets();
            return insets.z;
        }
        return 0f;
    }
    
    public bool IsPortrait()
    {
        return Screen.height > Screen.width;
    }
    
    public bool IsLandscape()
    {
        return Screen.width > Screen.height;
    }
    
    public ScreenOrientation GetCurrentOrientation()
    {
        return currentOrientation;
    }
    
    public Vector2 GetCurrentScreenSize()
    {
        return new Vector2(Screen.width, Screen.height);
    }
    
    public float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }
    
    // Public methods for runtime control
    public void SetOrientation(OrientationType orientation)
    {
        switch (orientation)
        {
            case OrientationType.Portrait:
                forcePortrait = true;
                forceLandscape = false;
                break;
            case OrientationType.Landscape:
                forcePortrait = false;
                forceLandscape = true;
                break;
            case OrientationType.Auto:
                forcePortrait = false;
                forceLandscape = false;
                break;
        }
        
        SetupOrientation();
        ApplyOrientationSettings();
    }
    
    public void SetAutoRotate(bool enabled)
    {
        autoRotate = enabled;
        if (enabled)
        {
            InvokeRepeating(nameof(CheckOrientationChange), rotationCheckInterval, rotationCheckInterval);
        }
        else
        {
            CancelInvoke(nameof(CheckOrientationChange));
        }
    }
    
    public void AddPortraitOnlyElement(GameObject element)
    {
        if (!portraitOnlyElements.Contains(element))
        {
            portraitOnlyElements.Add(element);
            ApplyOrientationSettings();
        }
    }
    
    public void AddLandscapeOnlyElement(GameObject element)
    {
        if (!landscapeOnlyElements.Contains(element))
        {
            landscapeOnlyElements.Add(element);
            ApplyOrientationSettings();
        }
    }
    
    public void AddAdaptiveElement(RectTransform element)
    {
        if (!adaptiveElements.Contains(element))
        {
            adaptiveElements.Add(element);
            ApplyOrientationSettings();
        }
    }
    
    public void RemovePortraitOnlyElement(GameObject element)
    {
        portraitOnlyElements.Remove(element);
    }
    
    public void RemoveLandscapeOnlyElement(GameObject element)
    {
        landscapeOnlyElements.Remove(element);
    }
    
    public void RemoveAdaptiveElement(RectTransform element)
    {
        adaptiveElements.Remove(element);
    }
    
    // Method to force refresh orientation
    public void RefreshOrientation()
    {
        ApplyOrientationSettings();
    }
    
    // Method to update backgrounds for orientation change
    private void UpdateBackgroundsForOrientation()
    {
        // Find all AdaptiveBackground components and update them
        AdaptiveBackground[] backgrounds = FindObjectsByType<AdaptiveBackground>(FindObjectsSortMode.None);
        foreach (var background in backgrounds)
        {
            if (background != null)
            {
                background.OnOrientationChanged();
            }
        }
        
        // Also update through AdaptiveUISystem if available
        if (AdaptiveUISystem.Instance != null)
        {
            AdaptiveUISystem.Instance.UpdateBackgroundsForOrientation();
        }
        
        Debug.Log($"Updated {backgrounds.Length} backgrounds for orientation change");
    }
    
    // Method to get optimal UI positioning for current orientation
    public Vector2 GetOptimalUIPosition(Vector2 basePosition, bool isTopUI = false, bool isBottomUI = false)
    {
        Vector2 adjustedPosition = basePosition;
        
        if (IsPortrait())
        {
            if (isTopUI)
            {
                adjustedPosition.y -= GetTopSafeAreaOffset();
            }
            else if (isBottomUI)
            {
                adjustedPosition.y += GetBottomSafeAreaOffset();
            }
        }
        else
        {
            // Landscape adjustments
            if (basePosition.x < Screen.width * 0.5f) // Left side
            {
                adjustedPosition.x += GetLeftSafeAreaOffset();
            }
            else // Right side
            {
                adjustedPosition.x -= GetRightSafeAreaOffset();
            }
        }
        
        return adjustedPosition;
    }
} 