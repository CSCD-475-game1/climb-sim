using UnityEngine;

public class NpcDialogueTarget : MonoBehaviour
{
    [SerializeField] private string displayName = "Ranger";
    public string DisplayName => displayName;

    public string GetReply(string playerText)
    {
        string t = playerText.ToLower();

        if (t.Contains("hello") || t.Contains("hi"))
            return "Hey. Need directions?";
        if (t.Contains("car") || t.Contains("vehicle"))
            return "That vehicle belongs to the ranger station.";
        if (t.Contains("trail"))
            return "The trail starts past the parking lot.";

        return "I don't know much about that.";
    }
}
