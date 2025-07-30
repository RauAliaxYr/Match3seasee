using UnityEngine;

public static class PlayerProgress
{
    // Get star count for level
    public static int GetStars(int levelId)
    {
        return PlayerPrefs.GetInt($"Level_{levelId}_Stars", 0);
    }

    // Save stars for level and increase total progress only if it became more
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

    // Get total progress (all earned stars)
    public static int GetTotalStars()
    {
        return PlayerPrefs.GetInt("TotalStars", 0);
    }

    // Reset progress (for tests or new game)
    public static void ResetProgress(int maxLevelId)
    {
        PlayerPrefs.SetInt("TotalStars", 0);
        for (int i = 1; i <= maxLevelId; i++)
            PlayerPrefs.SetInt($"Level_{i}_Stars", 0);
    }

    // Complete reset of all player progress (all PlayerPrefs)
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
} 