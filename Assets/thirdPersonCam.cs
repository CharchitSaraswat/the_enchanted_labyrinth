using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform playerTransform;
    private Vector3 offset;

    void Start() {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (playerTransform != null)
        {
            offset = transform.position - playerTransform.position;
        }
        else
        {
            Debug.LogError("Player Transform is not set on the camera script");
        }
    }
    void Update()
    {
        // Update the position of the camera to follow the player with an offset
        transform.position = playerTransform.position + offset;

        // Optional: Add smoothing, input handling, etc.
    }
}