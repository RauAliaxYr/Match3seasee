using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelGameplayData selectedLevel;

    public LevelGameplayData SelectedLevel => selectedLevel;

    private void Awake()
    {
        // Singleton implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sets the active level before transitioning to the gameplay scene.
    /// </summary>
    public void SetSelectedLevel(LevelGameplayData level)
    {
        selectedLevel = level;
    }
}

