using System.Collections.Generic;
using GameData;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelGameplayData", menuName = "Match3/Level Gameplay Data")]
public class LevelGameplayData:ScriptableObject
{
    [Header("Идентификатор уровня")]
    [SerializeField] private int levelId;
    [SerializeField] private string levelName;

    [Header("Основные параметры")]
    public int width = 9;
    public int height = 9;
    [SerializeField] private LevelGoalType goalType = LevelGoalType.Score;

    [Header("Маска заблокированных клеток")]
    public List<RowData> blockedCells; 
    
    [Header("Цели уровня")]
    [SerializeField] private int targetScore = 1000;
    [SerializeField] private int timeLimitSeconds = 60;
    [SerializeField] private int movesLimit = 0;

    [Header("Пороги звёзд")]
    [SerializeField] private int[] starThresholds = new int[3]; // 1, 2, 3 звезды

    [Header("Описание (для редактора)")]
    [TextArea]
    [SerializeField] private string description;

    // --- Публичные свойства ---
    public int LevelId => levelId;
    public string LevelName => levelName;
    public int Width => width;
    public int Height => height;
    public LevelGoalType GoalType => goalType;
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

