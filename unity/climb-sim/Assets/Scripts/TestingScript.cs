using UnityEngine;

public class TestingScript : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("AWAKE IS WORKING!");
    }

    void Update()
    {
        if (Input.anyKey)
        {
            Debug.Log("A key is being pressed!");
        }
    }
}