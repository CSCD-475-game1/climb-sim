using UnityEngine;

public class NpcTalkTrigger : MonoBehaviour
{
    [SerializeField] private ChatUIManager chatUI;
    [SerializeField] private NpcDialogueTarget npc;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            chatUI.SetCurrentNpc(npc);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            chatUI.SetCurrentNpc(null);
    }
}
