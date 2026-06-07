using UnityEngine;

public class MobileControls : MonoBehaviour
{
    private MoveRight moveRightScript;

    void Start()
    {
        // Get reference to the MoveRight script attached to the same object
        moveRightScript = GetComponent<MoveRight>();
        
        if (moveRightScript == null)
        {
            Debug.LogError("MobileControls: MoveRight script not found on this GameObject!");
        }
    }

    void Update()
    {
        // Handle touch input (for mobile)
        if (Input.touchCount > 0)
        {
            // Loop through all touches
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                // Check if this is a new touch
                if (touch.phase == TouchPhase.Began)
                {
                    // Call the Jump method on the MoveRight script
                    if (moveRightScript != null && moveRightScript.isGrounded)
                    {
                        Debug.Log("Touch detected - Jumping!");
                        moveRightScript.Jump();
                    }
                }
            }
        }
        
        // Also keep keyboard input for testing in editor
        if (Input.GetKeyDown(KeyCode.Space) && moveRightScript != null && moveRightScript.isGrounded)
        {
            moveRightScript.Jump();
        }
    }
}