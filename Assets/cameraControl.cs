using UnityEngine;

public class cameraControl : MonoBehaviour
{
    public Transform playerTransform; // Player's transform
    private Vector3 cameraOffset;     // Offset distance between the player and camera
    public float smoothFactor = 0.5f; // Smoothness of camera movement

    // Start is called before the first frame update
    void Start()
    {
        // Calculate initial offset
        cameraOffset = transform.position - playerTransform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Update the position of the camera to maintain the offset from the player
        Vector3 newPos = playerTransform.position + cameraOffset;
        transform.position = Vector3.Slerp(transform.position, newPos, smoothFactor);

        // Rotate the camera to match the player's rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation, smoothFactor);
    }
}
