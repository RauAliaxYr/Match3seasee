using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelChapterData", menuName = "Scriptable Objects/LevelChapterData")]
public class LevelChapterData : ScriptableObject
{
    public string chapterName;
    
    [Header("Backgrounds")]
    [Tooltip("Background for mobile devices (portrait 9:16)")]
    public Sprite backgroundMobile;
    
    [Tooltip("Background for desktop/WebGL (landscape 16:9)")]
    public Sprite backgroundDesktop;
    
    [Header("Levels")]
    public List<LevelMetaData> levels;
    public bool isUnlocked = true;
    
    /// <summary>
    /// Gets the appropriate background sprite for the current platform
    /// </summary>
    public Sprite GetBackgroundForCurrentPlatform()
    {
#if UNITY_ANDROID || UNITY_IOS
        return backgroundMobile != null ? backgroundMobile : backgroundDesktop;
#else
        return backgroundDesktop != null ? backgroundDesktop : backgroundMobile;
#endif
    }
}
