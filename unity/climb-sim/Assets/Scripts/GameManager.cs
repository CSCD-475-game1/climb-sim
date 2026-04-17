using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum PlayerMode
    {
        Gameplay,
        Chat,
        Inventory,
        Driving,
        Paused
    }

    public PlayerMode CurrentMode { get; private set; } = PlayerMode.Gameplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetMode(PlayerMode newMode)
    {
        CurrentMode = newMode;
        Debug.Log("Player mode set to: " + newMode);
    }

    public bool IsGameplayMode => CurrentMode == PlayerMode.Gameplay;
    public bool IsChatMode => CurrentMode == PlayerMode.Chat;
    public bool IsInventoryMode => CurrentMode == PlayerMode.Inventory;
    public bool IsDrivingMode => CurrentMode == PlayerMode.Driving;
    public bool IsPausedMode => CurrentMode == PlayerMode.Paused;
}
