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

    [Header("Геймплей")]
    [SerializeField] private BoardController boardController;
    [Header("База уровней")]
    [SerializeField] private LevelsDatabase levelsDatabase;
    
    private LevelResult currentResult;
    private bool isFrozen = false;
    private Coroutine currentStarAnimation;

    private void Start()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnLevelCompleted += ShowLevelComplete;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);
        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);

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
            Debug.Log($"ShowLevelComplete - StarsEarned: {result.StarsEarned}, IsCompleted: {result.IsCompleted}");
            StartCoroutine(AnimateStars(result.StarsEarned));
        }

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(result.IsCompleted);

        // Заморозка геймплея
        FreezeGameplay();
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
        Debug.Log($"AnimateStars called with starsEarned: {starsEarned}");
        Debug.Log($"starImages.Length: {starImages.Length}");
        
        // Сначала показываем все звезды как пустые
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) 
            {
                Debug.Log($"starImages[{i}] is null");
                continue;
            }
            starImages[i].sprite = starEmpty;
            starImages[i].transform.localScale = Vector3.one; // Все звезды видны по умолчанию
        }

        // Анимируем заполнение звезд
        for (int i = 0; i < starsEarned; i++)
        {
            if (starImages[i] == null) continue;
            
            float delay = i * starAnimationDelay;
            yield return new WaitForSeconds(delay);
            
            Debug.Log($"Animating star {i}, isLast: {i == starsEarned - 1}");
            
            // Если это последняя заработанная звезда, делаем её золотой
            if (i == starsEarned - 1)
            {
                Debug.Log($"Animating gold star {i} with vibration");
                starImages[i].sprite = starGold;
                currentStarAnimation = StartCoroutine(AnimateStarVibration(starImages[i].transform));
            }
            else
            {
                // Иначе делаем заполненной
                Debug.Log($"Animating filled star {i}");
                starImages[i].sprite = starFilled;
            }
        }
    }

    private IEnumerator AnimateStarScale(Transform starTransform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
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
        yield return StartCoroutine(AnimateStarScale(starTransform, pulseScale, 0.1f));
        yield return StartCoroutine(AnimateStarScale(starTransform, originalScale, 0.1f));
        yield return StartCoroutine(AnimateStarScale(starTransform, pulseScale, 0.1f));
        yield return StartCoroutine(AnimateStarScale(starTransform, originalScale, 0.1f));
    }

    private IEnumerator AnimateStarVibration(Transform starTransform)
    {
        Debug.Log("AnimateStarVibration started");
        
        Vector3 originalScale = starTransform.localScale;
        float pulseSpeed = 3f; // Скорость пульсации
        float pulseIntensity = 0.2f; // Интенсивность пульсации
        
        // Зацикленная пульсирующая анимация
        while (true)
        {
            float time = Time.unscaledTime * pulseSpeed;
            float scaleMultiplier = 1f + Mathf.Sin(time) * pulseIntensity;
            
            starTransform.localScale = originalScale * scaleMultiplier;
            
            yield return null;
        }
    }

    private IEnumerator AnimatePanelScale(GameObject panel, Vector3 targetScale, float duration)
    {
        Vector3 startScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        panel.transform.localScale = targetScale;
    }

    // --- ПРОФЕССИОНАЛЬНАЯ ЗАМОРОЗКА ГЕЙМПЛЕЯ ---
    public void FreezeGameplay()
    {
        if (isFrozen) return;
        isFrozen = true;
        Time.timeScale = 0f;
        if (boardController != null)
            boardController.BlockInput();
    }
    public void UnfreezeGameplay()
    {
        if (!isFrozen) return;
        isFrozen = false;
        Time.timeScale = 1f;
        if (boardController != null)
            boardController.UnblockInput();
    }

    public void RestartLevel()
    {
        // Останавливаем анимацию звезды
        if (currentStarAnimation != null)
        {
            StopCoroutine(currentStarAnimation);
            currentStarAnimation = null;
        }

        // Звук нажатия кнопки
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Останавливаем музыку победы/поражения
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        UnfreezeGameplay();

        // Сбросить прогресс уровня
        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.ResetLevelProgress();

        // Перегенерировать поле
        if (boardController != null)
        {
            var levelData = LevelProgressManager.Instance.CurrentLevel;
            var tileSize = boardController.TileSize;
            var tileSpacing = boardController.tileSpacing;
            boardController.RestartBoard(levelData, tileSize, tileSpacing);
        }

        // Скрыть Victory/Defeat панели
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        // Возвращаем музыку геймплея
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayTheme();
        }
    }

    public void NextLevel()
    {
        // Останавливаем анимацию звезды
        if (currentStarAnimation != null)
        {
            StopCoroutine(currentStarAnimation);
            currentStarAnimation = null;
        }

        // Звук нажатия кнопки
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Останавливаем музыку победы/поражения
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        UnfreezeGameplay();

        // Получаем следующий уровень
        int nextLevelId = LevelProgressManager.Instance.CurrentLevel.LevelId + 1;
        LevelGameplayData nextLevel = FindLevelById(nextLevelId);

        if (nextLevel != null)
        {
            GameManager.Instance.SetSelectedLevel(nextLevel);
            SceneLoader.LoadGameplayScene();
        }
        else
        {
            // Если следующего уровня нет — возвращаемся к выбору уровня
            SceneLoader.LoadLevelSelect();
        }
    }

    private LevelGameplayData FindLevelById(int levelId)
    {
        if (levelsDatabase != null)
            return levelsDatabase.GetLevelById(levelId);
        return null;
    }

    public void GoToMenu()
    {
        // Останавливаем анимацию звезды
        if (currentStarAnimation != null)
        {
            StopCoroutine(currentStarAnimation);
            currentStarAnimation = null;
        }

        // Звук нажатия кнопки
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Останавливаем музыку победы/поражения
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        UnfreezeGameplay();
        SceneLoader.LoadLevelSelect();
    }
} 