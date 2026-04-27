using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStreamingManager : MonoBehaviour
{
    public static SceneStreamingManager Instance { get; private set; }

    [Header("Optional: scenes to load at startup")]
    public List<string> startupScenes = new List<string>();

    private readonly HashSet<string> loadedDetailScenes = new HashSet<string>();
    private readonly HashSet<string> loadingScenes = new HashSet<string>();
    private readonly HashSet<string> unloadingScenes = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private async void Start()
    {
        foreach (string sceneName in startupScenes)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                await LoadDetailScene(sceneName);
            }
        }
    }

    public async Awaitable LoadDetailScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded || loadedDetailScenes.Contains(sceneName) || loadingScenes.Contains(sceneName))
            return;

        loadingScenes.Add(sceneName);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            Debug.LogError($"Failed to start loading scene '{sceneName}'. Is it in Build Settings?");
            loadingScenes.Remove(sceneName);
            return;
        }

        while (!op.isDone)
            await Awaitable.NextFrameAsync();

        loadedDetailScenes.Add(sceneName);
        loadingScenes.Remove(sceneName);

        Debug.Log($"Loaded detail scene: {sceneName}");
    }

    public async Awaitable UnloadDetailScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded || unloadingScenes.Contains(sceneName))
            return;

        unloadingScenes.Add(sceneName);

        AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"Failed to start unloading scene '{sceneName}'.");
            unloadingScenes.Remove(sceneName);
            return;
        }

        while (!op.isDone)
            await Awaitable.NextFrameAsync();

        loadedDetailScenes.Remove(sceneName);
        unloadingScenes.Remove(sceneName);

        Debug.Log($"Unloaded detail scene: {sceneName}");
    }

    public bool IsSceneLoaded(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).isLoaded;
    }

    // exit game
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
