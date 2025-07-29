using UnityEngine;
[System.Serializable]
public class LevelMetaData 
{
    public int levelId;
    public int levelNumber;
    public bool isUnlocked;
    public int starsEarned;
    public int requiredStars; // Number of stars required to unlock the level
}
