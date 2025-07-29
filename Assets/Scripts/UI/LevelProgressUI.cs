using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelProgressUI : MonoBehaviour
{
    [Header("Main UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Icons")]
    [SerializeField] private Image scoreIcon;
    [SerializeField] private Image movesIcon;
    [SerializeField] private Image timeIcon;
    
    [Header("Progress Bars")]
    [SerializeField] private Slider scoreProgressBar;
    [SerializeField] private Slider movesProgressBar;
    [SerializeField] private Slider timeProgressBar;
    
    [Header("Goal Panels")]
    [SerializeField] private GameObject goalPanel;
    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI targetMovesText;
    [SerializeField] private TextMeshProUGUI targetTimeText;
    
    [Header("Animations")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float scorePopupDuration = 0.5f;
    
    [Header("Effects")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Transform popupParent;
    
    // Cached values for animations
    private int lastScore = 0;
    private int lastMoves = 0;
    private float lastTime = 0f;
    
    // Colors for different states
    private Color normalColor = Color.white;
    private Color warningColor = new Color(1f, 0.8f, 0f, 1f); // Yellow
    private Color dangerColor = new Color(1f, 0.3f, 0.3f, 1f); // Red

    private void Start()
    {
        if (LevelProgressManager.Instance != null)
        {
            // Subscribe to events
            LevelProgressManager.Instance.OnScoreChanged += UpdateScore;
            LevelProgressManager.Instance.OnMovesChanged += UpdateMoves;
            LevelProgressManager.Instance.OnTimeChanged += UpdateTime;
            LevelProgressManager.Instance.OnLevelCompleted += OnLevelCompleted;

            // Initialize goal display
            InitializeGoalDisplay();
            InitializeProgressBars();
            
            // Update initial values
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

        // Hide all goal texts
        if (targetScoreText != null) targetScoreText.gameObject.SetActive(false);
        if (targetMovesText != null) targetMovesText.gameObject.SetActive(false);
        if (targetTimeText != null) targetTimeText.gameObject.SetActive(false);

        // Show only needed text depending on goal type
        switch (level.GoalType)
        {
            case LevelGoalType.Score:
                if (targetScoreText != null)
                {
                    targetScoreText.text = $"Goal: {level.TargetScore:N0}";
                    targetScoreText.gameObject.SetActive(true);
                }
                break;
                
            case LevelGoalType.MovesLimit:
                if (targetMovesText != null)
                {
                    targetMovesText.text = $"Moves: {level.MovesLimit}";
                    targetMovesText.gameObject.SetActive(true);
                }
                break;
                
            case LevelGoalType.TimeLimit:
                if (targetTimeText != null)
                {
                    targetTimeText.text = $"Time: {level.TimeLimitSeconds}s";
                    targetTimeText.gameObject.SetActive(true);
                }
                break;
                
            case LevelGoalType.ClearTiles:
                if (targetScoreText != null)
                {
                    targetScoreText.text = $"Clear: {level.TargetScore} tiles";
                    targetScoreText.gameObject.SetActive(true);
                }
                break;
        }
    }

    private void InitializeProgressBars()
    {
        var level = LevelProgressManager.Instance.CurrentLevel;
        if (level == null) return;

        // Configure progress bars depending on goal type
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
            // Text change animation
            StartCoroutine(AnimateValue(lastScore, score, (value) => {
                lastScore = (int)value;
                scoreText.text = $"Score: {lastScore:N0}";
            }));
        }

        // Update progress bar
        if (scoreProgressBar != null)
        {
            StartCoroutine(AnimateSlider(scoreProgressBar, score));
        }

        // Show popup when getting points
        if (score > lastScore && scorePopupPrefab != null && popupParent != null)
        {
            ShowScorePopup(score - lastScore);
        }

        // Change color when approaching goal
        UpdateProgressColor(scoreProgressBar, score, LevelProgressManager.Instance.CurrentLevel?.TargetScore ?? 0);
    }

    private void UpdateMoves(int movesMade)
    {
        var level = LevelProgressManager.Instance.CurrentLevel;
        int movesLimit = level?.MovesLimit ?? 0;
        int movesLeft = movesLimit > 0 ? Mathf.Max(0, movesLimit - movesMade) : 0;

        if (movesText != null)
        {
            movesText.text = movesLimit > 0
                ? $"Moves: {movesLeft}"
                : "Moves: ∞";
        }

        if (movesProgressBar != null)
        {
            if (movesLimit > 0)
                StartCoroutine(AnimateSlider(movesProgressBar, movesLeft));
            else
                movesProgressBar.value = movesProgressBar.maxValue;
        }

        UpdateProgressColor(movesProgressBar, movesLeft, movesLimit);
    }

    private void UpdateTime(float timeElapsed)
    {
        var level = LevelProgressManager.Instance.CurrentLevel;
        float timeLimit = level?.TimeLimitSeconds ?? 0f;
        float timeLeft = timeLimit > 0f ? Mathf.Max(0f, timeLimit - timeElapsed) : 0f;

        if (timeText != null)
        {
            if (timeLimit > 0f)
            {
                int minutes = Mathf.FloorToInt(timeLeft / 60f);
                int seconds = Mathf.FloorToInt(timeLeft % 60f);
                timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            else
            {
                timeText.text = "Time: ∞";
            }
        }

        if (timeProgressBar != null)
        {
            if (timeLimit > 0f)
                StartCoroutine(AnimateSlider(timeProgressBar, timeLeft));
            else
                timeProgressBar.value = timeProgressBar.maxValue;
        }

        UpdateProgressColor(timeProgressBar, timeLeft, timeLimit);
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

        // Now: the higher the ratio, the greener (normalColor)
        // Less than 0.4 — danger (red), 0.4-0.6 — warning (yellow), more than 0.6 — normal (green)
        if (ratio < 0.4f)
            targetColor = dangerColor;
        else if (ratio < 0.6f)
            targetColor = warningColor;
        else
            targetColor = normalColor;

        // Animate color change
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

        // Appearance and disappearance animation
        StartCoroutine(AnimatePopup(popup));
    }

    private IEnumerator AnimatePopup(GameObject popup)
    {
        // Initial state
        popup.transform.localScale = Vector3.zero;
        Vector3 startPos = popup.transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 100f;

        // Appearance animation
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

        // Move up animation
        elapsed = 0f;
        float moveDuration = scorePopupDuration * 0.7f;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            popup.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Disappearance animation
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
        // Level completion animation
        if (goalPanel != null)
        {
            StartCoroutine(AnimatePanelScale(goalPanel, Vector3.one * 1.2f, 0.2f, () => {
                StartCoroutine(AnimatePanelScale(goalPanel, Vector3.one, 0.1f));
            }));
        }

        Debug.Log($"Level completed! Stars: {result.StarsEarned}, Score: {result.Score}");
        
        // Level completion screen will be shown through LevelCompleteUI
        // Don't automatically go to level selection
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