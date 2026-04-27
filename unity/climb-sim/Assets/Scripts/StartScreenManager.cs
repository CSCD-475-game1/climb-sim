using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TMP_Text gameResultText;

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        string resultMessage = PlayerPrefs.GetString("GameResult", "");

        if (!string.IsNullOrEmpty(resultMessage))
        {
            gameResultText.text = resultMessage;
            gameResultText.gameObject.SetActive(true);
        }
        else
        {
            gameResultText.gameObject.SetActive(false);
        }

        PlayerPrefs.DeleteKey("GameResult");
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        int selectedIndex = difficultyDropdown.value;
        string selectedOption = difficultyDropdown.options[selectedIndex].text;

        Debug.Log("Selected difficulty: " + selectedOption);

        // Optional: store globally
        PlayerPrefs.SetString("Difficulty", selectedOption);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        SceneManager.LoadScene("Trailhead", LoadSceneMode.Additive);
        SceneManager.LoadScene("TrailA", LoadSceneMode.Additive);
        SceneManager.LoadScene("LogCrossing", LoadSceneMode.Additive);
    }
}
