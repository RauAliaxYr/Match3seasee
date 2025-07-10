using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsDatabase", menuName = "Scriptable Objects/LevelsDatabase")]
public class LevelsDatabase : ScriptableObject
{
    public List<LevelGameplayData> levels;

    public LevelGameplayData GetLevelById(int id)
    {
        return levels.FirstOrDefault(l => l.LevelId == id);
    }
}
