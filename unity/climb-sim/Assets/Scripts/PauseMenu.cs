using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayMode)
            return; 

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isCurrentlyActive = container.activeSelf;
            container.SetActive(!isCurrentlyActive);

            if (container.activeSelf) 
            {
                Time.timeScale = 0f;
            }
            else 
            {
                Time.timeScale = 1f;
            }
        }
    } 

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1f;
    }

    private void PauseGame()
    {
        container.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        container.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartButton()
    {
        // Add logic here later
    }
}
