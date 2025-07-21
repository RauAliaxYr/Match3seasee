using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static void LoadGameplayScene()
    {
        SceneManager.LoadScene("GameplayScene"); // Убедись, что сцена добавлена в Build Settings
    }

    public static void LoadLevelSelect()
    {
        SceneManager.LoadScene("LevelSelectScene");
    }

    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("LogoScene");
    }
}
