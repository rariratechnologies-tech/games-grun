using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxEffectMultiplier = 0.5f; // How fast it moves
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier, 0f, 0f);
        lastCameraPosition = cameraTransform.position;
    }
}
