using UnityEngine;

public class LoopBackground : MonoBehaviour
{
    public float length;  // ✅ this will show in Inspector
    public GameObject cameraObj; // ✅ this will show in Inspector

    private void Update()
    {
        if (cameraObj.transform.position.x > transform.position.x + length)
        {
            transform.position += new Vector3(2 * length, 0, 0);
        }
    }
}
