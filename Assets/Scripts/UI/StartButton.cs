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
        // Button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Go to level selection
        SceneLoader.LoadLevelSelect();
    }
} 