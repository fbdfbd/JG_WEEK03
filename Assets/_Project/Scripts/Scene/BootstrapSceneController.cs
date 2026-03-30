using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class BootstrapSceneController : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "01_Title";

    private void Start()
    {
        if (GameManager.I != null && GameManager.I.TryLoadTitleScene())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogWarning("BootstrapSceneController requires a valid title scene name.");
            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }
}
