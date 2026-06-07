using UnityEngine;
using System.Collections;

public class BowmanShooterWithSpeed : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowSpeed = 5f; // Default to match your Inspector value
    public float animationSpeed = 0.5f;
    public float initialDelay = 1f;
    public float stayVisibleAfterFiringTime = 4f; // Default 4, but may be overridden in Inspector
    public Color bowmanColor = Color.white;
    public Sprite arrowSprite; // Reference to the arrow sprite
    
    // New direction setting (defaults to left)
    public Vector3 arrowDirection = Vector3.left;
    
    // Camera visibility check parameters
    public bool onlyAppearWhenVisible = true; // Enable/disable this feature
    public float checkVisibilityInterval = 0.5f; // How often to check visibility
    private float lastVisibilityCheck = 0f;
    private bool isInView = false;
    
    private Animator animator;
    private bool hasStartedSequence = false;
    private SpriteRenderer spriteRenderer;
    private GameObject instantiatedArrow;
    private bool hasCompletedFiring = false;

    void Start()
    {
        Debug.Log("Bowman Start() method called with arrow speed: " + arrowSpeed);
        
        // Get required components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Initially hide the bowman
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Validate firePoint
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned! Please assign it in the inspector.");
        }
        
        // Only start the sequence if we're not waiting for visibility
        if (!onlyAppearWhenVisible)
        {
            StartCoroutine(DelayedStart());
        }
    }
    
    void Update()
    {
        // If we're using visibility check, perform it periodically
        if (onlyAppearWhenVisible)
        {
            lastVisibilityCheck += Time.deltaTime;
            
            if (lastVisibilityCheck >= checkVisibilityInterval)
            {
                lastVisibilityCheck = 0f;
                CheckIfVisible();
            }
        }
    }
    
    void CheckIfVisible()
    {
        if (hasStartedSequence)
            return; // Already started, no need to check
            
        // Check if renderer is in camera view
        if (spriteRenderer != null)
        {
            bool wasInView = isInView;
            isInView = IsVisibleToCamera(spriteRenderer);
            
            // If we just became visible
            if (isInView && !wasInView)
            {
                Debug.Log("Bowman became visible to camera - starting sequence");
                StartCoroutine(DelayedStart());
            }
        }
    }
    
    bool IsVisibleToCamera(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
    
    IEnumerator DelayedStart()
    {
        yield return null;
        yield return null;
        StartCoroutine(DelayedAppearAndShoot());
    }

    IEnumerator DelayedAppearAndShoot()
    {
        if (hasStartedSequence)
            yield break;
            
        hasStartedSequence = true;
        
        // Wait for the initial delay
        yield return new WaitForSeconds(initialDelay);
        
        // Make bowman visible
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = bowmanColor;
            Debug.Log("Bowman sprite renderer enabled - bowman is now visible");
        }
        
        // Now start the shooting sequence
        StartCoroutine(ShootSequence());
    }
    
    IEnumerator ShootSequence()
    {
        // Play animation if possible
        if (animator != null && animator.enabled)
        {
            try
            {
                animator.enabled = true;
                animator.speed = animationSpeed;
                animator.Play("BowmanShoot", 0, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Animation error: " + e.Message);
            }
        }
        
        // Wait for the animation to play
        float waitTime = 0.334f; 
        yield return new WaitForSeconds(waitTime);
        
        // Fire the arrow - but keep bowman visible
        FireArrow();
        hasCompletedFiring = true;
        
        Debug.Log("Arrow fired. Staying visible for " + stayVisibleAfterFiringTime + " seconds");
        
        // IMPORTANT: This is where we wait for visibility time
        yield return new WaitForSeconds(stayVisibleAfterFiringTime);
        
        Debug.Log("Time to hide bowman after " + stayVisibleAfterFiringTime + " seconds");
        
        // Now make bowman invisible
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            Debug.Log("Bowman sprite renderer disabled");
        }
        else
        {
            Debug.LogError("SpriteRenderer is null when trying to hide bowman");
        }
        
        // Wait a moment before destroying the bowman object
        yield return new WaitForSeconds(0.1f);
        
        // Destroy bowman
        Debug.Log("Destroying bowman gameObject");
        Destroy(gameObject);
    }

    void FireArrow()
{
    Debug.Log("Firing arrow with speed: 3");
    
    // Debug checks
    Debug.Log("FirePoint reference check: " + (firePoint != null ? "OK" : "NULL"));
    
    if (firePoint != null)
    {
        // Get the player position
        GameObject player = GameObject.FindGameObjectWithTag("Mouse") ?? GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPosition = firePoint.position;
        
        // Place arrow to the right of the bowman, but to the left of the player
        if (player != null)
        {
            // Position arrow at a good distance from player (10 units left of player)
            spawnPosition.x = player.transform.position.x + 10f;
        }
        
        // Position arrow at a lower height - right around where the player runs
        spawnPosition.y = -2.8f; // Lower position to match player running height
        
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.position = spawnPosition;
        arrow.tag = "Arrow";
        
        // Add a SpriteRenderer with a GUARANTEED visible sprite
        SpriteRenderer arrowRenderer = arrow.AddComponent<SpriteRenderer>();
        
        // Create a larger texture for better visibility
        Texture2D texture = new Texture2D(24, 8);
        
        // Create an arrow-like shape with colors
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Arrowhead (first 8 pixels) - make it a bright red triangle
                if (x < 8)
                {
                    // Create a triangle shape for the arrowhead
                    if (y >= (4 - x/2) && y < (4 + x/2))
                    {
                        texture.SetPixel(x, y, Color.yellow); // Yellow arrowhead
                    }
                    else
                    {
                        texture.SetPixel(x, y, new Color(0, 0, 0, 0)); // Transparent
                    }
                }
                // Arrow shaft (remaining pixels)
                else
                {
                    if (y >= 3 && y <= 4)
                    {
                        texture.SetPixel(x, y, Color.red); // Red shaft
                    }
                    else
                    {
                        texture.SetPixel(x, y, new Color(0, 0, 0, 0)); // Transparent
                    }
                }
            }
        }
        
        texture.Apply();
        
        // Create a sprite from the texture
        Sprite arrowDefaultSprite = Sprite.Create(texture, new Rect(0, 0, 24, 8), new Vector2(0.85f, 0.5f), 100);
        arrowRenderer.sprite = arrowDefaultSprite;
        
        // Fix Z-position to ensure visibility in 2D
        Vector3 newPosition = arrow.transform.position;
        newPosition.z = 0f; // Ensure Z is at 0 for 2D visibility
        arrow.transform.position = newPosition;
        Debug.Log($"Adjusted arrow Z-position to: {arrow.transform.position}");
        
        // Make the arrow LARGER for better visibility
        arrow.transform.localScale = new Vector3(5.0f, 2.0f, 1f);
        Debug.Log($"Set arrow to larger scale: {arrow.transform.localScale}");
        
        // CRUCIAL: Make sure the arrow is active in hierarchy
        arrow.SetActive(true);
        
        // Add a fresh ArrowController
        ArrowController arrowController = arrow.AddComponent<ArrowController>();
        Debug.Log("Added new ArrowController to arrow");
        
        // Make sure renderer is enabled and clearly visible
        arrowRenderer.enabled = true;
        arrowRenderer.color = Color.white; // Use white to ensure the colors in our texture show properly
        arrowRenderer.sortingOrder = 100; // Extremely high sorting order to ensure visibility
        Debug.Log($"Arrow renderer enabled: {arrowRenderer.enabled}, with color: {arrowRenderer.color}, sorting order: {arrowRenderer.sortingOrder}");
        
        // Set the direction and speed for the arrow
        arrowController.SetDirection(arrowDirection);
        arrowController.SetSpeed(1.5f); 
        Debug.Log($"Set arrow speed to: 1.5 in direction: {arrowDirection} (slowed down for player reaction time)");
        
        // Make sure the arrow has a collider that matches the visible part better
        BoxCollider2D boxCollider = arrow.AddComponent<BoxCollider2D>();
        boxCollider.enabled = false;

        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(1.5f, 0.2f); // Much smaller collider
        boxCollider.offset = new Vector2(0f, 0f); // Center the collider on the arrow
        Debug.Log("Added BoxCollider2D to arrow with reduced height for better gameplay");
        
        // Save reference to instantiated arrow
        instantiatedArrow = arrow;
    }
    else
    {
        Debug.LogError("FirePoint is null! Cannot create arrow.");
    }
}
    
    // OnDestroy is called when the GameObject is destroyed
    void OnDestroy()
    {
        // This helps detect if something else is destroying the object prematurely
        if (hasStartedSequence && !hasCompletedFiring)
        {
            Debug.LogWarning("Bowman was destroyed before completing the firing sequence!");
        }
    }
    
    // Optional: Use OnBecameVisible/OnBecameInvisible events
    // These events work when the renderer is enabled!
    void OnBecameVisible()
    {
        if (onlyAppearWhenVisible && !hasStartedSequence && spriteRenderer != null)
        {
            // The renderer needs to be enabled for OnBecameVisible to work
            // So we only use this if it's already enabled
            if (spriteRenderer.enabled)
            {
                Debug.Log("Bowman OnBecameVisible called - starting sequence");
                StartCoroutine(DelayedStart());
            }
        }
    }
}