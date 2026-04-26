using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChatUIManager : MonoBehaviour
{

    [SerializeField] private AudioSource notificationAudio;
    [SerializeField] private AudioClip messageDing;
    [SerializeField] private float delayMin = 1.0f;
    [SerializeField] private float delayMax = 4.0f;

    [Header("UI")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject messagePrefab;

    [Header("Keys")]
    [SerializeField] private KeyCode openChatKey = KeyCode.C;


    [SerializeField] private NpcDialogueTarget currentNpc;
    private bool awaitingReply;

    private void Start()
    {
        inputField.gameObject.SetActive(false);
        inputField.onSubmit.AddListener(HandleSubmit);

        if (panelGroup != null)
            panelGroup.alpha = 0.65f;

        ShowSystemMessage("Welcome to the Hiking Sim!");
        ShowSystemMessage("Press E to interact with objects.");
        ShowSystemMessage($"Press {openChatKey} to open chat.");
        ShowSystemMessage("Use WASD to move around. Space to jump.");
        ShowSystemMessage("TAB for inventory.");
        ShowSystemMessage("Press Esc to close chat.");
        ShowSystemMessage("Enjoy your hike!");
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

    public void OpenChat(NpcDialogueTarget npc)
    {
        currentNpc = npc;
        GameManager.Instance.SetMode(GameManager.PlayerMode.Chat);

        inputField.gameObject.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField();
        inputField.Select();

        ShowSystemMessage($"Chat opened with {npc.DisplayName}.");
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
        AddMessage($"<color=#B8E1FF>System:</color> {message}", false);
    }

    public void ShowNpcMessage(string npcName, string message)
    {
        AddMessage($"<color=#FFD37A>{npcName}:</color> {message}", true);
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

        if (awaitingReply)
            return;

        string trimmed = text.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return;

        AddMessage($"<color=#A7FFA7>You:</color> {trimmed}", false);
        StartCoroutine(ScrollToBottomRoutine());


        inputField.text = "";
        inputField.ActivateInputField();
        inputField.Select();

        if (currentNpc == null)
        {
            ShowSystemMessage("No NPC is connected to this chat.");
            return;
        }

        StartCoroutine(RequestNpcReplyRoutine(trimmed));
    }

    private IEnumerator RequestNpcReplyRoutine(string playerText)
    {
        awaitingReply = true;

        string npcName = currentNpc != null ? currentNpc.DisplayName : "Hiker";

        float delay = Random.Range(delayMin, delayMax);
        yield return new WaitForSeconds(delay * 3f);

        ShowSystemMessage($"{npcName} is typing...");
        delay = Random.Range(delayMin, delayMax);
        yield return new WaitForSeconds(delay);


        yield return StartCoroutine(
            NpcChatService.Instance.GetReply(playerText, npcName, (reply) =>
            {
                ShowNpcMessage(npcName, reply);
            })
        );
        awaitingReply = false;

        //Canvas.ForceUpdateCanvases();
        //scrollRect.verticalNormalizedPosition = 0f;

        inputField.ActivateInputField();
        inputField.Select();
    }


    private void AddMessage(string text, bool playSound = false)
    {
        GameObject entry = Instantiate(messagePrefab, contentRoot);
        TMP_Text label = entry.GetComponent<TMP_Text>();

        if (label != null)
            label.text = text;

        if (playSound && notificationAudio != null && messageDing != null)
        {
            notificationAudio.PlayOneShot(messageDing);
        }

        //Canvas.ForceUpdateCanvases();
        //scrollRect.verticalNormalizedPosition = 0f;

        StartCoroutine(ScrollToBottomRoutine());
    }


    private IEnumerator ScrollToBottomRoutine()
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);

        scrollRect.verticalNormalizedPosition = 0f;
        scrollRect.velocity = Vector2.zero;
    }
}
