using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] private LevelsDatabase levelsDatabase;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject levelButtonPrefab;

    void Start()
    {
        foreach (var config in levelsDatabase.levels)
        {
            var go = Instantiate(levelButtonPrefab, contentRoot);
            var button = go.GetComponent<LevelButtonUI>();
            button.Setup(config, OnLevelSelected);
        }
    }

    void OnLevelSelected(LevelConfig level)
    {
        GameSession.Instance.SetCurrentLevel(level);
        SceneManager.LoadScene("GameScene");
    }
}
