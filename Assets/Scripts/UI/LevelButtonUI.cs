using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Button playButton;

    private LevelConfig levelData;

    public void Setup(LevelConfig config, Action<LevelConfig> onClick)
    {
        levelData = config;
        levelNameText.text = $"Уровень {config.levelId}";
        playButton.onClick.AddListener(() => onClick?.Invoke(levelData));
    }
}
