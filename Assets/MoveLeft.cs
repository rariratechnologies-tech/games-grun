using UnityEngine;
using System.Collections;

public class DemrunController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Camera mainCamera;
    public float bounceForce = 10f; // How much the mouse bounces when jumping on Demrun
    
    [Header("Run Animation")]
    public Sprite[] runSprites; // Assign run frames in Inspector
    public float animationSpeed = 0.1f;

    private bool isVisible = false;
    private bool isDead = false;
    private Rigidbody2D rb;
    private Renderer objectRenderer;
    private SpriteRenderer spriteRenderer;
    private float initialY; // Store initial Y position
    private int currentFrame = 0;
    private float animationTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        objectRenderer = GetComponent<Renderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialY = transform.position.y;
        
        // Disable Animator if present — we use code-driven sprites instead
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Auto-find camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        // Make sure we have proper tag for detection
        if (string.IsNullOrEmpty(gameObject.tag))
        {
            gameObject.tag = "Enemy";
        }

        // Ensure we have a valid collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.Log("Adding BoxCollider2D to Demrun because none was found");
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f); // Adjust size based on your sprite
        }
    }

    void Update()
    {
        if (isDead) return;

        // Check if object is visible
        CheckVisibility();
        
        if (isVisible)
        {
            // Move left while maintaining Y position
            Vector3 newPosition = transform.position;
            newPosition.x -= moveSpeed * Time.deltaTime;
            newPosition.y = initialY; // Keep Y position constant
            transform.position = newPosition;
            
            // Animate run sprites
            if (runSprites != null && runSprites.Length > 0 && spriteRenderer != null)
            {
                animationTimer += Time.deltaTime;
                if (animationTimer >= animationSpeed)
                {
                    currentFrame = (currentFrame + 1) % runSprites.Length;
                    spriteRenderer.sprite = runSprites[currentFrame];
                    animationTimer = 0f;
                }
            }
        }
    }
    
    void CheckVisibility()
    {
        if (mainCamera != null && objectRenderer != null)
        {
            // Get the bounds of the renderer
            Bounds bounds = objectRenderer.bounds;
            
            // Check if any corner of the bounds is visible
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
            
            // Log when visibility changes
            if (wasVisible != isVisible)
            {
                Debug.Log(gameObject.name + " visibility changed to: " + isVisible);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Mouse"))
        {
            // Get collision details
            Vector2 contactPoint = collision.contacts[0].point;
            Vector2 center = GetComponent<Collider2D>().bounds.center;
            float colliderHeight = GetComponent<Collider2D>().bounds.size.y;

            // More reliable way to detect if mouse is above
            bool isHitFromAbove = contactPoint.y > center.y + (colliderHeight * 0.25f);
            
            if (isHitFromAbove)
            {
                // Get the mouse rigidbody
                Rigidbody2D mouseRb = collision.gameObject.GetComponent<Rigidbody2D>();
                
                // Only count as "from above" if the mouse is falling or moving down
                if (mouseRb != null && mouseRb.linearVelocity.y <= 0)
                {
                    Debug.Log("Mouse jumped on Demrun from above!");
                    
                    // Bounce the mouse up
                    mouseRb.linearVelocity = new Vector2(mouseRb.linearVelocity.x, bounceForce);
                    
                    // Start squash animation and die
                    StartCoroutine(SquashAndDie());
                }
                else
                {
                    // Player dies - hit from side while in air
                    Debug.Log("Game Over - Hit Demrun from side while in air");
                    TriggerGameOver(collision.gameObject);
                }
            }
            else
            {
                // Player dies - hit from side
                Debug.Log("Game Over - Hit Demrun from side");
                TriggerGameOver(collision.gameObject);
            }
        }
    }
    
    // Also handle trigger colliders
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Mouse"))
        {
            // Simple y-position check for trigger colliders
            bool isHitFromAbove = other.transform.position.y > transform.position.y + 0.5f;
            
            if (isHitFromAbove)
            {
                // Get the mouse rigidbody
                Rigidbody2D mouseRb = other.GetComponent<Rigidbody2D>();
                
                // Only count as "from above" if mouse is falling down
                if (mouseRb != null && mouseRb.linearVelocity.y <= 0)
                {
                    Debug.Log("Mouse jumped on Demrun from above (trigger)!");
                    
                    // Bounce the mouse up
                    mouseRb.linearVelocity = new Vector2(mouseRb.linearVelocity.x, bounceForce);
                    
                    // Start squash animation and die
                    StartCoroutine(SquashAndDie());
                }
                else
                {
                    // Player dies
                    Debug.Log("Game Over - Hit Demrun from side while in air (trigger)");
                    TriggerGameOver(other.gameObject);
                }
            }
            else
            {
                // Player dies
                Debug.Log("Game Over - Hit Demrun from side (trigger)");
                TriggerGameOver(other.gameObject);
            }
        }
    }

    // New method to handle game over properly
    private void TriggerGameOver(GameObject mouse)
    {
        // Try to call the GameOver method on the MoveRight component
        MoveRight moveRightScript = mouse.GetComponent<MoveRight>();
        if (moveRightScript != null)
        {
            moveRightScript.GameOver();
        }
        else
        {
            // Fallback if MoveRight script can't be found for some reason
            Debug.LogWarning("MoveRight script not found on mouse object. Using direct destroy.");
            Destroy(mouse);
        }
    }

    private IEnumerator SquashAndDie()
    {
        isDead = true;
        Debug.Log("Demrun squash animation started");

        // Disable Animator if present to prevent conflicts
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Squash animation parameters
        float squashDuration = 0.2f;
        float pauseDuration = 0.1f;
        float timer = 0f;

        // Store original scale
        Vector3 originalScale = transform.localScale;
        
        // Target scale - wide and flat
        Vector3 targetScale = new Vector3(originalScale.x * 1.5f, originalScale.y * 0.2f, originalScale.z);

        // Perform the squash animation
        while (timer < squashDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / squashDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure we reach the exact target scale
        transform.localScale = targetScale;

        // Pause briefly to show the squashed state
        yield return new WaitForSeconds(pauseDuration);
        
        // Destroy the Demrun
        Destroy(gameObject);
    }
}