using UnityEngine;

public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField] private ChatUIManager chatUI;
    [SerializeField] private InventoryUIManager inventoryUI;

    void Start()
    {
        if (chatUI == null)
        {
            Debug.LogError("ChatUIManager reference is missing in PlayerInputRouter.");
        }

        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUIManager reference is missing in PlayerInputRouter.");
        }

        // Ensure we start in Gameplay mode
        GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Gameplay);
    }

    void Update()
    {
        var mode = GameInputModeManager.Instance.CurrentMode;

        switch (mode)
        {
            case GameInputModeManager.InputMode.Gameplay:
                HandleGameplayInput();
                break;

            case GameInputModeManager.InputMode.Chat:
                HandleChatInput();
                break;

            case GameInputModeManager.InputMode.Inventory:
                HandleInventoryInput();
                break;
        }
    }

    void HandleGameplayInput()
    {
        if (GameManager.Instance.IsGameplayMode && Input.GetKeyDown(KeyCode.C))
        {
            GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Chat);
            chatUI.OpenChat();
        }
        else if (GameManager.Instance.IsChatMode && Input.GetKeyDown(KeyCode.Escape))
        {
            GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Gameplay);
            chatUI.CloseChat();
        }
        // movement is allowed here
    }

    void HandleChatInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            chatUI.CloseChat();
        }
    }

    void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I))
        {
            //inventoryUI.CloseInventory();
        }
    }
}
