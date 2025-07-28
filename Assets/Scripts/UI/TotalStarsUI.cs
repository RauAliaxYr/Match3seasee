using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TotalStarsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private Image starIcon;

    public void UpdateStars()
    {
        int totalStars = PlayerProgress.GetTotalStars();
        if (starsText != null)
            starsText.text = totalStars.ToString();
    }

    public void ResetProgressButton(int maxLevelId)
    {
        PlayerProgress.ResetProgress(maxLevelId);
        PlayerProgress.ResetAllProgress();
        Debug.Log("Progress reset");
        UpdateStars();
    }
} 