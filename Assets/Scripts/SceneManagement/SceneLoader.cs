using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static void LoadGameplayScene()
    {
        AudioManager.CreateAudioManagerIfNeeded();
        SceneManager.LoadScene("GameplayScene"); // Убедись, что сцена добавлена в Build Settings
    }

    public static void LoadLevelSelect()
    {
        AudioManager.CreateAudioManagerIfNeeded();
        SceneManager.LoadScene("LevelSelectScene");
    }

    public static void LoadMainMenu()
    {
        AudioManager.CreateAudioManagerIfNeeded();
        SceneManager.LoadScene("LogoScene");
    }
}
