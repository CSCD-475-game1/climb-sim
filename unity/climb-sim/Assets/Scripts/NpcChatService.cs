using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatRequest
{
    public string playerText;
    public string npcName;
}

[Serializable]
public class ChatResponse
{
    public string reply;
}

public class NpcChatService : MonoBehaviour
{
    public static NpcChatService Instance { get; private set; }

    [Header("Server")]
    [SerializeField] private string apiUrl = "http://localhost:3000/npc-chat";
    [SerializeField] private float requestTimeoutSeconds = 3f;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator GetReply(string playerText, string npcName, Action<string> onReply)
    {
        yield return StartCoroutine(TryServerReply(playerText, npcName, onReply));
    }

    private IEnumerator TryServerReply(string playerText, string npcName, Action<string> onReply)
    {
        ChatRequest requestData = new ChatRequest
        {
            playerText = playerText,
            npcName = npcName
        };

        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.timeout = Mathf.CeilToInt(requestTimeoutSeconds);

            yield return request.SendWebRequest();

            bool failed =
                request.result != UnityWebRequest.Result.Success ||
                string.IsNullOrWhiteSpace(request.downloadHandler.text);

            if (failed)
            {
                Debug.LogWarning($"Server unavailable, using fallback. Error: {request.error}");
                onReply?.Invoke(NpcRuleEngine.Instance.GetReply(npcName, playerText));
                //onReply?.Invoke(GetFallbackReply(playerText, npcName));
                yield break;
            }

            ChatResponse response = null;

            try
            {
                response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Bad server response, using fallback. {e.Message}");
            }

            if (response == null || string.IsNullOrWhiteSpace(response.reply))
            {
                 //onReply?.Invoke(GetFallbackReply(playerText, npcName));
                 onReply?.Invoke(NpcRuleEngine.Instance.GetReply(npcName, playerText));
                yield break;
            }

            onReply?.Invoke(response.reply);
        }
    }

    private string GetFallbackReply(string playerText, string npcName)
    {
        string lower = playerText.ToLowerInvariant();

        if (ContainsAny(lower, "where", "go", "direction", "which way"))
            return "Try heading uphill and watch for the fallen log.";

        if (ContainsAny(lower, "river", "water", "stream"))
            return "Stay out of the water. Cross at the log if you can.";

        if (ContainsAny(lower, "lost", "help", "stuck"))
            return "I think you're still near the trail. Look for landmarks and keep moving uphill.";

        if (ContainsAny(lower, "okay", "ok", "thanks", "thank you"))
            return "Okay. Text me again when you reach the crossing.";

        if (ContainsAny(lower, "name", "who are you"))
            return $"I'm {npcName}. I got separated a little while ago.";

        return "I can't tell exactly where you are, but keep looking for the trail and avoid the river.";
    }

    private bool ContainsAny(string text, params string[] patterns)
    {
        foreach (string pattern in patterns)
        {
            if (text.Contains(pattern))
                return true;
        }
        return false;
    }
}
