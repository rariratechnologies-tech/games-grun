using UnityEngine;

public class ArrowController : MonoBehaviour
{
    private float speed = 5f;
    private bool isInitialized = false;
    private static int instanceCounter = 0;
    private int instanceID;
    
    // Movement direction - defaults to LEFT (negative X)
    private Vector3 moveDirection = Vector3.left;
    
    // Add these to control appearance
    private SpriteRenderer spriteRenderer;
    private float distanceTraveled = 0f;
    
    // Constants to ensure visibility
    private const float MAX_LIFETIME = 10f; // Maximum time arrow can exist - reduced from 20f to 10f
    private float lifetime = 0f;
    
    void Awake()
    {
        instanceID = ++instanceCounter;
        Debug.Log($"ArrowController {instanceID} Awake()");
        
        // Get the sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"Arrow {instanceID} has no SpriteRenderer! Adding one...");
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            // Create a simple fallback sprite
            Texture2D texture = new Texture2D(8, 2);
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    texture.SetPixel(x, y, Color.red);
                }
            }
            texture.Apply();
            
            // Create a sprite from the texture
            Sprite arrowSprite = Sprite.Create(texture, new Rect(0, 0, 8, 2), new Vector2(0.75f, 0.5f), 100);
            spriteRenderer.sprite = arrowSprite;
        }
        
        // Make sure the arrow has the "Arrow" tag for collision detection
        gameObject.tag = "Arrow";
        
        // If the arrow has a renderer but no sprite, create one
        if (spriteRenderer.sprite == null)
        {
            Debug.LogError($"Arrow {instanceID} SpriteRenderer has no sprite assigned! Creating one...");
            
            // Create a simple fallback sprite
            Texture2D texture = new Texture2D(8, 2);
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    texture.SetPixel(x, y, Color.red);
                }
            }
            texture.Apply();
            
            // Create a sprite from the texture
            Sprite arrowSprite = Sprite.Create(texture, new Rect(0, 0, 8, 2), new Vector2(0.75f, 0.5f), 100);
            spriteRenderer.sprite = arrowSprite;
        }
        
        // Display the position to help with debugging
        Debug.Log($"Arrow {instanceID} initial position: {transform.position}");
        
        // Clean up duplicate controllers
        ArrowController[] controllers = GetComponents<ArrowController>();
        if (controllers.Length > 1)
        {
            Debug.LogWarning($"Multiple ArrowControllers found on GameObject! Count: {controllers.Length}");
            
            // Keep only this one if it's the newest
            if (instanceID == controllers.Length) // I'm the newest
            {
                for (int i = 0; i < controllers.Length - 1; i++)
                {
                    Debug.Log($"Destroying older ArrowController {i+1}");
                    Destroy(controllers[i]);
                }
            }
        }
    }

    void Start()
    {
        Debug.Log($"ArrowController {instanceID} Start() called with speed: {speed}, initialized: {isInitialized}");
        
        // Ensure the object is active and visible
        gameObject.SetActive(true);
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            // Make arrow bright red for better visibility
            spriteRenderer.color = Color.red;
            spriteRenderer.sortingOrder = 100; // Very high sorting order
            Debug.Log($"Arrow {instanceID} sprite enabled: {spriteRenderer.enabled}");
        }
        
        // Ensure the arrow is large enough to see
        if (transform.localScale.x < 1.0f || transform.localScale.y < 0.5f)
        {
            Debug.LogWarning($"Arrow {instanceID} is too small, increasing scale");
            transform.localScale = new Vector3(2.0f, 0.8f, 1f);
        }
        
        // Make sure Z position is 0 for 2D visibility
        Vector3 position = transform.position;
        if (position.z != 0)
        {
            position.z = 0;
            transform.position = position;
            Debug.Log($"Fixed Arrow {instanceID} Z position to 0");
        }
        
        // Add collider if missing
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(3f, 0.5f);
            Debug.Log($"Added missing collider to Arrow {instanceID}");
        }
    }

    void Update()
{
    // Always increment lifetime
    lifetime += Time.deltaTime;
    
    // Destroy if lived too long
    if (lifetime > MAX_LIFETIME)
    {
        Debug.Log($"Arrow {instanceID} lived too long, destroying");
        Destroy(gameObject);
        return;
    }
    
    if (isInitialized)
    {
        // Store old position for debugging
        Vector3 oldPos = transform.position;
        
        // Move the arrow based on direction and speed
        Vector3 newPosition = transform.position + (moveDirection * speed * Time.deltaTime);
        transform.position = newPosition;
        
        // Calculate distance traveled
        distanceTraveled += Vector3.Distance(oldPos, transform.position);
        
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Mouse") ?? GameObject.FindGameObjectWithTag("Player");
        
        // Only enable collider when close to player
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            
            // Get the collider
            Collider2D arrowCollider = GetComponent<Collider2D>();
            
            if (arrowCollider != null)
            {
                // Only enable collision when within 2 units of player
                if (distanceToPlayer <= 2.0f)
                {
                    arrowCollider.enabled = true;
                }
                else
                {
                    arrowCollider.enabled = false;
                }
            }
        }
        
        // Log movement more frequently for debugging
        if (Time.frameCount % 10 == 0) // Log every 10 frames
        {
            Debug.Log($"Arrow {instanceID} moving: position = {transform.position}, speed = {speed}, direction = {moveDirection}");
            
            // Check if sprite is still enabled and visible
            if (spriteRenderer != null)
            {
                Debug.Log($"Arrow {instanceID} sprite enabled: {spriteRenderer.enabled}, sorting order: {spriteRenderer.sortingOrder}");
            }
        }
    }
    else
    {
        // Log once per second to avoid flooding the console
        if (Time.frameCount % 60 == 0)
            Debug.LogWarning($"Arrow {instanceID} not initialized yet, waiting for SetSpeed()");
    }
}

    public void SetSpeed(float newSpeed)
    {
        Debug.Log($"ArrowController {instanceID} SetSpeed({newSpeed}) called");
        this.speed = newSpeed;
        this.isInitialized = true;
        Debug.Log($"ArrowController {instanceID} speed set to: {newSpeed} and initialized = {isInitialized}");
        
        // Make sure the arrow is visible when initialized
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log($"Arrow {instanceID} sprite enabled during SetSpeed");
        }
    }
    
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        Debug.Log($"Arrow {instanceID} direction set to: {moveDirection}");
    }
    
    // Handle collisions with the player
    // In ArrowController.cs, replace the OnTriggerEnter2D method

