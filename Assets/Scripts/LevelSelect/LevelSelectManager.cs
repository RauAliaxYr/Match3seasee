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
    [Header("Level Configuration")]
    [SerializeField] private LevelsDatabase levelsDatabase;
    [Header("Stars UI")]
    [SerializeField] private TotalStarsUI totalStarsUI;
    [Header("Navigation")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    
    [Header("Adaptive UI")]
    [SerializeField] private AdaptiveLevelGrid adaptiveLevelGrid;
    [SerializeField] private AdaptiveHeightController adaptiveHeightController;

    private int currentChapterIndex = 0;

    private void Start()
    {
        // Setup adaptive grid if available
        if (adaptiveLevelGrid == null)
        {
            adaptiveLevelGrid = levelsParent.GetComponent<AdaptiveLevelGrid>();
        }
        
        // Setup adaptive height controller if available
        if (adaptiveHeightController == null)
        {
            adaptiveHeightController = GetComponent<AdaptiveHeightController>();
        }
        
        LoadChapter(currentChapterIndex);
        
        // Start level selection music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuThemeIfNotPlaying();
            // Apply saved global music volume from PlayerPrefs
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            AudioManager.Instance.SetMusicVolume(savedMusicVolume);
        }
    }

    private void OnEnable()
    {
        LoadChapter(currentChapterIndex);
        
        // Start level selection music on activation
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuThemeIfNotPlaying();
            // Apply saved global music volume from PlayerPrefs
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            AudioManager.Instance.SetMusicVolume(savedMusicVolume);
        }
    }

    public void LoadChapter(int index)
    {
        currentChapterIndex = index;
        LevelChapterData chapter = chapters[index];

        backgroundImage.sprite = chapter.GetBackgroundForCurrentPlatform();
        chapterText.text = chapter.chapterName;

        // Remove old buttons
        foreach (Transform child in levelsParent)
            Destroy(child.gameObject);

        // --- Sequential level unlocking with star gates ---
        int totalStars = PlayerProgress.GetTotalStars();
        bool previousUnlocked = true;

        foreach (var meta in chapter.levels)
        {
            var config = levelsDatabase.GetLevelById(meta.levelId);
            if (config == null)
            {
                Debug.LogWarning($"No LevelGameplayData configuration for level {meta.levelNumber}// levelId = {meta.levelId}//");
                continue;
            }

            int stars = PlayerProgress.GetStars(meta.levelId);
            bool isUnlocked = false;

            // All levels (including first) require stars to unlock
            // Special case: first level of first chapter is always unlocked (requires 0 stars)
            if (meta.levelId == 1 || (previousUnlocked && totalStars >= meta.requiredStars))
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

        // Update stars UI
        if (totalStarsUI != null)
            totalStarsUI.UpdateStars();
            
        // Update navigation arrows state
        UpdateNavigationArrows();
        
        // Refresh adaptive grid if available
        if (adaptiveLevelGrid != null)
        {
            // Wait a frame to ensure all buttons are created
            StartCoroutine(RefreshGridAfterFrame());
        }
        
        // Refresh adaptive height controller if available
        if (adaptiveHeightController != null)
        {
            adaptiveHeightController.RefreshHeights();
        }
    }

    public void NextChapter() => LoadChapter((currentChapterIndex + 1) % chapters.Count);
    public void PreviousChapter() => LoadChapter((currentChapterIndex - 1 + chapters.Count) % chapters.Count);
    
    private System.Collections.IEnumerator RefreshGridAfterFrame()
    {
        // Wait for the end of frame to ensure all buttons are created
        yield return new WaitForEndOfFrame();
        
        if (adaptiveLevelGrid != null)
        {
            adaptiveLevelGrid.RefreshGrid();
        }
    }
    
    private void UpdateNavigationArrows()
    {
        // Deactivate left arrow on first chapter
        if (leftArrowButton != null)
        {
            leftArrowButton.interactable = (currentChapterIndex > 0);
        }
        
        // Deactivate right arrow on last chapter
        if (rightArrowButton != null)
        {
            rightArrowButton.interactable = (currentChapterIndex < chapters.Count - 1);
        }
        
        // If only one chapter, deactivate both arrows
        if (chapters.Count <= 1)
        {
            if (leftArrowButton != null) leftArrowButton.interactable = false;
            if (rightArrowButton != null) rightArrowButton.interactable = false;
        }
    }
}

