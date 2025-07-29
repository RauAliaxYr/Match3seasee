using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    
    [Header("Results")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Stars")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starEmpty;
    [SerializeField] private Sprite starFilled;
    [SerializeField] private Sprite starGold;
    
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button menuButton;
    
    [Header("Animations")]
    [SerializeField] private float starAnimationDelay = 0.3f;
    [SerializeField] private float panelAnimationDuration = 0.5f;

    [Header("Gameplay")]
    [SerializeField] private BoardController boardController;
    [Header("Level Database")]
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
        
        // Show appropriate panel
        GameObject targetPanel = result.IsCompleted ? victoryPanel : defeatPanel;
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            StartCoroutine(AnimatePanelScale(targetPanel, Vector3.one, panelAnimationDuration));
        }

        // Update information
        UpdateResultInfo(result);
        
        // Animate stars
        if (result.IsCompleted)
        {
            Debug.Log($"ShowLevelComplete - StarsEarned: {result.StarsEarned}, IsCompleted: {result.IsCompleted}");
            StartCoroutine(AnimateStars(result.StarsEarned));
        }

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(result.IsCompleted);

        // Freeze gameplay
        FreezeGameplay();
    }

    private void UpdateResultInfo(LevelResult result)
    {
        if (levelNumberText != null)
            levelNumberText.text = $"Level {result.LevelId}";
        if (scoreText != null)
            scoreText.text = $"Score: {result.Score:N0}";
        if (movesText != null)
            movesText.text = $"Moves: {result.MovesUsed}";
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(result.TimeUsed / 60f);
            int seconds = Mathf.FloorToInt(result.TimeUsed % 60f);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    private IEnumerator AnimateStars(int starsEarned)
    {
        Debug.Log($"AnimateStars called with starsEarned: {starsEarned}");
        Debug.Log($"starImages.Length: {starImages.Length}");
        
        // First show all stars as empty
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) 
            {
                Debug.Log($"starImages[{i}] is null");
                continue;
            }
            starImages[i].sprite = starEmpty;
            starImages[i].transform.localScale = Vector3.one; // All stars visible by default
        }

        // Animate star filling
        for (int i = 0; i < starsEarned; i++)
        {
            if (starImages[i] == null) continue;
            
            float delay = i * starAnimationDelay;
            yield return new WaitForSeconds(delay);
            
            Debug.Log($"Animating star {i}, isLast: {i == starsEarned - 1}");
            
            // If this is the last earned star, make it gold
            if (i == starsEarned - 1)
            {
                Debug.Log($"Animating gold star {i} with vibration");
                starImages[i].sprite = starGold;
                currentStarAnimation = StartCoroutine(AnimateStarVibration(starImages[i].transform));
            }
            else
            {
                // Otherwise make it filled
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
        float pulseSpeed = 3f; // Pulse speed
        float pulseIntensity = 0.2f; // Pulse intensity
        
        // Looped pulsing animation
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

    // --- GAMEPLAY FREEZING ---
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
        // Stop star animation
        if (currentStarAnimation != null)
        {
            StopCoroutine(currentStarAnimation);
            currentStarAnimation = null;
        }

        // Button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Stop victory/defeat music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        UnfreezeGameplay();

        // Reset level progress
        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.ResetLevelProgress();

        // Regenerate board
        if (boardController != null)
        {
            var levelData = LevelProgressManager.Instance.CurrentLevel;
            var tileSize = boardController.TileSize;
            var tileSpacing = boardController.tileSpacing;
            boardController.RestartBoard(levelData, tileSize, tileSpacing);
        }

        // Hide Victory/Defeat panels
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        // Return gameplay music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayTheme();
        }
    }

    public void NextLevel()
    {
        // Stop star animation
        if (currentStarAnimation != null)
        {
            StopCoroutine(currentStarAnimation);
            currentStarAnimation = null;
        }

        // Button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Stop victory/defeat music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        UnfreezeGameplay();

        // Get next level
        int nextLevelId = LevelProgressManager.Instance.CurrentLevel.LevelId + 1;
        LevelGameplayData nextLevel = FindLevelById(nextLevelId);

        if (nextLevel != null)
        {
            GameManager.Instance.SetSelectedLevel(nextLevel);
            SceneLoader.LoadGameplayScene();
        }
        else
        {
            // If no next level - return to level selection
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
        // Stop star animation
        if (currentStarAnimation != null)
        {
            StopCoroutine(currentStarAnimation);
            currentStarAnimation = null;
        }

        // Button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Stop victory/defeat music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        UnfreezeGameplay();
        SceneLoader.LoadLevelSelect();
    }
} 