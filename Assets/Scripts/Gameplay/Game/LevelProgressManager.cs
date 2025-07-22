using System;
using UnityEngine;
using GameData;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    [Header("Текущий уровень")]
    [SerializeField] private LevelGameplayData currentLevel;

    // Прогресс по целям
    private int currentScore = 0;
    private int movesMade = 0;
    private float timeElapsed = 0f;
    private int tilesCleared = 0;

    // События для UI
    public event Action<int> OnScoreChanged;
    public event Action<int> OnMovesChanged;
    public event Action<float> OnTimeChanged;
    public event Action<int> OnTilesClearedChanged;
    public event Action<LevelResult> OnLevelCompleted;

    // Публичные свойства
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
        // DontDestroyOnLoad(gameObject); // Удалено для корректного сброса состояния
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

        // Обновляем время всегда (для отображения в UI)
        timeElapsed += Time.deltaTime;
        OnTimeChanged?.Invoke(timeElapsed);

        // Проверяем лимит времени только для уровней с временным лимитом
        if (currentLevel.GoalType == LevelGoalType.TimeLimit)
        {
            if (timeElapsed >= currentLevel.TimeLimitSeconds)
            {
                CompleteLevel(false);
            }
        }
    }

    // Вызывается при успешном свапе
    public void OnMoveMade()
    {
        if (!isLevelActive) return;

        movesMade++;
        OnMovesChanged?.Invoke(movesMade);

        // Проверяем лимит ходов
        if (currentLevel.GoalType == LevelGoalType.MovesLimit && 
            currentLevel.MovesLimit > 0 && 
            movesMade >= currentLevel.MovesLimit)
        {
            CompleteLevel(false);
        }
    }

    // Вызывается при удалении тайлов (мэтчи)
    public void OnTilesMatched(int count, int matchSize = 3)
    {
        if (!isLevelActive) return;

        // Подсчёт очков за мэтч
        int baseScore = 10;
        int bonusMultiplier = Mathf.Max(1, matchSize - 2); // Бонус за большие мэтчи
        int scoreGain = count * baseScore * bonusMultiplier;

        currentScore += scoreGain;
        tilesCleared += count;

        OnScoreChanged?.Invoke(currentScore);
        OnTilesClearedChanged?.Invoke(tilesCleared);

        // Проверяем достижение цели по очкам
        if (currentLevel.GoalType == LevelGoalType.Score && 
            currentScore >= currentLevel.TargetScore)
        {
            CompleteLevel(true);
        }
    }

    // Определение количества звёзд
    private int CalculateStars()
    {
        if (currentLevel == null) return 0;

        int stars = 0;
        var thresholds = currentLevel.StarThresholds;

        switch (currentLevel.GoalType)
        {
            case LevelGoalType.Score:
                if (currentScore >= thresholds[0]) stars = 1;
                if (currentScore >= thresholds[1]) stars = 2;
                if (currentScore >= thresholds[2]) stars = 3;
                break;

            case LevelGoalType.TimeLimit:
                float timeRatio = 1f - (timeElapsed / currentLevel.TimeLimitSeconds);
                if (timeRatio >= 0.8f) stars = 3;
                else if (timeRatio >= 0.6f) stars = 2;
                else if (timeRatio >= 0.4f) stars = 1;
                break;

            case LevelGoalType.MovesLimit:
                if (currentLevel.MovesLimit > 0)
                {
                    float movesRatio = 1f - ((float)movesMade / currentLevel.MovesLimit);
                    if (movesRatio >= 0.8f) stars = 3;
                    else if (movesRatio >= 0.6f) stars = 2;
                    else if (movesRatio >= 0.4f) stars = 1;
                }
                break;

            case LevelGoalType.ClearTiles:
                if (tilesCleared >= thresholds[0]) stars = 1;
                if (tilesCleared >= thresholds[1]) stars = 2;
                if (tilesCleared >= thresholds[2]) stars = 3;
                break;
        }

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
            Score = currentScore,
            MovesUsed = movesMade,
            TimeUsed = timeElapsed,
            TilesCleared = tilesCleared
        };

        // --- Обновляем прогресс игрока ---
        PlayerProgress.AddStars(result.LevelId, result.StarsEarned);

        // Сохраняем прогресс (опционально, если нужно для аналитики)
        SaveLevelProgress(result);
        
        // Уведомляем UI
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
}

[System.Serializable]
public class LevelResult
{
    public int LevelId;
    public bool IsCompleted;
    public int StarsEarned;
    public int Score;
    public int MovesUsed;
    public float TimeUsed;
    public int TilesCleared;
} 