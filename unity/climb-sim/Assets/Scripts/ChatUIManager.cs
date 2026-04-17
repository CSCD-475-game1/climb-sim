using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject messagePrefab;

    [Header("Keys")]
    [SerializeField] private KeyCode openChatKey = KeyCode.C;


    private NpcDialogueTarget currentNpc;

    private void Start()
    {
        inputField.gameObject.SetActive(false);
        inputField.onSubmit.AddListener(HandleSubmit);

        if (panelGroup != null)
            panelGroup.alpha = 0.65f;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsGameplayMode && Input.GetKeyDown(openChatKey))
        {
            OpenChat();
            return;
        }

        if (GameManager.Instance.IsChatMode && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
            return;
        }
    }

    public void OpenChat()
    {
        GameManager.Instance.SetMode(GameManager.PlayerMode.Chat);

        inputField.gameObject.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void CloseChat()
    {
        inputField.text = "";
        inputField.DeactivateInputField();
        inputField.gameObject.SetActive(false);

        GameManager.Instance.SetMode(GameManager.PlayerMode.Gameplay);
    }

    public void ShowSystemMessage(string message)
    {
        AddMessage($"<color=#B8E1FF>System:</color> {message}");
    }

    public void ShowNpcMessage(string npcName, string message)
    {
        AddMessage($"<color=#FFD37A>{npcName}:</color> {message}");
    }
    public void SetCurrentNpc(NpcDialogueTarget npc)
    {
        currentNpc = npc;

        if (npc != null)
            ShowSystemMessage($"Press {openChatKey} to talk to {npc.DisplayName}.");
        else
            ShowSystemMessage("No one nearby to talk to.");
    }

    private void HandleSubmit(string text)
    {
        if (!GameManager.Instance.IsChatMode)
            return;

        string trimmed = text.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return;

        AddMessage($"<color=#A7FFA7>You:</color> {trimmed}");

        inputField.text = "";
        inputField.ActivateInputField();

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void AddMessage(string text)
    {
        GameObject entry = Instantiate(messagePrefab, contentRoot);
        TMP_Text label = entry.GetComponent<TMP_Text>();

        if (label != null)
            label.text = text;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
