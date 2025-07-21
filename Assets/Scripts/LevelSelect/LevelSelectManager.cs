using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Transform levelsParent;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private List<LevelChapterData> chapters;
    [SerializeField] private TextMeshProUGUI chapterText;
    
    [Header("Конфигурации уровней")]
    [SerializeField] private LevelsDatabase levelsDatabase;
    
    private int currentChapterIndex = 0;

    private void Start()
    {
        LoadChapter(currentChapterIndex);
    }
    public void LoadChapter(int index)
    {
        currentChapterIndex = index;
        LevelChapterData chapter = chapters[index];

        backgroundImage.sprite = chapter.background;
        chapterText.text = chapter.chapterName;

        // Удаляем старые кнопки
        foreach (Transform child in levelsParent)
            Destroy(child.gameObject);

        // Создаём новые кнопки
        foreach (var meta in chapter.levels)
        {
            var config = levelsDatabase.GetLevelById(meta.levelId);
            if (config == null)
            {
                Debug.LogWarning($"Нет конфигурации LevelGameplayData для уровня {meta.levelNumber}// levelId = {meta.levelId}//");
                continue;
            }

            GameObject btn = Instantiate(levelButtonPrefab, levelsParent);
            btn.GetComponent<LevelButton>().Initialize(
                meta.levelNumber,
                meta.isUnlocked,
                meta.starsEarned,
                config
            );
        }
    }

    public void NextChapter() => LoadChapter((currentChapterIndex + 1) % chapters.Count);
    public void PreviousChapter() => LoadChapter((currentChapterIndex - 1 + chapters.Count) % chapters.Count);
}

