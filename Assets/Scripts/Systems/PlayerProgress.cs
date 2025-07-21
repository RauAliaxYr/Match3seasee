using UnityEngine;

public static class PlayerProgress
{
    // Получить количество звёзд для уровня
    public static int GetStars(int levelId)
    {
        return PlayerPrefs.GetInt($"Level_{levelId}_Stars", 0);
    }

    // Сохранить количество звёзд для уровня (только если больше)
    public static void SetStars(int levelId, int stars)
    {
        int prev = GetStars(levelId);
        if (stars > prev)
            PlayerPrefs.SetInt($"Level_{levelId}_Stars", stars);
    }

    // Проверить, открыт ли уровень
    public static bool IsLevelUnlocked(int levelId)
    {
        return PlayerPrefs.GetInt($"Level_{levelId}_Unlocked", levelId == 1 ? 1 : 0) == 1;
    }

    // Открыть уровень
    public static void UnlockLevel(int levelId)
    {
        PlayerPrefs.SetInt($"Level_{levelId}_Unlocked", 1);
    }
} 