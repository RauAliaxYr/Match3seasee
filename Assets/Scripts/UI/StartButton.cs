using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnStartButtonClick);
        }
    }

    private void OnStartButtonClick()
    {
        // Звук нажатия кнопки
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Переход к выбору уровней
        SceneLoader.LoadLevelSelect();
    }
} 