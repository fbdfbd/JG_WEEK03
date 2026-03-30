using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Persistent Services")]
    [SerializeField] private CampaignRunService campaignRunService;
    [SerializeField] private StageFlowService stageFlowService;
    [SerializeField] private PlayerProfileService playerProfileService;

    public CampaignRunService CampaignRunService => campaignRunService;
    public StageFlowService StageFlowService => stageFlowService;
    public PlayerProfileService PlayerProfileService => playerProfileService;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
        EnsurePersistentServices();
    }

    private void OnDestroy()
    {
        if (I == this)
        {
            I = null;
        }
    }

    public bool TryLoadTitleScene()
    {
        EnsurePersistentServices();
        return stageFlowService != null && stageFlowService.LoadTitleScene();
    }

    public bool TryLoadLobbyScene()
    {
        EnsurePersistentServices();
        return stageFlowService != null && stageFlowService.LoadLobbyScene();
    }

    public bool TryLoadTrainingScene()
    {
        EnsurePersistentServices();
        return stageFlowService != null && stageFlowService.LoadTrainingScene();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsurePersistentServices()
    {
        campaignRunService ??= GetOrAddComponent<CampaignRunService>();
        stageFlowService ??= GetOrAddComponent<StageFlowService>();
        playerProfileService ??= GetOrAddComponent<PlayerProfileService>();
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return gameObject.AddComponent<T>();
    }
}
