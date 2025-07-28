using UnityEngine;

public static class PlayerProgress
{
    // Получить количество звёзд для уровня
    public static int GetStars(int levelId)
    {
        return PlayerPrefs.GetInt($"Level_{levelId}_Stars", 0);
    }

    // Сохраняет звёзды для уровня и увеличивает общий прогресс только если стало больше
    public static void AddStars(int levelId, int newStars)
    {
        int prevStars = GetStars(levelId);
        if (newStars > prevStars)
        {
            int diff = newStars - prevStars;
            int total = GetTotalStars() + diff;
            PlayerPrefs.SetInt("TotalStars", total);
            PlayerPrefs.SetInt($"Level_{levelId}_Stars", newStars);
        }
    }

    // Получить общий прогресс (все заработанные звёзды)
    public static int GetTotalStars()
    {
        return PlayerPrefs.GetInt("TotalStars", 0);
    }

    // Сбросить прогресс (для тестов или новой игры)
    public static void ResetProgress(int maxLevelId)
    {
        PlayerPrefs.SetInt("TotalStars", 0);
        for (int i = 1; i <= maxLevelId; i++)
            PlayerPrefs.SetInt($"Level_{i}_Stars", 0);
    }

    // Полный сброс всего прогресса игрока (все PlayerPrefs)
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
} 