using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Player detection")]
    public string playerTag = "Player";

    [Header("Scenes to load when entered")]
    public List<string> scenesToLoad = new List<string>();

    [Header("Scenes to unload when entered")]
    public List<string> scenesToUnload = new List<string>();

    [Header("Only fire once")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private async void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (SceneStreamingManager.Instance == null)
        {
            Debug.LogError("No SceneStreamingManager found in Main scene.");
            return;
        }

        hasTriggered = true;

        foreach (string sceneName in scenesToLoad)
        {
            await SceneStreamingManager.Instance.LoadDetailScene(sceneName);
        }

        foreach (string sceneName in scenesToUnload)
        {
            await SceneStreamingManager.Instance.UnloadDetailScene(sceneName);
        }
    }
}
