using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    
    [Header("Результаты")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Звёзды")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starEmpty;
    [SerializeField] private Sprite starFilled;
    [SerializeField] private Sprite starGold;
    
    [Header("Кнопки")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button menuButton;
    
    [Header("Анимации")]
    [SerializeField] private float starAnimationDelay = 0.3f;
    [SerializeField] private float panelAnimationDuration = 0.5f;
    
    private LevelResult currentResult;

    private void Start()
    {
        // Подписываемся на событие завершения уровня
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnLevelCompleted += ShowLevelComplete;
        }

        // Настраиваем кнопки
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);
        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);

        // Скрываем панели
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnLevelCompleted -= ShowLevelComplete;
        }
    }

    private void ShowLevelComplete(LevelResult result)
    {
        currentResult = result;
        
        // Показываем соответствующую панель
        GameObject targetPanel = result.IsCompleted ? victoryPanel : defeatPanel;
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            StartCoroutine(AnimatePanelScale(targetPanel, Vector3.one, panelAnimationDuration));
        }

        // Обновляем информацию
        UpdateResultInfo(result);
        
        // Анимируем звёзды
        if (result.IsCompleted)
        {
            StartCoroutine(AnimateStars(result.StarsEarned));
        }

        // Показываем/скрываем кнопки
        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(result.IsCompleted);
    }

    private void UpdateResultInfo(LevelResult result)
    {
        if (levelNumberText != null)
            levelNumberText.text = $"Уровень {result.LevelId}";
        
        if (scoreText != null)
            scoreText.text = $"Очки: {result.Score:N0}";
        
        if (movesText != null)
            movesText.text = $"Ходы: {result.MovesUsed}";
        
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(result.TimeUsed / 60f);
            int seconds = Mathf.FloorToInt(result.TimeUsed % 60f);
            timeText.text = $"Время: {minutes:00}:{seconds:00}";
        }
    }

    private IEnumerator AnimateStars(int starsEarned)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            // Начинаем с пустых звёзд
            starImages[i].sprite = starEmpty;
            starImages[i].transform.localScale = Vector3.zero;

            // Анимируем появление
            float delay = i * starAnimationDelay;
            yield return new WaitForSeconds(delay);
            
            yield return StartCoroutine(AnimateStarScale(starImages[i].transform, Vector3.one, 0.3f));
            
            // Если звезда заработана, меняем на заполненную
            if (i < starsEarned)
            {
                starImages[i].sprite = starFilled;
                // Дополнительная анимация для золотых звёзд
                if (i == starsEarned - 1)
                {
                    starImages[i].sprite = starGold;
                    yield return StartCoroutine(AnimateStarPulse(starImages[i].transform));
                }
            }
        }
    }

    private IEnumerator AnimateStarScale(Transform starTransform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            starTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        starTransform.localScale = targetScale;
    }

    private IEnumerator AnimateStarPulse(Transform starTransform)
    {
        Vector3 originalScale = starTransform.localScale;
        Vector3 pulseScale = originalScale * 1.2f;
        
        // Пульсация
        yield return StartCoroutine(AnimateStarScale(starTransform, pulseScale, 0.1f));
        yield return StartCoroutine(AnimateStarScale(starTransform, originalScale, 0.1f));
        yield return StartCoroutine(AnimateStarScale(starTransform, pulseScale, 0.1f));
        yield return StartCoroutine(AnimateStarScale(starTransform, originalScale, 0.1f));
    }

    private IEnumerator AnimatePanelScale(GameObject panel, Vector3 targetScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        panel.transform.localScale = targetScale;
    }

    private void RestartLevel()
    {
        // Перезапускаем текущий уровень
        SceneLoader.LoadGameplayScene();
    }

    private void NextLevel()
    {
        // Здесь можно добавить логику для перехода к следующему уровню
        // Пока просто возвращаемся к выбору уровня
        SceneLoader.LoadLevelSelect();
    }

    private void GoToMenu()
    {
        SceneLoader.LoadLevelSelect();
    }
} 