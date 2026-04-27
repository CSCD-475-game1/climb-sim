using UnityEngine;

public class GameInputModeManager : MonoBehaviour
{
    public static GameInputModeManager Instance { get; private set; }

    public enum InputMode
    {
        Gameplay,
        Chat,
        Inventory,
        Paused
    }

    public InputMode CurrentMode { get; private set; } = InputMode.Gameplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetMode(InputMode newMode)
    {
        CurrentMode = newMode;
        Debug.Log("Set Input mode: " + newMode);
    }

    public bool IsGameplay() => CurrentMode == InputMode.Gameplay;
    public bool IsChat() => CurrentMode == InputMode.Chat;
    public bool IsInventory() => CurrentMode == InputMode.Inventory;
    public bool IsPaused() => CurrentMode == InputMode.Paused;
}
