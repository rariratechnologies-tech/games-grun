using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float speed = 5f;
    public Camera mainCamera;
    
    private bool isVisible = false;
    private Renderer objectRenderer;
    private Rigidbody2D rb;
    
    void Start()
    {
        // Make sure ball has the correct tag
        gameObject.tag = "Ball";
        
        // Get the renderer component
        objectRenderer = GetComponent<Renderer>();
        
        // Auto-find camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        // Make sure we have a collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.Log("Adding CircleCollider2D to Ball because none was found");
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = false;
        }
        else
        {
            // If collider exists, make sure it's not a trigger
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.isTrigger = false;
            }
        }
        
        // Make sure we have a rigidbody with the right settings
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("Adding Rigidbody2D to Ball because none was found");
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure the rigidbody - use Dynamic instead of Kinematic
        rb.bodyType = RigidbodyType2D.Dynamic; 
        rb.gravityScale = 1f; // Enable gravity
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Optional: prevent rolling
    }
    
    void Update()
    {
        // Check if object is visible
        CheckVisibility();
        
        // Only move when visible
        if (isVisible)
        {
            // Apply constant horizontal velocity
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
        }
        
        // Optional: Destroy the ball when it's off-screen to the left
        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
    
    void CheckVisibility()
    {
        // Same visibility check logic as before...
        if (mainCamera != null && objectRenderer != null)
        {
            Bounds bounds = objectRenderer.bounds;
            
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            corners[1] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            corners[3] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            corners[4] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            corners[6] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            corners[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);

            bool wasVisible = isVisible;
            isVisible = false;
            
            foreach (Vector3 corner in corners)
            {
                Vector3 viewportPoint = mainCamera.WorldToViewportPoint(corner);
                if (viewportPoint.x > 0 && viewportPoint.x < 1 && 
                    viewportPoint.y > 0 && viewportPoint.y < 1 && 
                    viewportPoint.z > 0)
                {
                    isVisible = true;
                    break;
                }
            }
            
            if (wasVisible != isVisible)
            {
                Debug.Log(gameObject.name + " visibility changed to: " + isVisible);
            }
        }
    }
    
    // Handle collisions with mouse
    // Handle collisions with mouse
void OnCollisionEnter2D(Collision2D collision)
{
    Debug.Log("Ball collision with: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
    
    if (collision.gameObject.CompareTag("Mouse"))
    {
        Debug.Log("Ball hit the mouse. Game Over!");
        
        // Call the GameOver method instead of destroying the mouse
        MoveRight moveRightScript = collision.gameObject.GetComponent<MoveRight>();
        if (moveRightScript != null)
        {
            moveRightScript.GameOver();
        }
        else
        {
            // Fallback to direct destroy if the script isn't found
            Debug.LogWarning("MoveRight script not found on mouse. Using direct destroy.");
            Destroy(collision.gameObject);
        }
    }
}

// Also handle trigger colliders if used
void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log("Ball trigger with: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");
    
    if (other.CompareTag("Mouse"))
    {
        Debug.Log("Ball hit the mouse (trigger). Game Over!");
        
        // Call the GameOver method instead of destroying the mouse
        MoveRight moveRightScript = other.gameObject.GetComponent<MoveRight>();
        if (moveRightScript != null)
        {
            moveRightScript.GameOver();
        }
        else
        {
            // Fallback to direct destroy if the script isn't found
            Debug.LogWarning("MoveRight script not found on mouse. Using direct destroy.");
            Destroy(other.gameObject);
        }
    }
}
}