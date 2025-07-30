using System;
using TMPro;
using UnityEngine;

public class LivesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        UpdateUI();
        InvokeRepeating(nameof(UpdateUI), 1f, 1f);
    }

    private void UpdateUI()
    {
        int lives = LivesManager.Instance.CurrentLives;
        livesText.text = $"Lives: {lives}";

        if (lives < 5)
        {
            var timeLeft = GetTimeToNextLife();
            timerText.text = $"Next life in: {FormatTime(timeLeft)}";
        }
        else
        {
            timerText.text = "";
        }
    }

    private TimeSpan GetTimeToNextLife()
    {
        var lastUsed = PlayerPrefs.HasKey("LastLifeUsed")
            ? DateTime.FromBinary(Convert.ToInt64(PlayerPrefs.GetString("LastLifeUsed")))
            : DateTime.UtcNow;

        var timePassed = DateTime.UtcNow - lastUsed;
        float interval = LivesManager.Instance == null ? 30f : LivesManager.Instance.GetMinutesToRecoverOneLife();

        float timeRemaining = interval * 60f - (float)timePassed.TotalSeconds;
        return TimeSpan.FromSeconds(Mathf.Max(0, timeRemaining));
    }

    private string FormatTime(TimeSpan time)
    {
        return $"{time.Minutes:D2}:{time.Seconds:D2}";
    }
}
