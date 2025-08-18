using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AdaptiveLevelProgressUI : MonoBehaviour
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
    
    [Header("Adaptive UI Settings")]
    [SerializeField] private bool useAdaptiveUI = true;
    [SerializeField] private AdaptiveUIElement[] adaptiveElements;
    [SerializeField] private RectTransform[] dynamicElements;
    
    [Header("Mobile Layout")]
    [SerializeField] private Vector2 mobilePanelSize = new Vector2(300, 150);
    [SerializeField] private float mobileTextSize = 18f;
    [SerializeField] private Vector2 mobileIconSize = new Vector2(24, 24);
    
    [Header("Desktop Layout")]
    [SerializeField] private Vector2 desktopPanelSize = new Vector2(400, 200);
    [SerializeField] private float desktopTextSize = 24f;
    [SerializeField] private Vector2 desktopIconSize = new Vector2(32, 32);
    
    [Header("WebGL Layout")]
    [SerializeField] private Vector2 webGLPanelSize = new Vector2(350, 175);
    [SerializeField] private float webGLTextSize = 20f;
    [SerializeField] private Vector2 webGLIconSize = new Vector2(28, 28);
    
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
    
    // Adaptive UI components
    private AdaptiveUISystem adaptiveSystem;
    private RectTransform mainPanel;
    private bool isInitialized = false;

    private void Awake()
    {
        mainPanel = GetComponent<RectTransform>();
        adaptiveSystem = AdaptiveUISystem.Instance;
        
        // Setup adaptive elements if not assigned
        if (adaptiveElements == null || adaptiveElements.Length == 0)
        {
            SetupAdaptiveElements();
        }
    }

    private void Start()
    {
        if (LevelProgressManager.Instance != null)
        {
            // Subscribe to events
            LevelProgressManager.Instance.OnScoreChanged += UpdateScore;
            LevelProgressManager.Instance.OnScoreChangedWithPosition += UpdateScoreWithPosition;
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
        
        // Initialize adaptive UI
        if (useAdaptiveUI)
        {
            InitializeAdaptiveUI();
        }
    }

    private void OnDestroy()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnScoreChanged -= UpdateScore;
            LevelProgressManager.Instance.OnScoreChangedWithPosition -= UpdateScoreWithPosition;
            LevelProgressManager.Instance.OnMovesChanged -= UpdateMoves;
            LevelProgressManager.Instance.OnTimeChanged -= UpdateTime;
            LevelProgressManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        }
    }
    
    private void SetupAdaptiveElements()
    {
        // Create adaptive elements for text components
        if (scoreText != null && scoreText.GetComponent<AdaptiveUIElement>() == null)
        {
            AdaptiveUIElement adaptiveElement = scoreText.gameObject.AddComponent<AdaptiveUIElement>();
            adaptiveElement.ElementType = AdaptiveUIElement.UIElementType.Text;
        }
        
        if (movesText != null && movesText.GetComponent<AdaptiveUIElement>() == null)
        {
            AdaptiveUIElement adaptiveElement = movesText.gameObject.AddComponent<AdaptiveUIElement>();
            adaptiveElement.ElementType = AdaptiveUIElement.UIElementType.Text;
        }
        
        if (timeText != null && timeText.GetComponent<AdaptiveUIElement>() == null)
        {
            AdaptiveUIElement adaptiveElement = timeText.gameObject.AddComponent<AdaptiveUIElement>();
            adaptiveElement.ElementType = AdaptiveUIElement.UIElementType.Text;
        }
        
        // Create adaptive elements for icons
        if (scoreIcon != null && scoreIcon.GetComponent<AdaptiveUIElement>() == null)
        {
            AdaptiveUIElement adaptiveElement = scoreIcon.gameObject.AddComponent<AdaptiveUIElement>();
            adaptiveElement.ElementType = AdaptiveUIElement.UIElementType.Image;
        }
        
        if (movesIcon != null && movesIcon.GetComponent<AdaptiveUIElement>() == null)
        {
            AdaptiveUIElement adaptiveElement = movesIcon.gameObject.AddComponent<AdaptiveUIElement>();
            adaptiveElement.ElementType = AdaptiveUIElement.UIElementType.Image;
        }
        
        if (timeIcon != null && timeIcon.GetComponent<AdaptiveUIElement>() == null)
        {
            AdaptiveUIElement adaptiveElement = timeIcon.gameObject.AddComponent<AdaptiveUIElement>();
            adaptiveElement.ElementType = AdaptiveUIElement.UIElementType.Image;
        }
        
        // Find all adaptive elements
        adaptiveElements = GetComponentsInChildren<AdaptiveUIElement>();
    }
    
    private void InitializeAdaptiveUI()
    {
        if (isInitialized) return;
        
        // Apply adaptive settings
        ApplyAdaptiveLayout();
        
        // Register with adaptive system
        if (adaptiveSystem != null)
        {
            foreach (var element in adaptiveElements)
            {
                adaptiveSystem.AddAdaptiveElement(element);
            }
            
            foreach (var element in dynamicElements)
            {
                adaptiveSystem.AddDynamicElement(element);
            }
        }
        
        isInitialized = true;
    }
    
    private void ApplyAdaptiveLayout()
    {
        if (adaptiveSystem == null) return;
        
        Vector2 targetPanelSize;
        float targetTextSize;
        Vector2 targetIconSize;
        
        // Get target sizes based on platform
        if (adaptiveSystem.IsMobileDevice())
        {
            targetPanelSize = mobilePanelSize;
            targetTextSize = mobileTextSize;
            targetIconSize = mobileIconSize;
        }
        else if (adaptiveSystem.IsWebGLBuild())
        {
            targetPanelSize = webGLPanelSize;
            targetTextSize = webGLTextSize;
            targetIconSize = webGLIconSize;
        }
        else
        {
            targetPanelSize = desktopPanelSize;
            targetTextSize = desktopTextSize;
            targetIconSize = desktopIconSize;
        }
        
        // Apply panel size
        if (mainPanel != null)
        {
            mainPanel.sizeDelta = targetPanelSize;
        }
        
        // Apply text sizes
        if (scoreText != null) scoreText.fontSize = targetTextSize;
        if (movesText != null) movesText.fontSize = targetTextSize;
        if (timeText != null) timeText.fontSize = targetTextSize;
        if (targetScoreText != null) targetScoreText.fontSize = targetTextSize;
        if (targetMovesText != null) targetMovesText.fontSize = targetTextSize;
        if (targetTimeText != null) targetTimeText.fontSize = targetTextSize;
        
        // Apply icon sizes
        if (scoreIcon != null) scoreIcon.rectTransform.sizeDelta = targetIconSize;
        if (movesIcon != null) movesIcon.rectTransform.sizeDelta = targetIconSize;
        if (timeIcon != null) timeIcon.rectTransform.sizeDelta = targetIconSize;
        
        // Adjust layout based on orientation
        AdjustLayoutForOrientation();
    }
    
    private void AdjustLayoutForOrientation()
    {
        if (adaptiveSystem == null) return;
        
        bool isPortrait = adaptiveSystem.IsPortrait();
        
        // Adjust panel position based on orientation
        if (mainPanel != null)
        {
            Vector2 currentPosition = mainPanel.anchoredPosition;
            
            if (isPortrait)
            {
                // Portrait layout - position at top
                mainPanel.anchorMin = new Vector2(0.5f, 1f);
                mainPanel.anchorMax = new Vector2(0.5f, 1f);
                mainPanel.anchoredPosition = new Vector2(0, -50);
            }
            else
            {
                // Landscape layout - position at top center
                mainPanel.anchorMin = new Vector2(0.5f, 1f);
                mainPanel.anchorMax = new Vector2(0.5f, 1f);
                mainPanel.anchoredPosition = new Vector2(0, -30);
            }
        }
        
        // Adjust text alignment
        TextAlignmentOptions alignment = isPortrait ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;
        
        if (scoreText != null) scoreText.alignment = alignment;
        if (movesText != null) movesText.alignment = alignment;
        if (timeText != null) timeText.alignment = alignment;
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

        // Initialize progress bars based on goal type
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
                    movesProgressBar.value = level.MovesLimit;
                }
                break;
            case LevelGoalType.TimeLimit:
                if (timeProgressBar != null)
                {
                    timeProgressBar.maxValue = level.TimeLimitSeconds;
                    timeProgressBar.value = level.TimeLimitSeconds;
                }
                break;
            case LevelGoalType.ClearTiles:
                if (scoreProgressBar != null)
                {
                    scoreProgressBar.maxValue = level.TargetScore;
                    scoreProgressBar.value = 0;
                }
                break;
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            StartCoroutine(AnimateValue(lastScore, score, (value) => {
                scoreText.text = $"Score: {value:N0}";
            }));
        }

        if (scoreProgressBar != null)
        {
            var level = LevelProgressManager.Instance.CurrentLevel;
            if (level != null && level.GoalType == LevelGoalType.Score)
            {
                StartCoroutine(AnimateSlider(scoreProgressBar, score));
                UpdateProgressColor(scoreProgressBar, score, level.TargetScore);
            }
        }

        lastScore = score;
    }

    private void UpdateScoreWithPosition(int score, Vector3? matchPosition)
    {
        UpdateScore(score);
        
        if (matchPosition.HasValue)
        {
            int scoreGain = score - lastScore;
            if (scoreGain > 0)
            {
                ShowScorePopup(scoreGain, matchPosition);
            }
        }
    }

    private void UpdateMoves(int movesMade)
    {
        if (movesText != null)
        {
            StartCoroutine(AnimateValue(lastMoves, movesMade, (value) => {
                movesText.text = $"Moves: {value}";
            }));
        }

        if (movesProgressBar != null)
        {
            var level = LevelProgressManager.Instance.CurrentLevel;
            if (level != null && level.GoalType == LevelGoalType.MovesLimit)
            {
                float remainingMoves = level.MovesLimit - movesMade;
                StartCoroutine(AnimateSlider(movesProgressBar, remainingMoves));
                UpdateProgressColor(movesProgressBar, remainingMoves, level.MovesLimit);
            }
        }

        lastMoves = movesMade;
    }

    private void UpdateTime(float timeElapsed)
    {
        if (timeText != null)
        {
            StartCoroutine(AnimateValue(lastTime, timeElapsed, (value) => {
                timeText.text = $"Time: {value:F1}s";
            }));
        }

        if (timeProgressBar != null)
        {
            var level = LevelProgressManager.Instance.CurrentLevel;
            if (level != null && level.GoalType == LevelGoalType.TimeLimit)
            {
                float remainingTime = level.TimeLimitSeconds - timeElapsed;
                StartCoroutine(AnimateSlider(timeProgressBar, remainingTime));
                UpdateProgressColor(timeProgressBar, remainingTime, level.TimeLimitSeconds);
            }
        }

        lastTime = timeElapsed;
    }

    private IEnumerator AnimateValue(float from, float to, System.Action<float> onUpdate)
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float currentValue = Mathf.Lerp(from, to, progress);
            
            onUpdate?.Invoke(currentValue);
            yield return null;
        }
        
        onUpdate?.Invoke(to);
    }

    private IEnumerator AnimateSlider(Slider slider, float targetValue)
    {
        float startValue = slider.value;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            slider.value = Mathf.Lerp(startValue, targetValue, progress);
            yield return null;
        }
        
        slider.value = targetValue;
    }

    private void UpdateProgressColor(Slider progressBar, float current, float target)
    {
        if (progressBar == null) return;
        
        float percentage = current / target;
        Image fillImage = progressBar.fillRect?.GetComponent<Image>();
        
        if (fillImage != null)
        {
            Color targetColor;
            
            if (percentage > 0.6f)
                targetColor = normalColor;
            else if (percentage > 0.3f)
                targetColor = warningColor;
            else
                targetColor = dangerColor;
            
            StartCoroutine(AnimateColor(fillImage, fillImage.color, targetColor));
        }
    }

    private IEnumerator AnimateColor(Image image, Color from, Color to)
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            image.color = Color.Lerp(from, to, progress);
            yield return null;
        }
        
        image.color = to;
    }

    private void ShowScorePopup(int scoreGain, Vector3? matchPosition = null)
    {
        if (scorePopupPrefab == null || popupParent == null) return;
        
        GameObject popup = Instantiate(scorePopupPrefab, popupParent);
        
        // Set popup position
        if (matchPosition.HasValue)
        {
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(matchPosition.Value);
            popup.transform.position = screenPos;
        }
        else
        {
            // Position at center of screen
            popup.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        }
        
        // Set popup text
        TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (popupText != null)
        {
            popupText.text = $"+{scoreGain}";
        }
        
        StartCoroutine(AnimatePopup(popup));
    }

    private IEnumerator AnimatePopup(GameObject popup)
    {
        if (popup == null) yield break;
        
        RectTransform rectTransform = popup.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;
        
        // Initial scale
        rectTransform.localScale = Vector3.zero;
        
        // Scale up
        float elapsed = 0f;
        while (elapsed < scorePopupDuration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (scorePopupDuration * 0.3f);
            rectTransform.localScale = Vector3.one * progress;
            yield return null;
        }
        
        rectTransform.localScale = Vector3.one;
        
        // Wait
        yield return new WaitForSeconds(scorePopupDuration * 0.4f);
        
        // Scale down and fade out
        elapsed = 0f;
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popup.AddComponent<CanvasGroup>();
        }
        
        while (elapsed < scorePopupDuration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (scorePopupDuration * 0.3f);
            rectTransform.localScale = Vector3.one * (1f - progress * 0.5f);
            canvasGroup.alpha = 1f - progress;
            yield return null;
        }
        
        Destroy(popup);
    }

    private void OnLevelCompleted(LevelResult result)
    {
        // Handle level completion
        Debug.Log($"Level completed! Score: {result.Score}, Stars: {result.StarsEarned}");
    }
    
    // Public methods for runtime control
    public void RefreshAdaptiveLayout()
    {
        if (useAdaptiveUI)
        {
            ApplyAdaptiveLayout();
        }
    }
    
    public void SetAdaptiveUIEnabled(bool enabled)
    {
        useAdaptiveUI = enabled;
        if (enabled && !isInitialized)
        {
            InitializeAdaptiveUI();
        }
    }
    
    public void UpdateLayoutForOrientation()
    {
        AdjustLayoutForOrientation();
    }
} 