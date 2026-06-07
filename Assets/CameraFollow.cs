using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // the mouse to follow
    public Vector3 offset; // small distance offset
    public float smoothSpeed = 0.125f; // how smooth the camera follows

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = new Vector3(smoothedPosition.x, transform.position.y, transform.position.z);
        }
    }
}
