using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MoveRight : MonoBehaviour
{
    public float speed = 7f;
    public float jumpForce = 15f;
    public float bounceMultiplier = 0.75f; // 
    public float spiderBounceMultiplier = 0.75f; // 
    public float demonBounceMultiplier = 0.75f; // Added specific multiplier for demon bounce
    
    [Header("Collision Detection")]
    public float verticalJumpDetectionThreshold = 0.5f; // How close mouse needs to be to consider it "above" an enemy
    public float arrowJumpDetectionPrecision = 0.1f; // Smaller value = more precise arrow detection
    public float arrowFallSpeed = 10f; // Speed at which arrow falls after being jumped on
    
    [Header("Fall Detection")]
    public float fallThreshold = -10f; // Y position at which the mouse is considered fallen off the screen
    
    [Header("Game Over Settings")]
    public GameObject gameOverPanel; // Assign this in the Inspector - the UI panel with restart button
    public static bool gameIsOver = false; // Static flag to check if game is over
    
    [Header("Audio")]
    public AudioClip jumpSound;       // Assign this in the Inspector
    private AudioSource audioSource;  // Will get component in Start()
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    public Sprite[] runSprites; // running frames
    public Sprite[] jumpSprites; // jump frames

    [Header("Debug")]
    public bool debugVisibility = true;
    public float debugInterval = 1f;

    private int currentFrame = 0;
    private float animationTimer = 0f;
    public float animationSpeed = 0.1f;

    private int jumpFrame = 0;
    private float jumpAnimationTimer = 0f;
    public float jumpAnimationSpeed = 0.1f;
    private float visibilityDebugTimer = 0f;
    private float lastMovementFrameTime = -1f;
    private string lastAssignedSpriteName = "";
    private bool wasGrounded = true; // track ground-to-air transition
    private float jumpCooldown = 0f; // prevents re-grounding immediately after jump
    private bool hasShownJumpMiddleSprite = false;

    public bool isGrounded = false;
    
    // Add this to prevent multiple collisions in a single frame
    private bool processingDemonCollision = false;
    private bool processingArrowCollision = false;
    
    // Track enemies we've already jumped on to prevent double-processing
    private System.Collections.Generic.HashSet<int> processedEnemies = new System.Collections.Generic.HashSet<int>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        rb.freezeRotation = true;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        transform.rotation = Quaternion.identity;
        Time.timeScale = 1f;

        // Apply frictionless physics material so mouse slides off walls instead of sticking
        PhysicsMaterial2D slippery = new PhysicsMaterial2D("MouseNoFriction");
        slippery.friction = 0f;
        slippery.bounciness = 0f;
        boxCollider.sharedMaterial = slippery;
        
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Make sure the mouse has the "Mouse" tag
        gameObject.tag = "Mouse";
        
        // Reset game over state when starting/restarting
        gameIsOver = false;
        
        // Make sure game over panel is hidden at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        LogStartupSpriteState();
    }

    void Update()
    {
        if (debugVisibility)
        {
            visibilityDebugTimer += Time.unscaledDeltaTime;
            if (visibilityDebugTimer >= debugInterval)
            {
                LogRuntimeVisibility(gameIsOver ? "UPDATE STOPPED GAME OVER" : "UPDATE");
                visibilityDebugTimer = 0f;
            }
        }

        if (Time.timeScale == 0f && !gameIsOver)
        {
            Debug.LogWarning($"MOVEMENT STOPPED: Time.timeScale is 0. The game is paused. pos={transform.position}, velocity={(rb != null ? rb.linearVelocity : Vector2.zero)}");
            return;
        }

        // Only process movement if game is not over
        if (!gameIsOver)
        {
            MoveForward(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }

            // Tick down jump cooldown
            if (jumpCooldown > 0f)
                jumpCooldown -= Time.deltaTime;

            UpdateAnimation();
            
            // Check if the mouse has fallen below the threshold
            CheckForFall();
        }
        
        // Add keyboard shortcut for restart (optional)
        if (gameIsOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        rb.freezeRotation = true;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;

        if (!gameIsOver && Time.timeScale > 0f)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            MoveForward(Time.fixedDeltaTime);
        }
    }

    private void MoveForward(float deltaTime)
    {
        if (deltaTime <= 0f || Time.time == lastMovementFrameTime)
        {
            return;
        }

        Vector3 nextPosition = transform.position + Vector3.right * speed * deltaTime;

        if (rb != null && rb.simulated)
        {
            rb.position = new Vector2(nextPosition.x, rb.position.y);
        }
        else
        {
            transform.position = nextPosition;
        }

        lastMovementFrameTime = Time.time;
    }
    
    // New method to check if the mouse has fallen off the screen
    void CheckForFall()
    {
        if (transform.position.y < fallThreshold && !gameIsOver)
        {
            Debug.Log($"Mouse fell off the screen. Game Over! pos={transform.position}, fallThreshold={fallThreshold}");
            GameOver();
        }
    }
    
    public void GameOver()
    {
        Debug.Log($"GAME OVER called. pos={transform.position}, sprite={DescribeSprite(spriteRenderer != null ? spriteRenderer.sprite : null)}, rendererEnabled={(spriteRenderer != null && spriteRenderer.enabled)}, active={gameObject.activeInHierarchy}");

        // Set game over flag
        gameIsOver = true;
        
        // Call GameManager if it exists
        GameManager manager = FindObjectOfType<GameManager>();
        if (manager != null)
        {
            manager.GameOver();
        }
        else
        {
            // Fallback to old behavior
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }
        
        // Stop the character
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Hide the mouse rather than destroying it
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void Jump()
{
    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    isGrounded = false;
    jumpCooldown = 0.2f; // prevent re-grounding for 0.2 seconds
    
    // Play jump sound if assigned
    if (jumpSound != null && audioSource != null)
    {
        float volume = 0.5f; // Set volume between 0.0f (silent) and 1.0f (full volume)
        audioSource.PlayOneShot(jumpSound, volume);
    }
}

    void UpdateAnimation()
    {
        // Override isGrounded if we're clearly in the air (moving up or falling fast)
        bool trulyGrounded = isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.5f && jumpCooldown <= 0f;

        // Debug every second to track state
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"ANIM STATE: pos={transform.position}, isGrounded={isGrounded}, vel={rb.linearVelocity}, jumpCooldown={jumpCooldown:F2}, trulyGrounded={trulyGrounded}, rendererEnabled={(spriteRenderer != null && spriteRenderer.enabled)}, currentSprite={DescribeSprite(spriteRenderer != null ? spriteRenderer.sprite : null)}, runSprites={(runSprites != null ? runSprites.Length : -1)}, jumpSprites={(jumpSprites != null ? jumpSprites.Length : -1)}");
        }

        if (trulyGrounded)
        {
            // Reset jump frame when landing
            jumpFrame = 0;
            jumpAnimationTimer = 0f;
            wasGrounded = true;
            hasShownJumpMiddleSprite = false;

            animationTimer += Time.deltaTime;
            if (animationTimer >= animationSpeed)
            {
                if (runSprites == null || runSprites.Length == 0)
                {
                    Debug.LogError("SPRITE DEBUG: runSprites array is empty or missing. Mouse cannot show running animation.");
                    animationTimer = 0f;
                    return;
                }

                currentFrame = (currentFrame + 1) % runSprites.Length;
                AssignSprite(runSprites[currentFrame], $"runSprites[{currentFrame}]");
                animationTimer = 0f;
            }
        }
        else
        {
            if (jumpSprites != null && jumpSprites.Length > 0)
            {
                int startSpriteIndex = 0;
                int middleSpriteIndex = Mathf.Min(1, jumpSprites.Length - 1);
                int endSpriteIndex = Mathf.Min(2, jumpSprites.Length - 1);

                // Immediately show first jump sprite on transition
                if (wasGrounded)
                {
                    wasGrounded = false;
                    jumpFrame = startSpriteIndex;
                    jumpAnimationTimer = 0f;
                    hasShownJumpMiddleSprite = false;
                    AssignSprite(jumpSprites[startSpriteIndex], $"jumpSprites[{startSpriteIndex}] start");
                    Debug.Log($"Jump started - showing jump start sprite. Total jump sprites: {jumpSprites.Length}, assigned={DescribeSprite(jumpSprites[startSpriteIndex])}");
                }
                else if (jumpSprites.Length >= 3)
                {
                    jumpAnimationTimer += Time.deltaTime;

                    if (!hasShownJumpMiddleSprite && jumpAnimationTimer >= jumpAnimationSpeed)
                    {
                        jumpFrame = middleSpriteIndex;
                        AssignSprite(jumpSprites[middleSpriteIndex], $"jumpSprites[{middleSpriteIndex}] middle");
                        hasShownJumpMiddleSprite = true;
                    }

                    if (hasShownJumpMiddleSprite)
                    {
                        int selectedJumpIndex = rb.linearVelocity.y < -0.1f ? endSpriteIndex : middleSpriteIndex;
                        AssignSprite(jumpSprites[selectedJumpIndex], $"jumpSprites[{selectedJumpIndex}] airborne");
                    }
                }
                else
                {
                    int selectedJumpIndex = Mathf.Min(1, jumpSprites.Length - 1);
                    AssignSprite(jumpSprites[selectedJumpIndex], $"jumpSprites[{selectedJumpIndex}] fallback");
                }
            }
            else
            {
                Debug.LogWarning("jumpSprites array is EMPTY! Assign jump sprites in the Inspector on the Mouse object.");
            }
        }
    }

    private void AssignSprite(Sprite sprite, string source)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"SPRITE DEBUG: Cannot assign {source}; SpriteRenderer is missing on {name}.");
            return;
        }

        if (sprite == null)
        {
            Debug.LogError($"SPRITE DEBUG: {source} is NULL. Keeping current sprite={DescribeSprite(spriteRenderer.sprite)}. Check this slot in the Mouse MoveRight component.");
            LogRuntimeVisibility("NULL SPRITE ASSIGNMENT");
            return;
        }

        spriteRenderer.sprite = sprite;

        if (debugVisibility && lastAssignedSpriteName != sprite.name)
        {
            Debug.Log($"SPRITE DEBUG: assigned {source} -> {DescribeSprite(sprite)} at pos={transform.position}");
            lastAssignedSpriteName = sprite.name;
        }
    }

    private void LogStartupSpriteState()
    {
        if (!debugVisibility) return;

        Debug.Log($"SPRITE DEBUG START: object={name}, active={gameObject.activeInHierarchy}, pos={transform.position}, scale={transform.localScale}, rendererEnabled={(spriteRenderer != null && spriteRenderer.enabled)}, rendererSprite={DescribeSprite(spriteRenderer != null ? spriteRenderer.sprite : null)}, sortingOrder={(spriteRenderer != null ? spriteRenderer.sortingOrder : -999)}");
        LogSpriteArray("runSprites", runSprites);
        LogSpriteArray("jumpSprites", jumpSprites);
        LogRuntimeVisibility("START");
    }

    private void LogSpriteArray(string label, Sprite[] sprites)
    {
        if (sprites == null)
        {
            Debug.LogError($"SPRITE DEBUG START: {label} is NULL.");
            return;
        }

        Debug.Log($"SPRITE DEBUG START: {label}.Length={sprites.Length}, first={DescribeSprite(sprites.Length > 0 ? sprites[0] : null)}, last={DescribeSprite(sprites.Length > 0 ? sprites[sprites.Length - 1] : null)}");

        for (int index = 0; index < sprites.Length; index++)
        {
            if (sprites[index] == null)
            {
                Debug.LogError($"SPRITE DEBUG START: {label}[{index}] is NULL.");
            }
        }
    }

    private void LogRuntimeVisibility(string label)
    {
        Camera mainCamera = Camera.main;
        Vector3 viewportPosition = mainCamera != null
            ? mainCamera.WorldToViewportPoint(transform.position)
            : new Vector3(float.NaN, float.NaN, float.NaN);
        bool insideCamera = mainCamera != null && viewportPosition.z > 0f && viewportPosition.x >= 0f && viewportPosition.x <= 1f && viewportPosition.y >= 0f && viewportPosition.y <= 1f;
        string boundsInfo = boxCollider != null ? boxCollider.bounds.ToString() : "no BoxCollider2D";

        string rigidbodyInfo = rb != null
            ? $"bodyType={rb.bodyType}, simulated={rb.simulated}, constraints={rb.constraints}, mass={rb.mass}"
            : "no Rigidbody2D";

        Debug.Log($"VISIBILITY DEBUG [{label}]: timeScale={Time.timeScale}, gameIsOver={gameIsOver}, enabled={enabled}, pos={transform.position}, vel={(rb != null ? rb.linearVelocity : Vector2.zero)}, {rigidbodyInfo}, cameraViewport={viewportPosition}, insideCamera={insideCamera}, fallThreshold={fallThreshold}, rendererEnabled={(spriteRenderer != null && spriteRenderer.enabled)}, currentSprite={DescribeSprite(spriteRenderer != null ? spriteRenderer.sprite : null)}, bounds={boundsInfo}");
    }

    private string DescribeSprite(Sprite sprite)
    {
        if (sprite == null) return "NULL";
        return $"{sprite.name} size={sprite.rect.width}x{sprite.rect.height} ppu={sprite.pixelsPerUnit}";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Cloud"))
        {
            if (jumpCooldown <= 0f && IsLandingOnTop(collision))
                isGrounded = true;
        }

        // Handle Arrow collision
        if (collision.gameObject.CompareTag("Arrow") && !processingArrowCollision)
        {
            int enemyID = collision.gameObject.GetInstanceID();
            if (processedEnemies.Contains(enemyID)) return;
            
            processingArrowCollision = true;
            processedEnemies.Add(enemyID);
            
            // Get mouse and arrow positions and bounds
            Bounds mouseBounds = boxCollider.bounds;
            Bounds arrowBounds = collision.collider.bounds;
            
            // Determine if mouse is above arrow - use more precise detection
            float verticalDistance = mouseBounds.min.y - arrowBounds.max.y;
            bool mouseAboveArrow = verticalDistance > -arrowJumpDetectionPrecision;
            
            // Ensure there's horizontal overlap - just check if mouse is above arrow at all
            bool mouseIsAboveArrow = transform.position.y > collision.transform.position.y + 0.1f;
            
            Debug.Log($"Arrow collision check: verticalDistance={verticalDistance}, " +
                     $"mouseAboveArrow={mouseAboveArrow}, mouseIsAboveArrow={mouseIsAboveArrow}");
            
            // Only count as jumping on if position is above
            if (mouseIsAboveArrow)
            {
                // Jumped on top of arrow – make it fall instead of destroying it
                Debug.Log("Mouse jumped on arrow - making it fall");
                
                // Make arrow fall
                MakeArrowFall(collision.gameObject);
                
                // Force application with a slight delay
                StartCoroutine(ApplyBounceAfterDelay(0.02f, bounceMultiplier));
            }
            else
            {
                // Hit from side – Game Over
                Debug.Log("Game Over - Hit by arrow from side"); 
                GameOver();
            }
            
            // Reset processing flag after a short delay
            Invoke("ResetArrowCollisionFlag", 0.2f);
        }

        // Handle Spider collision
        if (collision.gameObject.CompareTag("Spider"))
        {
            int enemyID = collision.gameObject.GetInstanceID();
            if (processedEnemies.Contains(enemyID)) return;
            processedEnemies.Add(enemyID);
            
            // Check if mouse is above the spider AND moving downward (falling onto it)
            bool isJumpingOnFromAbove = IsJumpingOnTopOf(collision.transform);
            HandleSpiderCollision(collision.gameObject, isJumpingOnFromAbove);
        }

        // Handle Ball collision - any contact with ball kills mouse
        if (collision.gameObject.CompareTag("Ball"))
        {
            Debug.Log("Mouse hit the ball. Game Over!");
            Destroy(gameObject);
            // TODO: Add your game over logic here
        }

        // Handle Demon collision - completely rewritten section
        if (collision.gameObject.name.Contains("Demrun") && !processingDemonCollision)
        {
            int enemyID = collision.gameObject.GetInstanceID();
            if (processedEnemies.Contains(enemyID)) return;
            processedEnemies.Add(enemyID);
            
            processingDemonCollision = true;
            
            // Get mouse and demon positions
            float mouseY = transform.position.y;
            float demonY = collision.transform.position.y;
            
            // Get mouse and demon collider bounds
            Bounds mouseBounds = boxCollider.bounds;
            Bounds demonBounds = collision.collider.bounds;
            
            // Check if mouse feet are above demon head
            bool mouseAboveDemon = mouseBounds.min.y > demonBounds.max.y - verticalJumpDetectionThreshold;
            
            Debug.Log($"Demon collision check: mouseY={mouseY}, demonY={demonY}, " +
                      $"mouseBounds.min.y={mouseBounds.min.y}, demonBounds.max.y={demonBounds.max.y}, " +
                      $"mouseAboveDemon={mouseAboveDemon}");
            
            // If mouse Y is significantly higher than demon Y, consider it jumping on top
            if (mouseY > demonY + verticalJumpDetectionThreshold || mouseAboveDemon)
            {
                // Jumped on top of demon – kill demon
                Debug.Log("Mouse jumped on demon - killing demon");
                Destroy(collision.gameObject);
                
                // Force application with a slight delay to ensure collision is processed
                StartCoroutine(ApplyBounceAfterDelay(0.02f, demonBounceMultiplier));
            }
            else
            {
                // Hit from side – Game Over
                Debug.Log("Game Over - Hit demon from side"); 
                GameOver();
            }
            
            // Reset processing flag after a short delay
            Invoke("ResetDemonCollisionFlag", 0.2f);
        }
    }
    
    // New method to make an arrow fall down
    private void MakeArrowFall(GameObject arrow)
    {
        if (arrow == null) return;
        
        // Disable any existing movement script
        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        if (arrowController != null)
        {
            arrowController.enabled = false;
        }
        
        // Make sure it has a rigidbody
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        if (arrowRb == null)
        {
            arrowRb = arrow.AddComponent<Rigidbody2D>();
        }
        
        // Set rigidbody to dynamic so gravity affects it
        arrowRb.bodyType = RigidbodyType2D.Dynamic;
        arrowRb.gravityScale = 2.0f; // Increase gravity for faster fall
        
        // Add downward force
        arrowRb.linearVelocity = new Vector2(0, -arrowFallSpeed);
        
        // Rotate the arrow to point downward
        arrow.transform.rotation = Quaternion.Euler(0, 0, 270); // Point downward
        
        // Add a self-destruct component
        StartCoroutine(DestroyAfterDelay(arrow, 2.0f));
    }
    
    // Helper method to destroy after delay
    private System.Collections.IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
    
    private System.Collections.IEnumerator ApplyBounceAfterDelay(float delay, float multiplier)
    {
        yield return new WaitForSeconds(delay);
        if (this != null && gameObject != null) // Extra check to ensure mouse still exists
        {
            // Make sure we apply bounce force correctly
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset vertical velocity
            rb.AddForce(Vector2.up * jumpForce * multiplier, ForceMode2D.Impulse);
        }
    }
    
    private void ResetDemonCollisionFlag()
    {
        processingDemonCollision = false;
    }
    
    private void ResetArrowCollisionFlag()
    {
        processingArrowCollision = false;
    }

    // Improved method to check if mouse is jumping on top of something
    private bool IsJumpingOnTopOf(Transform other)
    {
        // 1. Get the colliders
        Bounds mouseBounds = boxCollider != null ? boxCollider.bounds : new Bounds(transform.position, Vector3.one);
        Collider2D otherCollider = other.GetComponent<Collider2D>();
        Bounds otherBounds = otherCollider != null ? otherCollider.bounds : new Bounds(other.position, Vector3.one);
        
        // 2. Check vertical position (mouse needs to be significantly above the object)
        float mouseBottom = mouseBounds.min.y;
        float otherTop = otherBounds.max.y;
        float verticalDistance = mouseBottom - otherTop;
        bool isAbove = verticalDistance > -verticalJumpDetectionThreshold; 
        
        // 3. Check horizontal overlap to ensure we're actually above the enemy
        bool isOverlappingHorizontally = 
            mouseBounds.min.x < otherBounds.max.x && 
            mouseBounds.max.x > otherBounds.min.x;
        
        // 4. Calculate the center positions to determine if mouse is truly above enemy
        bool mouseCenterIsAboveEnemyCenter = transform.position.y > other.position.y;
        
        Debug.Log($"IsJumpingOnTopOf check: verticalDistance={verticalDistance}, isAbove={isAbove}, " +
                  $"isOverlapping={isOverlappingHorizontally}, mouseCenterAboveEnemyCenter={mouseCenterIsAboveEnemyCenter}");
        
        // Consider it a jumping-on collision if most conditions are true
        return isAbove && isOverlappingHorizontally;
    }

    // Handle interaction with spiders
    private void HandleSpiderCollision(GameObject spider, bool isJumpingOn)
    {
        if (isJumpingOn)
        {
            // Jumped on top of spider - call its stepped on method if available
            SpiderMover spiderScript = spider.GetComponent<SpiderMover>();
            if (spiderScript != null)
            {
                spiderScript.OnSteppedOn();
            }
            else
            {
                // If no script or OnSteppedOn method, just destroy it
                Destroy(spider);
            }
            
            // Bounce up with even stronger jump force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset vertical velocity
            rb.AddForce(Vector2.up * jumpForce * spiderBounceMultiplier, ForceMode2D.Impulse);
            
            Debug.Log("Mouse jumped on spider and defeated it");
        }
        else
        {
            // Hit from side or bottom - game over
            Debug.Log("Mouse attacked by spider. Game Over!");
            GameOver();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (jumpCooldown > 0f) return; // don't re-ground during jump cooldown

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Cloud"))
        {
            if (IsLandingOnTop(collision) && rb.linearVelocity.y <= 0.1f)
            {
                isGrounded = true;
            }
            else if (!IsLandingOnTop(collision))
            {
                // Hitting the side of a platform — not grounded
                isGrounded = false;
            }
        }
    }

    // Check if the collision contact normal indicates we are on TOP of the platform
    private bool IsLandingOnTop(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Normal pointing up means the surface is below us (we're on top)
            if (contact.normal.y > 0.5f)
                return true;
        }
        return false;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Cloud"))
        {
            isGrounded = false;
        }
    }
    
    // This handles trigger colliders if enemies use trigger collider instead of standard collider
    private void OnTriggerEnter2D(Collider2D other)
    {

        // Handle Arrow trigger - completely new approach
        if (other.CompareTag("Arrow") && !processingArrowCollision)
        {
            int enemyID = other.gameObject.GetInstanceID();
            if (processedEnemies.Contains(enemyID))
            {
                Debug.Log("Already processed this arrow, ignoring trigger event");
                return;
            }
            
            processingArrowCollision = true;
            processedEnemies.Add(enemyID);
            
            // CRITICAL FIX: We're now using just a simple vertical position check
            // This is much more reliable than the bounds-based checks
            bool mouseIsAboveArrow = transform.position.y > other.transform.position.y + 0.5f;
            
            Debug.Log($"FIXED Arrow trigger check: mousePos.y={transform.position.y}, arrowPos.y={other.transform.position.y}, mouseIsAboveArrow={mouseIsAboveArrow}");
            
            if (mouseIsAboveArrow && rb.linearVelocity.y < 0) // Only if falling
            {
                // Store the arrow reference
                GameObject arrow = other.gameObject;
                
                Debug.Log("Mouse is above arrow and falling - making arrow fall");
                
                // First disable the arrow's collider to prevent further collisions with the mouse
                if (other.enabled)
                {
                    other.enabled = false;
                }
                
                // Make arrow fall down
                MakeArrowFall(arrow);
                
                // Apply bounce with a slight delay
                StartCoroutine(ApplyBounceDelayed(0.05f));
            }
            else
            {
                // Hit from side – Game Over
                Debug.Log("Game Over - Hit by arrow from side (trigger)"); 
                GameOver();
            }
            
            // Reset processing flag after a short delay
            Invoke("ResetArrowCollisionFlag", 0.2f);
        }

        // Handle Spider trigger
        if (other.CompareTag("Spider"))
        {
            int enemyID = other.gameObject.GetInstanceID();
            if (processedEnemies.Contains(enemyID)) return;
            processedEnemies.Add(enemyID);
            
            bool isJumpingOnFromAbove = IsJumpingOnTopOf(other.transform);
            HandleSpiderCollision(other.gameObject, isJumpingOnFromAbove);
        }
        
        // Handle Ball trigger - any contact with ball kills mouse
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Mouse hit the ball (trigger). Game Over!");
            GameOver();
        }
        
        // Handle demon trigger if needed
        if (other.name.Contains("Demrun") && !processingDemonCollision)
        {
            int enemyID = other.gameObject.GetInstanceID();
            if (processedEnemies.Contains(enemyID)) return;
            processedEnemies.Add(enemyID);
            
            processingDemonCollision = true;
            
            // Much simpler check for trigger colliders
            float mouseY = transform.position.y;
            float demonY = other.transform.position.y;
            
            if (mouseY > demonY + verticalJumpDetectionThreshold)
            {
                // Jumped on top of demon – kill demon
                Debug.Log("Mouse jumped on demon (trigger) - killing demon");
                Destroy(other.gameObject);
                
                // Force application with a slight delay to ensure collision is processed
                StartCoroutine(ApplyBounceAfterDelay(0.02f, demonBounceMultiplier));
            }
            else
            {
                // Hit from side – Game Over
                Debug.Log("Game Over - Hit demon from side (trigger)"); 
                GameOver();
            }
            
            // Reset processing flag after a short delay
            Invoke("ResetDemonCollisionFlag", 0.2f);
        }
    }
    
    // Simplified bounce coroutine - just for arrows
    private System.Collections.IEnumerator ApplyBounceDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (this != null && gameObject != null) // Check mouse still exists
        {
            Debug.Log("Applying delayed bounce after arrow destroyed");
            // Reset vertical velocity first
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            // Apply bounce force
            rb.AddForce(Vector2.up * jumpForce * bounceMultiplier, ForceMode2D.Impulse);
        }
    }
    
    void OnDestroy()
    {
        // Clean up any pending invokes when object is destroyed
        CancelInvoke();
    }
    
    // Method to restart the game - can be called from a UI button
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}