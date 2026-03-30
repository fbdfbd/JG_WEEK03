using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainingSceneController : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private string _lobbySceneName = "03_Lobby";

    private void OnEnable()
    {
        if (_exitButton != null)
        {
            _exitButton.onClick.AddListener(OnClick);
        }
    }

    private void OnDisable()
    {
        if (_exitButton != null)
        {
            _exitButton.onClick.RemoveListener(OnClick);
        }
    }

    public void OnClick()
    {
        if (GameManager.I != null && GameManager.I.TryLoadLobbyScene())
        {
            return;
        }

        SceneManager.LoadScene(_lobbySceneName);
    }
}
