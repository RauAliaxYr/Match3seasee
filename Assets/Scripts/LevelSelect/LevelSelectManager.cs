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

        // Создаём новые
        foreach (var level in chapter.levels)
        {
            GameObject btn = Instantiate(levelButtonPrefab, levelsParent);
            btn.GetComponent<LevelButton>().Initialize(
                level.levelNumber,
                level.isUnlocked,
                level.starsEarned
            );
        }
    }

    public void NextChapter()
    {
        int nextIndex = (currentChapterIndex + 1) % chapters.Count;
        LoadChapter(nextIndex);
    }

    public void PreviousChapter()
    {
        int prevIndex = (currentChapterIndex - 1 + chapters.Count) % chapters.Count;
        LoadChapter(prevIndex);
    }
}

