using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelGameplayData selectedLevel;

    public LevelGameplayData SelectedLevel => selectedLevel;

    private void Awake()
    {
        // Реализация Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Устанавливает активный уровень перед переходом на геймплейную сцену.
    /// </summary>
    public void SetSelectedLevel(LevelGameplayData level)
    {
        selectedLevel = level;
    }

    // Можно добавить методы для сброса/сохранения данных уровня
}

