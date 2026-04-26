using UnityEngine;
using UnityEngine.SceneManagement;

public class RiverTrigger : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered river");
        
        CharacterController controller = other.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        other.transform.position = respawnPoint.position;

        if (controller != null)
            controller.enabled = true;

        SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player exited river");
    }
}
