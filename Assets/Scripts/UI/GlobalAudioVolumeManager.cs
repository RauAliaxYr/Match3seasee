using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalAudioVolumeManager : MonoBehaviour
{
    [Header("Audio Volume UI Prefab")]
    [SerializeField] private GameObject audioVolumeUIPrefab;
    
    [Header("Settings")]
    [SerializeField] private bool createUIOnSceneLoad = true;
    [SerializeField] private bool persistBetweenScenes = true;
    
    private static GlobalAudioVolumeManager instance;
    private GameObject currentAudioVolumeUI;
    private Canvas cachedCanvas; // Cache the Canvas reference for performance
    
    public static GlobalAudioVolumeManager Instance => instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            
            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Create initial UI if needed
        if (createUIOnSceneLoad)
        {
            CreateAudioVolumeUI();
        }
        
        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from scene load events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clear cached Canvas when scene changes
        cachedCanvas = null;
        
        if (createUIOnSceneLoad)
        {
            // Wait a frame to ensure scene is fully loaded
            StartCoroutine(CreateAudioVolumeUIAfterDelay());
        }
        
        // Sync audio settings when scene changes
        StartCoroutine(SyncAudioSettingsAfterSceneLoad());
    }
    
    private System.Collections.IEnumerator CreateAudioVolumeUIAfterDelay()
    {
        yield return null;
        CreateAudioVolumeUI();
    }
    
    /// <summary>
    /// Syncs audio settings from PlayerPrefs to AudioManager after scene load
    /// </summary>
    private System.Collections.IEnumerator SyncAudioSettingsAfterSceneLoad()
    {
        // Wait a few frames to ensure AudioManager is fully initialized
        yield return null;
        yield return null;
        
        // Try to find AudioManager multiple times if needed
        int attempts = 0;
        while (AudioManager.Instance == null && attempts < 10)
        {
            yield return null;
            attempts++;
        }
        
        if (AudioManager.Instance != null)
        {
            // Force AudioManager to reload settings from PlayerPrefs
            AudioManager.Instance.ForceReloadSettings();
            Debug.Log("GlobalAudioVolumeManager: Synced audio settings with AudioManager after scene load");
        }
        else
        {
            Debug.LogWarning("GlobalAudioVolumeManager: AudioManager not found after multiple attempts during scene load sync");
        }
    }
    
    /// <summary>
    /// Syncs audio settings after UI creation
    /// </summary>
    private System.Collections.IEnumerator SyncAudioSettingsAfterUICreation()
    {
        // Wait a frame to ensure UI is fully initialized
        yield return null;
        
        // Also wait for AudioManager to be available
        int attempts = 0;
        while (AudioManager.Instance == null && attempts < 10)
        {
            yield return null;
            attempts++;
        }
        
        if (currentAudioVolumeUI != null)
        {
            AudioVolumeController controller = currentAudioVolumeUI.GetComponent<AudioVolumeController>();
            if (controller != null)
            {
                // Force reload settings to ensure UI shows correct values
                controller.ForceReloadSettings();
                Debug.Log("GlobalAudioVolumeManager: Synced audio settings with UI after creation");
            }
        }
        
        if (AudioManager.Instance != null)
        {
            Debug.Log("GlobalAudioVolumeManager: AudioManager is available for UI sync");
        }
        else
        {
            Debug.LogWarning("GlobalAudioVolumeManager: AudioManager not available for UI sync");
        }
    }
    
    /// <summary>
    /// Finds Canvas in the current scene using modern Unity methods
    /// </summary>
    private Canvas FindCanvasInScene()
    {
        // Check if we have a cached Canvas that's still valid
        if (cachedCanvas != null && cachedCanvas.gameObject != null && cachedCanvas.gameObject.scene == SceneManager.GetActiveScene())
        {
            Debug.Log($"GlobalAudioVolumeManager: Using cached Canvas: {cachedCanvas.name}");
            return cachedCanvas;
        }
        
        // Method 1: Try to find Canvas in the current scene's root objects
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        
        foreach (GameObject rootObject in rootObjects)
        {
            // Check if the root object itself is a Canvas
            Canvas rootCanvas = rootObject.GetComponent<Canvas>();
            if (rootCanvas != null)
            {
                cachedCanvas = rootCanvas; // Cache the found Canvas
                Debug.Log($"GlobalAudioVolumeManager: Found Canvas in root object: {rootObject.name}");
                return rootCanvas;
            }
            
            // Check for Canvas in children of root objects
            Canvas childCanvas = rootObject.GetComponentInChildren<Canvas>();
            if (childCanvas != null)
            {
                cachedCanvas = childCanvas; // Cache the found Canvas
                Debug.Log($"GlobalAudioVolumeManager: Found Canvas in child of root object: {rootObject.name}");
                return childCanvas;
            }
        }
        
        // Method 2: If no Canvas found in root objects, try to find any Canvas in the scene
        // This is a fallback method that searches more broadly
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            // Only consider canvases that are part of the current scene
            if (canvas.gameObject.scene == currentScene)
            {
                cachedCanvas = canvas; // Cache the found Canvas
                Debug.Log($"GlobalAudioVolumeManager: Found Canvas using fallback method: {canvas.name}");
                return canvas;
            }
        }
        
        Debug.LogWarning("GlobalAudioVolumeManager: No Canvas found in the current scene!");
        cachedCanvas = null; // Clear cache if no Canvas found
        return null;
    }
    
    public void CreateAudioVolumeUI()
    {
        // Skip creation on Logo scene
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "LogoScene")
        {
            if (currentAudioVolumeUI != null)
            {
                Destroy(currentAudioVolumeUI);
                currentAudioVolumeUI = null;
            }
            Debug.Log("GlobalAudioVolumeManager: Skipping audio UI creation on LogoScene");
            return;
        }

        // Destroy existing UI if any
        if (currentAudioVolumeUI != null)
        {
            Destroy(currentAudioVolumeUI);
        }
        
        // Find Canvas in current scene using modern approach
        Canvas canvas = FindCanvasInScene();
        if (canvas == null)
        {
            Debug.LogWarning("GlobalAudioVolumeManager: No Canvas found in scene!");
            return;
        }
        
        // Create UI from prefab
        if (audioVolumeUIPrefab != null)
        {
            // Check if prefab itself is active
            if (!audioVolumeUIPrefab.activeSelf)
            {
                Debug.LogWarning("GlobalAudioVolumeManager: Audio Volume UI Prefab is inactive! Activating it...");
                audioVolumeUIPrefab.SetActive(true);
            }
            
            currentAudioVolumeUI = Instantiate(audioVolumeUIPrefab, canvas.transform);
            
            // Ensure the instantiated UI is active
            if (currentAudioVolumeUI != null)
            {
                currentAudioVolumeUI.SetActive(true);
                
                // Also ensure all children are active
                foreach (Transform child in currentAudioVolumeUI.transform)
                {
                    child.gameObject.SetActive(true);
                }
                
                Debug.Log($"GlobalAudioVolumeManager: Created and activated audio volume UI in scene: {SceneManager.GetActiveScene().name}");
            }
            else
            {
                Debug.LogError("GlobalAudioVolumeManager: Failed to instantiate audio volume UI!");
            }
            
            // Position in bottom right corner
            RectTransform rectTransform = currentAudioVolumeUI.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                rectTransform.pivot = new Vector2(1, 0);
                rectTransform.anchoredPosition = new Vector2(-20, 20); // 20px from right and bottom
            }
        }
        else
        {
            Debug.LogError("GlobalAudioVolumeManager: Audio Volume UI Prefab is not assigned!");
        }
        
        // Sync audio settings after UI creation
        StartCoroutine(SyncAudioSettingsAfterUICreation());
    }
    
    public void DestroyCurrentAudioVolumeUI()
    {
        if (currentAudioVolumeUI != null)
        {
            Destroy(currentAudioVolumeUI);
            currentAudioVolumeUI = null;
        }
    }
    
    public void RefreshAudioVolumeUI()
    {
        if (currentAudioVolumeUI != null)
        {
            AudioVolumeController controller = currentAudioVolumeUI.GetComponent<AudioVolumeController>();
            if (controller != null)
            {
                controller.RefreshUI();
            }
        }
    }
    
    // Method to force reload all audio settings from PlayerPrefs
    public void ForceReloadAudioSettings()
    {
        if (currentAudioVolumeUI != null)
        {
            AudioVolumeController controller = currentAudioVolumeUI.GetComponent<AudioVolumeController>();
            if (controller != null)
            {
                controller.ForceReloadSettings();
            }
        }
        
        Debug.Log("GlobalAudioVolumeManager: Forced reload of all audio settings");
    }
    
    // Method to get current audio settings
    public (float musicVolume, float sfxVolume) GetCurrentAudioSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        return (musicVolume, sfxVolume);
    }
    
    public void ShowVolumePanel()
    {
        if (currentAudioVolumeUI != null)
        {
            AudioVolumeController controller = currentAudioVolumeUI.GetComponent<AudioVolumeController>();
            if (controller != null)
            {
                controller.ShowVolumePanelTemporarily();
            }
        }
    }
    
    public void HideVolumePanel()
    {
        if (currentAudioVolumeUI != null)
        {
            AudioVolumeController controller = currentAudioVolumeUI.GetComponent<AudioVolumeController>();
            if (controller != null)
            {
                controller.ForceHideVolumePanel();
            }
        }
    }
    
    // Public methods for external control
    public void SetCreateUIOnSceneLoad(bool enabled)
    {
        createUIOnSceneLoad = enabled;
    }
    
    public void ForceCreateUI()
    {
        CreateAudioVolumeUI();
    }
    
    public bool HasAudioVolumeUI()
    {
        return currentAudioVolumeUI != null;
    }
    
    public GameObject GetCurrentAudioVolumeUI()
    {
        return currentAudioVolumeUI;
    }
} 