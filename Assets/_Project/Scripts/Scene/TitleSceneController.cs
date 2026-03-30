using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class TitleSceneController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button settingButton;

    [Header("Fallback")]
    [SerializeField] private string trainingSceneName = "02_Training";

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeButtons();
    }

    private void OnDisable()
    {
        UnsubscribeButtons();
    }

    public void OnStartButtonClicked()
    {
        if (GameManager.I != null && GameManager.I.TryLoadTrainingScene())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(trainingSceneName))
        {
            Debug.LogWarning("TitleSceneController requires a valid training scene name.");
            return;
        }

        SceneManager.LoadScene(trainingSceneName);
    }

    public void OnExitButtonClicked()
    {
        if (GameManager.I != null)
        {
            GameManager.I.QuitGame();
            return;
        }

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnSettingButtonClicked()
    {
        Debug.Log("TitleSceneController.OnSettingButtonClicked is ready. Add the settings flow here when the settings scene is prepared.");
    }

    private void ResolveReferences()
    {
        startButton ??= FindButton("NewStartButton");
        exitButton ??= FindButton("ExitButton");
        settingButton ??= FindButton("SettingButton");
    }

    private void SubscribeButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        if (settingButton != null)
        {
            settingButton.onClick.AddListener(OnSettingButtonClicked);
        }
    }

    private void UnsubscribeButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(OnSettingButtonClicked);
        }
    }

    private Button FindButton(string buttonName)
    {
        if (string.IsNullOrWhiteSpace(buttonName))
        {
            return null;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int index = 0; index < buttons.Length; index++)
        {
            Button currentButton = buttons[index];
            if (currentButton != null && currentButton.name == buttonName)
            {
                return currentButton;
            }
        }

        return null;
    }
}
