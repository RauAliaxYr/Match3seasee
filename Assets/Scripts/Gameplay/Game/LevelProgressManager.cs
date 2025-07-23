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

        // Завершение уровня только по лимиту времени
        if (HasTimeLimit() && timeElapsed >= currentLevel.TimeLimitSeconds)
        {
            int stars = CalculateStars();
            CompleteLevel(stars > 0);
        }
    }

    // Вызывается при успешном свапе
    public void OnMoveMade()
    {
        if (!isLevelActive) return;

        movesMade++;
        OnMovesChanged?.Invoke(movesMade);

        // Завершение уровня только по лимиту ходов
        if (HasMovesLimit() && movesMade >= currentLevel.MovesLimit)
        {
            int stars = CalculateStars();
            CompleteLevel(stars > 0);
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
    }

    // Определение количества звёзд
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

    // Вспомогательные методы для лимитов
    private bool HasTimeLimit() => currentLevel != null && currentLevel.TimeLimitSeconds > 0f;
    private bool HasMovesLimit() => currentLevel != null && currentLevel.MovesLimit > 0;
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