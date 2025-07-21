using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelProgressUI : MonoBehaviour
{
    [Header("Основные UI элементы")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Иконки")]
    [SerializeField] private Image scoreIcon;
    [SerializeField] private Image movesIcon;
    [SerializeField] private Image timeIcon;
    
    [Header("Прогресс-бары")]
    [SerializeField] private Slider scoreProgressBar;
    [SerializeField] private Slider movesProgressBar;
    [SerializeField] private Slider timeProgressBar;
    
    [Header("Панели целей")]
    [SerializeField] private GameObject goalPanel;
    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI targetMovesText;
    [SerializeField] private TextMeshProUGUI targetTimeText;
    
    [Header("Анимации")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float scorePopupDuration = 0.5f;
    
    [Header("Эффекты")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Transform popupParent;
    
    // Кэшированные значения для анимаций
    private int lastScore = 0;
    private int lastMoves = 0;
    private float lastTime = 0f;
    
    // Цвета для разных состояний
    private Color normalColor = Color.white;
    private Color warningColor = new Color(1f, 0.8f, 0f, 1f); // Жёлтый
    private Color dangerColor = new Color(1f, 0.3f, 0.3f, 1f); // Красный

    private void Start()
    {
        if (LevelProgressManager.Instance != null)
        {
            // Подписываемся на события
            LevelProgressManager.Instance.OnScoreChanged += UpdateScore;
            LevelProgressManager.Instance.OnMovesChanged += UpdateMoves;
            LevelProgressManager.Instance.OnTimeChanged += UpdateTime;
            LevelProgressManager.Instance.OnLevelCompleted += OnLevelCompleted;

            // Инициализируем отображение целей
            InitializeGoalDisplay();
            InitializeProgressBars();
            
            // Обновляем начальные значения
            UpdateScore(LevelProgressManager.Instance.CurrentScore);
            UpdateMoves(LevelProgressManager.Instance.MovesMade);
            UpdateTime(LevelProgressManager.Instance.TimeElapsed);
        }
    }

    private void OnDestroy()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnScoreChanged -= UpdateScore;
            LevelProgressManager.Instance.OnMovesChanged -= UpdateMoves;
            LevelProgressManager.Instance.OnTimeChanged -= UpdateTime;
            LevelProgressManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        }
    }

    private void InitializeGoalDisplay()
    {
        var level = LevelProgressManager.Instance.CurrentLevel;
        if (level == null) return;

        // Скрываем все тексты целей
        if (targetScoreText != null) targetScoreText.gameObject.SetActive(false);
        if (targetMovesText != null) targetMovesText.gameObject.SetActive(false);
        if (targetTimeText != null) targetTimeText.gameObject.SetActive(false);

        // Показываем только нужный текст в зависимости от типа цели
        switch (level.GoalType)
        {
            case LevelGoalType.Score:
                if (targetScoreText != null)
                {
                    targetScoreText.text = $"Цель: {level.TargetScore:N0}";
                    targetScoreText.gameObject.SetActive(true);
                }
                break;
                
            case LevelGoalType.MovesLimit:
                if (targetMovesText != null)
                {
                    targetMovesText.text = $"Ходов: {level.MovesLimit}";
                    targetMovesText.gameObject.SetActive(true);
                }
                break;
                
            case LevelGoalType.TimeLimit:
                if (targetTimeText != null)
                {
                    targetTimeText.text = $"Время: {level.TimeLimitSeconds}s";
                    targetTimeText.gameObject.SetActive(true);
                }
                break;
                
            case LevelGoalType.ClearTiles:
                if (targetScoreText != null)
                {
                    targetScoreText.text = $"Очистить: {level.TargetScore} тайлов";
                    targetScoreText.gameObject.SetActive(true);
                }
                break;
        }
    }

    private void InitializeProgressBars()
    {
        var level = LevelProgressManager.Instance.CurrentLevel;
        if (level == null) return;

        // Настраиваем прогресс-бары в зависимости от типа цели
        switch (level.GoalType)
        {
            case LevelGoalType.Score:
                if (scoreProgressBar != null)
                {
                    scoreProgressBar.maxValue = level.TargetScore;
                    scoreProgressBar.value = 0;
                }
                break;
                
            case LevelGoalType.MovesLimit:
                if (movesProgressBar != null)
                {
                    movesProgressBar.maxValue = level.MovesLimit;
                    movesProgressBar.value = 0;
                }
                break;
                
            case LevelGoalType.TimeLimit:
                if (timeProgressBar != null)
                {
                    timeProgressBar.maxValue = level.TimeLimitSeconds;
                    timeProgressBar.value = 0;
                }
                break;
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            // Анимация изменения текста
            StartCoroutine(AnimateValue(lastScore, score, (value) => {
                lastScore = (int)value;
                scoreText.text = $"Очки: {lastScore:N0}";
            }));
        }

        // Обновляем прогресс-бар
        if (scoreProgressBar != null)
        {
            StartCoroutine(AnimateSlider(scoreProgressBar, score));
        }

        // Показываем popup при получении очков
        if (score > lastScore && scorePopupPrefab != null && popupParent != null)
        {
            ShowScorePopup(score - lastScore);
        }

        // Изменяем цвет при приближении к цели
        UpdateProgressColor(scoreProgressBar, score, LevelProgressManager.Instance.CurrentLevel?.TargetScore ?? 0);
    }

    private void UpdateMoves(int moves)
    {
        if (movesText != null)
        {
            StartCoroutine(AnimateValue(lastMoves, moves, (value) => {
                lastMoves = (int)value;
                movesText.text = $"Ходы: {lastMoves}";
            }));
        }

        if (movesProgressBar != null)
        {
            StartCoroutine(AnimateSlider(movesProgressBar, moves));
        }

        UpdateProgressColor(movesProgressBar, moves, LevelProgressManager.Instance.CurrentLevel?.MovesLimit ?? 0);
    }

    private void UpdateTime(float time)
    {
        if (timeText != null)
        {
            StartCoroutine(AnimateValue(lastTime, time, (value) => {
                lastTime = value;
                int minutes = Mathf.FloorToInt(lastTime / 60f);
                int seconds = Mathf.FloorToInt(lastTime % 60f);
                timeText.text = $"Время: {minutes:00}:{seconds:00}";
            }));
        }

        if (timeProgressBar != null)
        {
            StartCoroutine(AnimateSlider(timeProgressBar, time));
        }

        UpdateProgressColor(timeProgressBar, time, LevelProgressManager.Instance.CurrentLevel?.TimeLimitSeconds ?? 0);
    }

    private IEnumerator AnimateValue(float from, float to, System.Action<float> onUpdate)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float currentValue = Mathf.Lerp(from, to, t);
            onUpdate(currentValue);
            yield return null;
        }
        onUpdate(to);
    }

    private IEnumerator AnimateSlider(Slider slider, float targetValue)
    {
        float startValue = slider.value;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            slider.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }
        slider.value = targetValue;
    }

    private void UpdateProgressColor(Slider progressBar, float current, float target)
    {
        if (progressBar == null || target <= 0) return;

        float ratio = current / target;
        Color targetColor = normalColor;

        if (ratio >= 0.8f)
            targetColor = dangerColor;
        else if (ratio >= 0.6f)
            targetColor = warningColor;

        // Анимируем изменение цвета
        var fillImage = progressBar.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            StartCoroutine(AnimateColor(fillImage, fillImage.color, targetColor));
        }
    }

    private IEnumerator AnimateColor(Image image, Color from, Color to)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            image.color = Color.Lerp(from, to, t);
            yield return null;
        }
        image.color = to;
    }

    private void ShowScorePopup(int scoreGain)
    {
        if (scorePopupPrefab == null || popupParent == null) return;

        GameObject popup = Instantiate(scorePopupPrefab, popupParent);
        TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
        
        if (popupText != null)
        {
            popupText.text = $"+{scoreGain}";
        }

        // Анимация появления и исчезновения
        StartCoroutine(AnimatePopup(popup));
    }

    private IEnumerator AnimatePopup(GameObject popup)
    {
        // Начальное состояние
        popup.transform.localScale = Vector3.zero;
        Vector3 startPos = popup.transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 100f;

        // Анимация появления
        float elapsed = 0f;
        float appearDuration = scorePopupDuration * 0.3f;
        
        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / appearDuration;
            popup.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
        popup.transform.localScale = Vector3.one;

        // Анимация движения вверх
        elapsed = 0f;
        float moveDuration = scorePopupDuration * 0.7f;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            popup.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Анимация исчезновения
        elapsed = 0f;
        float disappearDuration = scorePopupDuration * 0.3f;
        
        while (elapsed < disappearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / disappearDuration;
            popup.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        Destroy(popup);
    }

    private void OnLevelCompleted(LevelResult result)
    {
        // Анимация завершения уровня
        if (goalPanel != null)
        {
            StartCoroutine(AnimatePanelScale(goalPanel, Vector3.one * 1.2f, 0.2f, () => {
                StartCoroutine(AnimatePanelScale(goalPanel, Vector3.one, 0.1f));
            }));
        }

        Debug.Log($"Уровень завершён! Звёзд: {result.StarsEarned}, Очки: {result.Score}");
        
        // Экран завершения уровня будет показан через LevelCompleteUI
        // Не переходим автоматически к выбору уровня
    }

    private IEnumerator AnimatePanelScale(GameObject panel, Vector3 targetScale, float duration, System.Action onComplete = null)
    {
        Vector3 startScale = panel.transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        panel.transform.localScale = targetScale;
        onComplete?.Invoke();
    }
} 