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
    [Header("UI звёзд")]
    [SerializeField] private TotalStarsUI totalStarsUI;

    private int currentChapterIndex = 0;

    private void Start()
    {
        LoadChapter(currentChapterIndex);
        
        // Запускаем музыку выбора уровней
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuTheme();
            AudioManager.Instance.SetMusicVolume(0.7f);
        }
    }

    private void OnEnable()
    {
        LoadChapter(currentChapterIndex);
        
        // Запускаем музыку выбора уровней при активации
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuTheme();
            AudioManager.Instance.SetMusicVolume(0.7f);
        }
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

        // --- Последовательное открытие уровней с гейтами по звёздам ---
        int totalStars = PlayerProgress.GetTotalStars();
        bool previousUnlocked = true;

        foreach (var meta in chapter.levels)
        {
            var config = levelsDatabase.GetLevelById(meta.levelId);
            if (config == null)
            {
                Debug.LogWarning($"Нет конфигурации LevelGameplayData для уровня {meta.levelNumber}// levelId = {meta.levelId}//");
                continue;
            }

            int stars = PlayerProgress.GetStars(meta.levelId);
            bool isUnlocked = false;

            if (meta.levelNumber == 1)
            {
                isUnlocked = true;
            }
            else if (previousUnlocked && totalStars >= meta.requiredStars)
            {
                isUnlocked = true;
            }

            GameObject btn = Instantiate(levelButtonPrefab, levelsParent);
            btn.GetComponent<LevelButton>().Initialize(
                meta.levelNumber,
                isUnlocked,
                stars,
                config
            );

            previousUnlocked = isUnlocked;
        }

        // Обновляем UI звёзд
        if (totalStarsUI != null)
            totalStarsUI.UpdateStars();
    }

    public void NextChapter() => LoadChapter((currentChapterIndex + 1) % chapters.Count);
    public void PreviousChapter() => LoadChapter((currentChapterIndex - 1 + chapters.Count) % chapters.Count);
}

