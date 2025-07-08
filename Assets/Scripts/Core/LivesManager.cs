using System;
using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [SerializeField] private int maxLives = 5;
    [SerializeField] private float minutesToRecoverOneLife = 30f;

    private int currentLives;
    private DateTime lastLifeUsedTime;

    public int CurrentLives => currentLives;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLives();
        InvokeRepeating(nameof(UpdateLives), 1f, 30f);
    }

    public float GetMinutesToRecoverOneLife()
    {
        return minutesToRecoverOneLife;
    }

    public bool TryUseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            lastLifeUsedTime = DateTime.UtcNow;
            SaveLives();
            return true;
        }
        return false;
    }

    public void AddLives(int amount)
    {
        currentLives = Mathf.Min(maxLives, currentLives + amount);
        SaveLives();
    }

    private void UpdateLives()
    {
        if (currentLives >= maxLives)
            return;

        var timePassed = DateTime.UtcNow - lastLifeUsedTime;
        int livesToRecover = Mathf.FloorToInt((float)timePassed.TotalMinutes / minutesToRecoverOneLife);

        if (livesToRecover > 0)
        {
            currentLives = Mathf.Min(maxLives, currentLives + livesToRecover);
            lastLifeUsedTime = DateTime.UtcNow - TimeSpan.FromMinutes((float)timePassed.TotalMinutes % minutesToRecoverOneLife);
            SaveLives();
        }
    }

    private void SaveLives()
    {
        PlayerPrefs.SetInt("Lives", currentLives);
        PlayerPrefs.SetString("LastLifeUsed", lastLifeUsedTime.ToBinary().ToString());
    }

    private void LoadLives()
    {
        currentLives = PlayerPrefs.GetInt("Lives", maxLives);
        if (PlayerPrefs.HasKey("LastLifeUsed"))
        {
            long binary = Convert.ToInt64(PlayerPrefs.GetString("LastLifeUsed"));
            lastLifeUsedTime = DateTime.FromBinary(binary);
        }
        else
        {
            lastLifeUsedTime = DateTime.UtcNow;
        }
    }
    
}
        
