using UnityEngine;
using UnityEngine.UI;

public class PlayButtonHandler : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        if (LivesManager.Instance.TryUseLife())
        {
            // Тут можно загрузить сцену геймплея
            Debug.Log("Уровень запускается...");
            // SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("Нет жизней!");
            PlayerPrefs.DeleteAll();
            // Можно вызвать окно “Купить жизни”
        }
    }
}
