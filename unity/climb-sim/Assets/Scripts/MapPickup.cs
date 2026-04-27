using UnityEngine;

public class MapPickup : MonoBehaviour
{
    [SerializeField] private GameObject mapVisual;
    [SerializeField] private GameObject minimapCanvas;
    [SerializeField] private ChatUIManager chatUIManager;
    [SerializeField] private string promptMessage = "Press E to take map";

    private bool playerNearby;
    private bool hasMap;

    private void Start()
    {
        if (minimapCanvas != null)
            minimapCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!playerNearby) return;

        if (Input.GetKeyDown(KeyCode.E) && !hasMap)
        {
            PickUpMap();
        }
    }

    private void PickUpMap()
    {
        hasMap = true;

        if (mapVisual != null)
            mapVisual.SetActive(false);

        if (minimapCanvas != null)
            minimapCanvas.SetActive(true);

        Debug.Log("Map picked up.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNearby = true;
        Debug.Log(promptMessage);
        if (chatUIManager != null)
            chatUIManager.ShowSystemMessage(promptMessage);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNearby = false;
    }
}
