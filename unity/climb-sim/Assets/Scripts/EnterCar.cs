using UnityEngine;

public class EnterCar : MonoBehaviour
{
    [Header("FPS side")]
    public GameObject fpsControllerRoot;
    public Camera fpsCamera;
    public Behaviour[] fpsBehavioursToDisable;

    [Header("Car side")]
    public float enterDistance = 4f;

    public GameObject car;
    private OffRoadWheelCarController carController;

    private Camera carCamera;
    private AudioListener fpsListener;
    private AudioListener carListener;

    public bool isUnlocked = true;

    [Header("ChatUI")]
    [SerializeField] private ChatUIManager chatUI;

    [Header("Inventory")]
    public InventoryUIManager inventoryUI;

    private bool inCar = false;
    private bool loggedCar = false;
    private bool showingEnterPrompt = false;

    void Start()
    {
        fpsListener = fpsCamera != null ? fpsCamera.GetComponent<AudioListener>() : null;
        ForceStartOnFoot();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayMode)
            return; 

        TryFindCar();

        if (!inCar && car != null && isUnlocked)
        {
            float d = Vector3.Distance(fpsControllerRoot.transform.position, car.transform.position);

            if (d <= enterDistance)
            {
                if (!showingEnterPrompt)
                {
                    chatUI.ShowSystemMessage("Press E to enter vehicle or R to resupply from the vehicle.");

                    showingEnterPrompt = true;
                }
                
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Debug.Log("R key pressed.");
                    inventoryUI.DebugClearInventory();
                    inventoryUI.DebugFillInventory();
                            
                }

            }
            else
            {                
                //if (showingEnterPrompt)
                    //chatUI.ClearSystemMessage();
                //showingEnterPrompt = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed. In car: " + inCar);
            if (!inCar)
                TryEnter();
            else
                ExitVehicle();
        }  
    }

    void ForceStartOnFoot()
    {
        inCar = false;

        if (fpsCamera != null)
            fpsCamera.enabled = true;

        if (fpsListener != null)
            fpsListener.enabled = true;

        SetFPSControlsEnabled(true);

        if (carController != null)
            carController.canDrive = false;

        if (carCamera != null)
            carCamera.enabled = false;

        if (carListener != null)
            carListener.enabled = false;
    }

    void TryFindCar()
    {
        //Debug.Log("Trying to find car... " + (car != null ? "Car already assigned: " + car.name : "Car not assigned"));
        if (car != null && carController != null && carCamera != null)
            return;

        //car = GameObject.FindWithTag("Car");
        //if (car == null)
            //return;

        carController = car.GetComponent<OffRoadWheelCarController>();
        carCamera = FindNamedCamera(car, "CarCamera");

        if (carCamera != null)
            carListener = carCamera.GetComponent<AudioListener>();

        if (!loggedCar)
        {
            Debug.Log("Car found: " + car.name);
            Debug.Log("Car camera found: " + (carCamera != null ? carCamera.name : "null"));
            loggedCar = true;
        }

        if (carController != null)
            carController.canDrive = false;

        if (carCamera != null)
            carCamera.enabled = false;

        if (carListener != null)
            carListener.enabled = false;
    }

    Camera FindNamedCamera(GameObject root, string targetName)
    {
        Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras)
        {
            if (cam.name == targetName)
                return cam;
        }
        return null;
    }

    void TryEnter()
    {
        if (car == null || carController == null || carCamera == null || fpsControllerRoot == null || fpsCamera == null)
        {
            Debug.LogError("Missing car, car controller, car camera, fps controller root, or fps camera.");
            return;
        }

        float d = Vector3.Distance(fpsControllerRoot.transform.position, car.transform.position);
        if (d > enterDistance)
            return;

        EnterVehicle();
    }

    void EnterVehicle()
    {
        inCar = true;

        Vector3 enterPos = car.transform.position + car.transform.right * 4f + Vector3.up * 0.25f;
        fpsControllerRoot.transform.position = enterPos;


        // Enable car view first
        carCamera.enabled = true;
        if (carListener != null) carListener.enabled = true;

        // Then disable FPS view
        fpsCamera.enabled = false;
        if (fpsListener != null) fpsListener.enabled = false;

        // Disable FPS movement, but keep object alive
        SetFPSControlsEnabled(false);

        // Enable car movement
        carController.canDrive = true;



        Debug.Log($"Entered car. car transform: {car.transform.position}, player transform: {fpsControllerRoot.transform.position}. Cameras active: {Camera.allCamerasCount}");
    }

    void ExitVehicle()
    {
        inCar = false;

        // Disable car control/view
        carController.canDrive = false;
        if (carCamera != null) carCamera.enabled = false;
        if (carListener != null) carListener.enabled = false;

        // Keep FPS controls off while repositioning
        SetFPSControlsEnabled(false);

        CharacterController cc = fpsControllerRoot.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Put player beside car, slightly offset upward to avoid ground clipping
        Vector3 exitPos = car.transform.position + car.transform.right * 4f + Vector3.up * 0.25f;
        fpsControllerRoot.transform.position = exitPos;

        // Optional: face same direction as car
        Vector3 flatForward = car.transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude > 0.001f)
            fpsControllerRoot.transform.rotation = Quaternion.LookRotation(flatForward);

        if (cc != null) cc.enabled = true;

        // Re-enable FPS view
        if (fpsCamera != null) fpsCamera.enabled = true;
        if (fpsListener != null) fpsListener.enabled = true;

        // Re-enable FPS controls last
        SetFPSControlsEnabled(true);

        Debug.Log($"Exited car. car transform: {car.transform.position}, player transform: {fpsControllerRoot.transform.position}");
    }
    void SetFPSControlsEnabled(bool enabledState)
    {
        if (fpsBehavioursToDisable == null) return;

        foreach (Behaviour b in fpsBehavioursToDisable)
        {
            if (b != null)
                b.enabled = enabledState;
        }
    }
}
