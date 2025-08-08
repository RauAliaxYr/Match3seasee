using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LogoController : MonoBehaviour
{
    [SerializeField] private Image logoImage;
    [SerializeField] private CanvasGroup startScreenCanvasGroup;

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float showTime = 2f;

    private void Start()
    {
        // Set canvas to camera mode for logo scene
        if (AdaptiveUISystem.Instance != null)
        {
            AdaptiveUISystem.Instance.SetCanvasToCameraMode();
        }
        
        // Fix camera viewport to prevent image shifting
        StartCoroutine(FixViewportDelayed());
        
        logoImage.canvasRenderer.SetAlpha(0f);
        startScreenCanvasGroup.alpha = 0f;
        StartCoroutine(PlayLogoSequence());
    }
    
    private IEnumerator FixViewportDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(0, 0, 1, 1);
            mainCamera.ResetAspect();
            Debug.Log($"LogoController: Fixed camera viewport. Screen: {Screen.width}x{Screen.height}");
        }
    }

    private IEnumerator PlayLogoSequence()
    {
        logoImage.CrossFadeAlpha(1f, fadeDuration, false);
        yield return new WaitForSeconds(showTime);

        logoImage.CrossFadeAlpha(0f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration + 0.2f);

        startScreenCanvasGroup.alpha = 1f;
        
        // Start main menu music after logo animation completion
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuThemeIfNotPlaying();
            AudioManager.Instance.SetMusicVolume(0.7f);
        }
    }
}
