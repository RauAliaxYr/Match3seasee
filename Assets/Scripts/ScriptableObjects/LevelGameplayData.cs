using System.Collections.Generic;
using GameData;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelGameplayData", menuName = "Match3/Level Gameplay Data")]
public class LevelGameplayData:ScriptableObject
{
    [Header("Level Identifier")]
    [SerializeField] private int levelId;
    [SerializeField] private string levelName;

    [Header("Main Parameters")]
    public int width = 9;
    public int height = 9;
    [SerializeField] private LevelGoalType goalType = LevelGoalType.Score;
    [SerializeField] private int tileTypesCount = 5; // Number of tile types available on this level (1-8, default: 5)

    [Header("Blocked Cells Mask")]
    public List<RowData> blockedCells; 
    
    [Header("Level Goals")]
    [SerializeField] private int targetScore = 1000;
    [SerializeField] private int timeLimitSeconds = 60;
    [SerializeField] private int movesLimit = 0;

    [Header("Star Thresholds")]
    [SerializeField] private int[] starThresholds = new int[3]; // 1, 2, 3 stars

    [Header("Description (for editor)")]
    [TextArea]
    [SerializeField] private string description;

    // --- Public properties ---
    public int LevelId => levelId;
    public string LevelName => levelName;
    public int Width => width;
    public int Height => height;
    public LevelGoalType GoalType => goalType;
    public int TileTypesCount => tileTypesCount;
    public int TargetScore => targetScore;
    public int TimeLimitSeconds => timeLimitSeconds;
    public int MovesLimit => movesLimit;
    public int[] StarThresholds => starThresholds;
    public string Description => description;
    public bool[,] GetBlockedCells()
    {
        var result = new bool[width, height];
        for (int y = 0; y < height && y < blockedCells.Count; y++)
        {
            var row = blockedCells[y];
            for (int x = 0; x < width && x < row.cells.Count; x++)
            {
                result[x, y] = row.cells[x];
            }
        }
        return result;
    }
}

public enum LevelGoalType
{
    Score,
    TimeLimit,
    MovesLimit,
    ClearTiles
}
[System.Serializable]
public class RowData
{
    public List<bool> cells = new List<bool>();
}

