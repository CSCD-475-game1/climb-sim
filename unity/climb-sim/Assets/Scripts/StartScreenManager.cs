using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        SceneManager.LoadScene("Trailhead", LoadSceneMode.Additive);
        SceneManager.LoadScene("TrailA", LoadSceneMode.Additive);
        SceneManager.LoadScene("LogCrossing", LoadSceneMode.Additive);
    }
}
