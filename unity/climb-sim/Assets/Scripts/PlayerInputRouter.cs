using UnityEngine;

public class PlayerInputRouter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChatUIManager chatUI;
    [SerializeField] private InventoryUIManager inventoryUI;
    [SerializeField] private PlayerInventory playerInventory;

    private void Start()
    {
        if (chatUI == null)
            Debug.LogError("ChatUIManager reference is missing in PlayerInputRouter.");

        if (inventoryUI == null)
            Debug.LogError("InventoryUIManager reference is missing in PlayerInputRouter.");

        if (playerInventory == null)
            Debug.LogError("PlayerInventory reference is missing in PlayerInputRouter.");

        if (GameInputModeManager.Instance != null)
            GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Gameplay);
    }

    private void Update()
    {
        if (GameInputModeManager.Instance == null)
            return;

        switch (GameInputModeManager.Instance.CurrentMode)
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

    private void HandleGameplayInput()
    {
        HandleHotbarNumberInput();

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (playerInventory != null)
                playerInventory.UseSelectedItem();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (chatUI != null)
                chatUI.OpenChat();

            GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Chat);
            return;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryUI != null)
                inventoryUI.OpenInventory();

            GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Inventory);
            return;
        }
        if (inventoryUI != null)
            inventoryUI.RefreshFromInventory();
    }

    private void HandleChatInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.C))
        {
            if (chatUI != null)
                chatUI.CloseChat();

            GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Gameplay);
        }
    }

    private void HandleInventoryInput()
    {
        HandleHotbarNumberInput();

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (playerInventory != null)
                playerInventory.UseSelectedItem();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryUI != null)
                inventoryUI.CloseInventory();

            GameInputModeManager.Instance.SetMode(GameInputModeManager.InputMode.Gameplay);
        }
    }

    private void HandleHotbarNumberInput()
    {
        if (playerInventory == null)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            playerInventory.SelectHotbar(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            playerInventory.SelectHotbar(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            playerInventory.SelectHotbar(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            playerInventory.SelectHotbar(3);
    }
}
