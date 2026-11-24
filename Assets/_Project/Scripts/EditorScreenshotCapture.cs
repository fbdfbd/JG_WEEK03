#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

// Automatically available in Play Mode; no scene setup required.
public sealed class EditorScreenshotCapture : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (FindFirstObjectByType<EditorScreenshotCapture>() != null)
        {
            return;
        }

        var captureObject = new GameObject(nameof(EditorScreenshotCapture));
        captureObject.AddComponent<EditorScreenshotCapture>();
        DontDestroyOnLoad(captureObject);
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.f12Key.wasPressedThisFrame)
        {
            return;
        }

        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Screenshots"));
        Directory.CreateDirectory(directory);

        string filename = $"FACE-OFF_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        string path = Path.Combine(directory, filename);
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"[Screenshot] Capture requested ({Screen.width}x{Screen.height}): {path}");
    }
}
#endif
