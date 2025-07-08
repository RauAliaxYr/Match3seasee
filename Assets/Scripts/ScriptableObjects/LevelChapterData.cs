using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelChapterData", menuName = "Scriptable Objects/LevelChapterData")]
public class LevelChapterData : ScriptableObject
{
    public string chapterName;
    public Sprite background;
    public List<LevelMetaData> levels;

    public bool isUnlocked = true;
}
