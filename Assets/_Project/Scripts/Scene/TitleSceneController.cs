using TMPro;
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

    private TitleLanguagePanel languagePanel;

    private void Awake()
    {
        ResolveReferences();
        if (settingButton != null) settingButton.gameObject.SetActive(true);
        RefreshLanguage();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeButtons();
    }

    private void OnDisable()
    {
        UnsubscribeButtons();
        languagePanel?.Hide();
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
        if (settingButton == null) return;
        languagePanel ??= new TitleLanguagePanel(settingButton, RefreshLanguage);
        languagePanel.Show();
    }

    private void RefreshLanguage()
    {
        SetLabel(startButton, "시작");
        SetLabel(exitButton, "종료");
        SetLabel(settingButton, "설정");
    }

    private static void SetLabel(Button button, string korean)
    {
        if (button == null) return;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;
        label.text = GameLanguage.Text(korean);
        GameLanguage.Fit(label);
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
