using UnityEngine;
using UnityEngine.SceneManagement;

public class GameWinTrigger : MonoBehaviour
{
    [SerializeField] private string npcTag = "NPC"; // set your NPC tag
    private bool hasWon = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Game Win Trigger entered by: " + other.gameObject.name + " with tag: " + other.gameObject.tag);

        if (hasWon)
            return;

        if (other.CompareTag(npcTag))
        {
            hasWon = true;

            Debug.Log("NPC reached trailhead — YOU WIN");

            HandleWin();
        }
    }

    private void HandleWin()
    {
        PlayerPrefs.SetString("GameResult", "Victory! You safely guided Alice back to the trailhead.");
        SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);

        // stop gameplay
        //GameManager.Instance.SetMode(GameManager.PlayerMode.Paused);
        //WinUI.Instance.ShowWinScreen();
    }
}
