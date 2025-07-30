using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Image[] stars;
    [SerializeField] private Sprite starActiveSprite;   // gold star
    [SerializeField] private Sprite starInactiveSprite; // empty star
    [SerializeField] private Button button;
    
    private LevelGameplayData levelData;
    private int levelId;

    public void Initialize(int level,  bool isUnlocked , int starsEarned ,LevelGameplayData data)
    {
        levelData = data;
        levelId = levelData.LevelId;
        levelText.text = level.ToString();
        lockedOverlay.SetActive(!isUnlocked);
        button.interactable = isUnlocked;

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = (i < starsEarned) ? starActiveSprite : starInactiveSprite;
            stars[i].enabled = true; // always enabled
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Debug.Log($"Starting level {levelId}");

        // Button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        if (levelData == null)
        {
            Debug.LogError("LevelGameplayData is not set!");
            return;
        }

        GameManager.Instance.SetSelectedLevel(levelData);
        SceneLoader.LoadGameplayScene();
    }
}