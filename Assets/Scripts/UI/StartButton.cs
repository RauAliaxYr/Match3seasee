using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        StartCoroutine(LoadLevelSelectWithDelay());
    }

    private IEnumerator LoadLevelSelectWithDelay()
    {
        yield return new WaitForSeconds(0.25f); // 250 ms delay for button sound
        SceneLoader.LoadLevelSelect();
    }
} 