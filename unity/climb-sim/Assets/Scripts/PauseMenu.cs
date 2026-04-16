using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;

    void Update()
    {
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

    public void RestartButton()
    {
        // Add logic here later
    }
}