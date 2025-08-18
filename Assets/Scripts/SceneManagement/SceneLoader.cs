using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void LoadGameplayScene()
    {
        AudioManager.CreateAudioManagerIfNeeded();
        SceneManager.LoadScene("GameplayScene");
        if (instance != null)
        {
            instance.StartCoroutine(instance.FixViewportAfterSceneLoad());
        }
    }

    public static void LoadLevelSelect()
    {
        AudioManager.CreateAudioManagerIfNeeded();
        SceneManager.LoadScene("LevelSelectScene");
        if (instance != null)
        {
            instance.StartCoroutine(instance.FixViewportAfterSceneLoad());
        }
    }

    public static void LoadMainMenu()
    {
        AudioManager.CreateAudioManagerIfNeeded();
        SceneManager.LoadScene("LogoScene");
        if (instance != null)
        {
            instance.StartCoroutine(instance.FixViewportAfterSceneLoad());
        }
    }
    
    private IEnumerator FixViewportAfterSceneLoad()
    {
        // Wait for scene to load
        yield return new WaitForSeconds(0.1f);
        
        // Fix camera viewport
        CameraViewportFixer.FixAllCameras();
        
        // Additional fix for main camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(0, 0, 1, 1);
            mainCamera.ResetAspect();
            Debug.Log($"SceneLoader: Fixed main camera viewport after scene load. Screen: {Screen.width}x{Screen.height}");
        }
    }
}