void OnTriggerEnter2D(Collider2D other)
{
    // Only process collisions with the player
    if (other.CompareTag("Mouse") || other.CompareTag("Player"))
    {
        // Get positions
        Vector2 playerPos = other.transform.position;
        Vector2 arrowPos = transform.position;
        
        // Calculate ACTUAL horizontal distance
        float horizontalDistance = Mathf.Abs(playerPos.x - arrowPos.x);
        
        // Debug log the distance
        Debug.Log($"Arrow {instanceID} distance from player: {horizontalDistance}");
        
        // Only register hit if very close (within 1 unit)
        if (horizontalDistance <= 1.0f)
        {
            // Check if player is jumping over
            if (playerPos.y > arrowPos.y + 0.5f)
            {
                // Player is above arrow - safe!
                Debug.Log($"Player jumped safely over arrow {instanceID}");
                return;
            }
            
            // Hit confirmed - player dies
            Debug.Log($"HIT CONFIRMED - Arrow {instanceID} hit player at close range ({horizontalDistance})");
            
            // Try to find the player controller to call its GameOver method
            MoveRight playerController = other.GetComponent<MoveRight>();
            if (playerController != null)
            {
                playerController.GameOver();
            }
        }
        else
        {
            // Arrow is too far away - ignore collision completely
            Debug.Log($"FALSE COLLISION IGNORED - Arrow {instanceID} is too far from player ({horizontalDistance})");
        }
    }
}
    
    // Public method to check if initialized (for camera)
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    // For debugging
    void OnBecameInvisible()
    {
        Debug.Log($"Arrow {instanceID} became invisible to camera");
    }
    
    void OnBecameVisible()
    {
        Debug.Log($"Arrow {instanceID} became visible to camera");
    }
}