using System;
using UnityEngine;
using GameData;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    [Header("Current Level")]
    [SerializeField] private LevelGameplayData currentLevel;

    // Goal progress
    private int currentScore = 0;
    private int movesMade = 0;
    private float timeElapsed = 0f;
    private int tilesCleared = 0;

    // Events for UI
    public event Action<int> OnScoreChanged;
    public event Action<int> OnMovesChanged;
    public event Action<float> OnTimeChanged;
    public event Action<int> OnTilesClearedChanged;
    public event Action<LevelResult> OnLevelCompleted;

    // Public properties
    public int CurrentScore => currentScore;
    public int MovesMade => movesMade;
    public float TimeElapsed => timeElapsed;
    public int TilesCleared => tilesCleared;
    public LevelGameplayData CurrentLevel => currentLevel;

    private bool isLevelActive = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // Removed for correct state reset
    }

    public void InitializeLevel(LevelGameplayData levelData)
    {
        currentLevel = levelData;
        ResetProgress();
        isLevelActive = true;
    }

    private void ResetProgress()
    {
        currentScore = 0;
        movesMade = 0;
        timeElapsed = 0f;
        tilesCleared = 0;
        
        OnScoreChanged?.Invoke(currentScore);
        OnMovesChanged?.Invoke(movesMade);
        OnTimeChanged?.Invoke(timeElapsed);
        OnTilesClearedChanged?.Invoke(tilesCleared);
    }

    private void Update()
    {
        if (!isLevelActive || currentLevel == null) return;

        // Always update time (for UI display)
        timeElapsed += Time.deltaTime;
        OnTimeChanged?.Invoke(timeElapsed);

        // Complete level only by time limit
        if (HasTimeLimit() && timeElapsed >= currentLevel.TimeLimitSeconds)
        {
            int stars = CalculateStars();
            CompleteLevel(stars > 0);
        }
    }

    // Called on successful swap
    public void OnMoveMade()
    {
        if (!isLevelActive) return;

        movesMade++;
        OnMovesChanged?.Invoke(movesMade);

        // Complete level only by moves limit
        if (HasMovesLimit() && movesMade >= currentLevel.MovesLimit)
        {
            int stars = CalculateStars();
            CompleteLevel(stars > 0);
        }
    }

    // Called when tiles are removed (matches)
    public void OnTilesMatched(int count, int matchSize = 3)
    {
        if (!isLevelActive) return;

        // Calculate score for match
        int baseScore = 10;
        int bonusMultiplier = Mathf.Max(1, matchSize - 2); // Bonus for large matches
        int scoreGain = count * baseScore * bonusMultiplier;

        currentScore += scoreGain;
        tilesCleared += count;

        OnScoreChanged?.Invoke(currentScore);
        OnTilesClearedChanged?.Invoke(tilesCleared);
    }

    // Determine number of stars
    private int CalculateStars()
    {
        if (currentLevel == null) return 0;
        int stars = 0;
        var thresholds = currentLevel.StarThresholds;
        if (currentScore >= thresholds[0]) stars = 1;
        if (currentScore >= thresholds[1]) stars = 2;
        if (currentScore >= thresholds[2]) stars = 3;
        return stars;
    }

    private void CompleteLevel(bool isVictory)
    {
        isLevelActive = false;
        
        int stars = CalculateStars();
        var result = new LevelResult
        {
            LevelId = currentLevel.LevelId,
            IsCompleted = isVictory,
            StarsEarned = stars,
            PreviouslyEarnedStars = 0, // Will be set after getting old progress
            Score = currentScore,
            MovesUsed = movesMade,
            TimeUsed = timeElapsed,
            TilesCleared = tilesCleared
        };

        // --- Update player progress ---
        // Get old progress before updating
        int oldStars = PlayerProgress.GetStars(result.LevelId);
        PlayerProgress.AddStars(result.LevelId, result.StarsEarned);
        
        // Store old progress in result for UI
        result.PreviouslyEarnedStars = oldStars;

        // Save progress (optional, if needed for analytics)
        SaveLevelProgress(result);
        
        // Start appropriate music
        if (AudioManager.Instance != null)
        {
            if (isVictory)
            {
                AudioManager.Instance.PlayVictoryTheme();
            }
            else
            {
                AudioManager.Instance.PlayLoseTheme();
            }
        }
        
        // Notify UI
        OnLevelCompleted?.Invoke(result);
    }

    private void SaveLevelProgress(LevelResult result)
    {
        string key = $"Level_{result.LevelId}";
        string json = JsonUtility.ToJson(result);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public void PauseLevel()
    {
        isLevelActive = false;
    }

    public void ResumeLevel()
    {
        isLevelActive = true;
    }

    public void ResetLevelProgress()
    {
        ResetProgress();
        isLevelActive = true;
    }

    // Helper methods for limits
    private bool HasTimeLimit() => currentLevel != null && currentLevel.TimeLimitSeconds > 0f;
    private bool HasMovesLimit() => currentLevel != null && currentLevel.MovesLimit > 0;
}

[System.Serializable]
public class LevelResult
{
    public int LevelId;
    public bool IsCompleted;
    public int StarsEarned;
    public int PreviouslyEarnedStars; // Stars earned before this attempt
    public int Score;
    public int MovesUsed;
    public float TimeUsed;
    public int TilesCleared;
} 