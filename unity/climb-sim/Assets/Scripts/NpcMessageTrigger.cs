
using UnityEngine;
using UnityEngine.SceneManagement;

public class NpcMessageTrigger : MonoBehaviour
{
    [SerializeField] private NpcDialogueTarget npc;
    [SerializeField] private string message;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;
        if (npc == null) return;

        npc.SendAutoMessage(message);

        hasTriggered = true;
    }
}
