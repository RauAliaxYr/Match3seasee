using UnityEngine;
using TMPro;
using System.Collections;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float moveDistance = 50f;

    private void Start()
    {
        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        // Начальное состояние
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * moveDistance;

        // Анимация появления
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Анимация движения вверх
        elapsed = 0f;
        float totalDuration = fadeInDuration + fadeOutDuration;
        
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalDuration;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Анимация исчезновения
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"+{score}";
        }
    }
} 