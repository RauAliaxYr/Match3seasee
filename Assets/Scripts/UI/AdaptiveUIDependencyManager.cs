using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages dependencies between adaptive UI components without using FindObjectOfType.
/// This script should be placed on a GameObject that contains all adaptive UI components.
/// </summary>
public class AdaptiveUIDependencyManager : MonoBehaviour
{
    [Header("System Components")]
    [SerializeField] private AdaptiveUISystem adaptiveUISystem;
    [SerializeField] private AdaptiveUIManager adaptiveUIManager;
    [SerializeField] private SafeAreaHandler safeAreaHandler;
    [SerializeField] private OrientationManager orientationManager;
    [SerializeField] private PlatformSettings platformSettings;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool autoFindComponents = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private List<AdaptiveUIElement> adaptiveElements = new List<AdaptiveUIElement>();
    
    private void Awake()
    {
        if (autoFindComponents)
        {
            FindComponents();
        }
    }
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupDependencies();
        }
    }
    
    private void FindComponents()
    {
        // Find components in children first
        if (adaptiveUISystem == null)
            adaptiveUISystem = GetComponentInChildren<AdaptiveUISystem>();
            
        if (adaptiveUIManager == null)
            adaptiveUIManager = GetComponentInChildren<AdaptiveUIManager>();
            
        if (safeAreaHandler == null)
            safeAreaHandler = GetComponentInChildren<SafeAreaHandler>();
            
        if (orientationManager == null)
            orientationManager = GetComponentInChildren<OrientationManager>();
            
        if (platformSettings == null)
            platformSettings = GetComponentInChildren<PlatformSettings>();
            
        // If not found in children, try to find in parent
        if (adaptiveUISystem == null)
            adaptiveUISystem = GetComponentInParent<AdaptiveUISystem>();
            
        if (adaptiveUIManager == null)
            adaptiveUIManager = GetComponentInParent<AdaptiveUIManager>();
            
        if (safeAreaHandler == null)
            safeAreaHandler = GetComponentInParent<SafeAreaHandler>();
            
        if (orientationManager == null)
            orientationManager = GetComponentInParent<OrientationManager>();
            
        if (platformSettings == null)
            platformSettings = GetComponentInParent<PlatformSettings>();
            
        // Find all adaptive elements
        AdaptiveUIElement[] elements = GetComponentsInChildren<AdaptiveUIElement>();
        adaptiveElements.Clear();
        adaptiveElements.AddRange(elements);
        
        if (showDebugLogs)
        {
            LogComponentStatus();
        }
    }
    
    private void SetupDependencies()
    {
        if (showDebugLogs)
        {
            Debug.Log("Setting up Adaptive UI Dependencies...");
        }
        
        // Setup AdaptiveUISystem dependencies
        if (adaptiveUISystem != null)
        {
            if (adaptiveUIManager != null)
                adaptiveUISystem.SetAdaptiveUIManager(adaptiveUIManager);
                
            if (safeAreaHandler != null)
                adaptiveUISystem.SetSafeAreaHandler(safeAreaHandler);
                
            if (orientationManager != null)
                adaptiveUISystem.SetOrientationManager(orientationManager);
                
            if (platformSettings != null)
                adaptiveUISystem.SetPlatformSettings(platformSettings);
        }
        
        // Setup AdaptiveUIElement dependencies
        foreach (var element in adaptiveElements)
        {
            if (element != null)
            {
                if (adaptiveUISystem != null)
                    element.SetAdaptiveUISystem(adaptiveUISystem);
                    
                if (adaptiveUIManager != null)
                    element.SetAdaptiveUIManager(adaptiveUIManager);
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Dependencies setup complete. Found {adaptiveElements.Count} adaptive elements.");
        }
    }
    
    private void LogComponentStatus()
    {
        Debug.Log("=== Adaptive UI Component Status ===");
        Debug.Log($"AdaptiveUISystem: {(adaptiveUISystem != null ? "Found" : "Missing")}");
        Debug.Log($"AdaptiveUIManager: {(adaptiveUIManager != null ? "Found" : "Missing")}");
        Debug.Log($"SafeAreaHandler: {(safeAreaHandler != null ? "Found" : "Missing")}");
        Debug.Log($"OrientationManager: {(orientationManager != null ? "Found" : "Missing")}");
        Debug.Log($"PlatformSettings: {(platformSettings != null ? "Found" : "Missing")}");
        Debug.Log($"AdaptiveUIElements: {adaptiveElements.Count} found");
        Debug.Log("=====================================");
    }
    
    // Public methods for manual setup
    public void SetAdaptiveUISystem(AdaptiveUISystem system)
    {
        adaptiveUISystem = system;
        SetupDependencies();
    }
    
    public void SetAdaptiveUIManager(AdaptiveUIManager manager)
    {
        adaptiveUIManager = manager;
        SetupDependencies();
    }
    
    public void SetSafeAreaHandler(SafeAreaHandler handler)
    {
        safeAreaHandler = handler;
        SetupDependencies();
    }
    
    public void SetOrientationManager(OrientationManager manager)
    {
        orientationManager = manager;
        SetupDependencies();
    }
    
    public void SetPlatformSettings(PlatformSettings settings)
    {
        platformSettings = settings;
        SetupDependencies();
    }
    
    public void RefreshDependencies()
    {
        FindComponents();
        SetupDependencies();
    }
    
    public void AddAdaptiveElement(AdaptiveUIElement element)
    {
        if (!adaptiveElements.Contains(element))
        {
            adaptiveElements.Add(element);
            
            // Setup dependencies for the new element
            if (adaptiveUISystem != null)
                element.SetAdaptiveUISystem(adaptiveUISystem);
                
            if (adaptiveUIManager != null)
                element.SetAdaptiveUIManager(adaptiveUIManager);
        }
    }
    
    public void RemoveAdaptiveElement(AdaptiveUIElement element)
    {
        adaptiveElements.Remove(element);
    }
    
    // Validation methods
    public bool ValidateDependencies()
    {
        bool isValid = true;
        
        if (adaptiveUISystem == null)
        {
            Debug.LogWarning("AdaptiveUISystem is missing!");
            isValid = false;
        }
        
        if (adaptiveUIManager == null)
        {
            Debug.LogWarning("AdaptiveUIManager is missing!");
            isValid = false;
        }
        
        if (safeAreaHandler == null)
        {
            Debug.LogWarning("SafeAreaHandler is missing!");
            isValid = false;
        }
        
        if (orientationManager == null)
        {
            Debug.LogWarning("OrientationManager is missing!");
            isValid = false;
        }
        
        if (platformSettings == null)
        {
            Debug.LogWarning("PlatformSettings is missing!");
            isValid = false;
        }
        
        if (adaptiveElements.Count == 0)
        {
            Debug.LogWarning("No AdaptiveUIElements found!");
            isValid = false;
        }
        
        return isValid;
    }
    
    // Editor helper methods
    [ContextMenu("Setup Dependencies")]
    private void SetupDependenciesContextMenu()
    {
        FindComponents();
        SetupDependencies();
    }
    
    [ContextMenu("Validate Dependencies")]
    private void ValidateDependenciesContextMenu()
    {
        ValidateDependencies();
    }
    
    [ContextMenu("Log Component Status")]
    private void LogComponentStatusContextMenu()
    {
        LogComponentStatus();
    }
} 