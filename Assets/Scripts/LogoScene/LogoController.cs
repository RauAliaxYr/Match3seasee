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
        logoImage.canvasRenderer.SetAlpha(0f);
        startScreenCanvasGroup.alpha = 0f;
        StartCoroutine(PlayLogoSequence());
    }

    private IEnumerator PlayLogoSequence()
    {
        logoImage.CrossFadeAlpha(1f, fadeDuration, false);
        yield return new WaitForSeconds(showTime);

        logoImage.CrossFadeAlpha(0f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration + 0.2f);

        startScreenCanvasGroup.alpha = 1f;
    }
}
