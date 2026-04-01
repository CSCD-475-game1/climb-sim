using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneBootstrap : MonoBehaviour
{
    [SerializeField] private string trailheadSceneName = "Trailhead";

    async void Start()
    {
        if (!SceneManager.GetSceneByName(trailheadSceneName).isLoaded)
        {
            await SceneManager.LoadSceneAsync(trailheadSceneName, LoadSceneMode.Additive);
        }
    }
}
