using UnityEngine;
using System.Collections;

public class SceneViewportManager : MonoBehaviour
{
    [Header("Auto Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool autoFixOnResolutionChange = true;
    [SerializeField] private float fixDelay = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private Camera mainCamera;
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("SceneViewportManager: Main camera not found!");
            return;
        }
        
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        if (autoFixOnStart)
        {
            StartCoroutine(FixViewportDelayed());
        }
    }
    
    private void Update()
    {
        if (!autoFixOnResolutionChange) return;
        
        // Check if screen resolution changed
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"SceneViewportManager: Resolution changed from {lastScreenWidth}x{lastScreenHeight} to {Screen.width}x{Screen.height}");
            }
            
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            
            StartCoroutine(FixViewportDelayed());
        }
    }
    
    private IEnumerator FixViewportDelayed()
    {
        yield return new WaitForSeconds(fixDelay);
        FixViewport();
    }
    
    public void FixViewport()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        // Reset viewport to full screen
        mainCamera.rect = new Rect(0, 0, 1, 1);
        
        // Force camera to update
        mainCamera.ResetAspect();
        
        if (enableDebugLogs)
        {
            Debug.Log($"SceneViewportManager: Fixed viewport. Screen: {Screen.width}x{Screen.height}, Camera rect: {mainCamera.rect}");
        }
    }
    
    // Public method to be called from other scripts
    public static void FixCurrentSceneViewport()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(0, 0, 1, 1);
            mainCamera.ResetAspect();
            Debug.Log($"SceneViewportManager: Fixed current scene viewport. Screen: {Screen.width}x{Screen.height}");
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && autoFixOnResolutionChange)
        {
            StartCoroutine(FixViewportDelayed());
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && autoFixOnResolutionChange)
        {
            StartCoroutine(FixViewportDelayed());
        }
    }
} 