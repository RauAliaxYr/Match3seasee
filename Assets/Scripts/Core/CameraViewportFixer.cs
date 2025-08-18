using UnityEngine;
using System.Collections;

public class CameraViewportFixer : MonoBehaviour
{
    [Header("Viewport Settings")]
    [SerializeField] private bool fixViewportOnStart = true;
    [SerializeField] private bool fixViewportOnResolutionChange = true;
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
            Debug.LogError("CameraViewportFixer: Main camera not found!");
            return;
        }
        
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        if (fixViewportOnStart)
        {
            StartCoroutine(FixViewportDelayed());
        }
    }
    
    private void Update()
    {
        if (!fixViewportOnResolutionChange) return;
        
        // Check if screen resolution changed
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"CameraViewportFixer: Resolution changed from {lastScreenWidth}x{lastScreenHeight} to {Screen.width}x{Screen.height}");
            }
            
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            
            StartCoroutine(FixViewportDelayed());
        }
    }
    
    private IEnumerator FixViewportDelayed()
    {
        yield return new WaitForSeconds(fixDelay);
        FixCameraViewport();
    }
    
    public void FixCameraViewport()
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
            Debug.Log($"CameraViewportFixer: Fixed camera viewport. Screen: {Screen.width}x{Screen.height}, Camera rect: {mainCamera.rect}");
        }
    }
    
    public void FixCameraViewportImmediate()
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
            Debug.Log($"CameraViewportFixer: Fixed camera viewport immediately. Screen: {Screen.width}x{Screen.height}, Camera rect: {mainCamera.rect}");
        }
    }
    
    // Public method to be called from other scripts
    public static void FixAllCameras()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var camera in cameras)
        {
            camera.rect = new Rect(0, 0, 1, 1);
            camera.ResetAspect();
        }
        
        Debug.Log($"CameraViewportFixer: Fixed {cameras.Length} cameras");
    }
    
    // Method to handle fullscreen toggle
    public void OnFullscreenToggle()
    {
        StartCoroutine(FixViewportDelayed());
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && fixViewportOnResolutionChange)
        {
            StartCoroutine(FixViewportDelayed());
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && fixViewportOnResolutionChange)
        {
            StartCoroutine(FixViewportDelayed());
        }
    }
} 