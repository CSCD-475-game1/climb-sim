using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;
    public GameObject exitButton;

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayMode)
            return;

        if (GameInputModeManager.Instance != null && GameInputModeManager.Instance.IsInventory())
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isCurrentlyActive = container.activeSelf;
            container.SetActive(!isCurrentlyActive);

            if (container.activeSelf) 
            {
                Time.timeScale = 0f;
                exitButton.SetActive(true);
            }
            else 
            {
                Time.timeScale = 1f;
                exitButton.SetActive(false);
            }
        }
    } 

    public void ResumeButton()
    {
        container.SetActive(false);
        exitButton.SetActive(false);
        Time.timeScale = 1f;
    }

    private void PauseGame()
    {
        container.SetActive(true);
        exitButton.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        container.SetActive(false);
        Time.timeScale = 1f;
        exitButton.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartButton()
    {
        // Add logic here later
    }
}
