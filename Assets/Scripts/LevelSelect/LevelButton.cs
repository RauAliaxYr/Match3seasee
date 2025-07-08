using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Image[] stars;
    [SerializeField] private Button button;

    private int levelNumber;

    public void Initialize(int level, bool isUnlocked = true, int starsEarned = 0)
    {
        levelNumber = level;
        levelText.text = level.ToString();
        lockedOverlay.SetActive(!isUnlocked);
        button.interactable = isUnlocked;

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = (i < starsEarned);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Debug.Log($"Запуск уровня {levelNumber}");
        // TODO: загрузка игровой сцены
    }
}
