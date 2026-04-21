using UnityEngine;

public class NpcDialogueTarget : MonoBehaviour
{
    [SerializeField] private string displayName = "Ranger";
    public string DisplayName => displayName;

    private ChatUIManager chatUI;

    private void Awake()
    {
        chatUI = FindFirstObjectByType<ChatUIManager>();
    }


    public void SendAutoMessage(string message)
    {
        // This method can be used to send automatic messages to the player, such as hints or updates.
        Debug.Log($"Auto message to player: {message}");
        chatUI.ShowNpcMessage(displayName, message);
    }

    public string GetReply(string playerText)
    {
        string lower = playerText.ToLower();

        if (lower.Contains("where") || lower.Contains("go"))
            return "Keep uphill and look for the log crossing.";

        if (lower.Contains("river") || lower.Contains("water"))
            return "Don't go into the river. Cross on the log.";

        if (lower.Contains("okay") || lower.Contains("ok"))
            return "Good. Tell me when you reach the other side.";

        if (lower.Contains("help") || lower.Contains("lost"))
            return "I think the trail continues past the rocks.";

        return "I can text, but I can't see you well from here. Look for landmarks.";
    }

    public string GetFallbackReply(string playerText)
    {
        if (NpcRuleEngine.Instance == null)
            return "...";

        return NpcRuleEngine.Instance.GetReply(DisplayName, playerText);
    }

}
