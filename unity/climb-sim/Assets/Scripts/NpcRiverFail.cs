using UnityEngine;
using UnityEngine.SceneManagement;

public class NpcRiverFail : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform npcSpawn;
    [SerializeField] private MonoBehaviour npcMovementScript;
    [SerializeField] private MonoBehaviour playerMovementScript;

    private bool gameOverTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        //if (gameOverTriggered) return;

        //if (other.CompareTag("River"))
        //{
            //gameOverTriggered = true;
            //HandleRiverFail();
        //}
    }

    private void HandleRiverFail()
    {
        Debug.Log("NPC fell into river. Game over.");

        if (npcMovementScript != null) npcMovementScript.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        Invoke(nameof(RespawnBoth), 1.0f);
    }

    private void RespawnBoth()
    {
        if (player != null && playerSpawn != null)
            player.position = playerSpawn.position;

        if (npcSpawn != null)
            transform.position = npcSpawn.position;

        if (npcMovementScript != null) npcMovementScript.enabled = true;
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        gameOverTriggered = false;
    }
}
