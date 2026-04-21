using UnityEngine;

public class WinUI : MonoBehaviour
{
    public static WinUI Instance;

    [SerializeField] private GameObject winPanel;

    private void Awake()
    {
        Instance = this;
        winPanel.SetActive(false);
    }

    public void ShowWinScreen()
    {
        Debug.Log("Player wins!");
        winPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
