using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class StageFlowService : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "01_Title";
    [SerializeField] private string lobbySceneName = "03_Lobby";
    [SerializeField] private string trainingSceneName = "02_Training";

    public bool LoadTitleScene()
    {
        return TryLoadScene(titleSceneName, "StageFlowService requires a valid title scene name.");
    }

    public bool LoadStageScene(StageEntryContext stageEntryContext)
    {
        if (stageEntryContext == null || !stageEntryContext.IsValid)
        {
            Debug.LogWarning("StageFlowService requires a valid StageEntryContext.");
            return false;
        }

        SceneManager.LoadScene(stageEntryContext.SceneName);
        return true;
    }

    public bool LoadTrainingScene()
    {
        return TryLoadScene(trainingSceneName, "StageFlowService requires a valid training scene name.");
    }

    public bool LoadLobbyScene()
    {
        return TryLoadScene(lobbySceneName, "StageFlowService requires a valid lobby scene name.");
    }

    private bool TryLoadScene(string sceneName, string warningMessage)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning(warningMessage);
            return false;
        }

        SceneManager.LoadScene(sceneName);
        return true;
    }
}
