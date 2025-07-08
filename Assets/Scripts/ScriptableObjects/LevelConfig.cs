using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public int levelId;
    public string levelName;

    public GoalType goalType;
    public int goalValue; // Очки или таймер — в зависимости от типа цели
    public int[] starThresholds = new int[3]; // Например: [1000, 2000, 3000]

    public int boardWidth = 8;
    public int boardHeight = 8;
}

public enum GoalType
{
    ScoreByTime,
    ScoreByMoves,
    ClearTiles
}
