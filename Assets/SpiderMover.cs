using UnityEngine;

public class SpiderMover : MonoBehaviour
{
    public float speed = 2f;
    public Camera mainCamera; // Reference to the main camera

    private bool hasStartedMoving = false;
    private Renderer rend;
    private SpriteRenderer spriteRenderer;
    private Vector3 viewportPoint;

    void Start()
    {
        rend = GetComponent<Renderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-find camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        // Ensure the spider has the "Spider" tag
        gameObject.tag = "Spider";
        
        // Make sure the spider has a collider
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("Added BoxCollider2D to Spider because none was found");
        }
    }

    void Update()
    {
        if (!hasStartedMoving)
        {
            // Convert the spider's position to viewport coordinates
            viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
            
            // Check if the spider is within the camera's view (0,0 is bottom left, 1,1 is top right)
            bool isVisible = viewportPoint.x > 0 && viewportPoint.x < 1 && 
                             viewportPoint.y > 0 && viewportPoint.y < 1 && 
                             viewportPoint.z > 0;
                             
            if (isVisible)
            {
                hasStartedMoving = true;
                Debug.Log("Spider started moving - detected in camera view");
            }
        }
        else
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);

            // Optional: destroy if too far left
            if (transform.position.x < -20f)
            {
                Debug.Log("Spider moved off-screen, destroying");
                Destroy(gameObject);
            }
        }
    }
    
    // Visual feedback and behavior when spider is stepped on
    public void OnSteppedOn()
    {
        // Optional: Add death animation or effect
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray; // Make it gray to indicate it's dying
        }
        
        // Disable the collider so it can't hurt the player anymore
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Stop the spider
        speed = 0;
        
        // Destroy after a short delay (could be used for death animation)
        Destroy(gameObject, 0.2f);
    }
}