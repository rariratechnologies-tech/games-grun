using UnityEngine;

public class FlagTrigger : MonoBehaviour
{
    // Reference to Game Manager
    private GameManager gameManager;
    
    // State tracking - changing from static to instance variable
    private bool flagTriggered = false;
    
    void Start()
    {
        // Get reference to GameManager
        gameManager = FindObjectOfType<GameManager>();
        
        // Reset the game state for this instance
        flagTriggered = false;
        
        // Add a trigger collider if needed
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector2(2f, 4f);
            Debug.Log("Added trigger collider to flag");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger once for this instance
        if (flagTriggered || MoveRight.gameIsOver)
            return;
            
        // Check if it's the mouse
        if (other.CompareTag("Mouse"))
        {
            // Set flag immediately to prevent multiple triggers
            flagTriggered = true;
            MoveRight.gameIsOver = true; // Stop the mouse immediately
            
            Debug.Log("Flag reached - game over");
            
            // Find the mouse/camel and stop it immediately
            MoveRight mouseController = FindObjectOfType<MoveRight>();
            if (mouseController != null)
            {
                // Stop the mouse from moving
                Rigidbody2D mouseRb = mouseController.GetComponent<Rigidbody2D>();
                if (mouseRb != null)
                {
                    mouseRb.linearVelocity = Vector2.zero;
                    mouseRb.isKinematic = true;
                }
                
                // Disable the MoveRight script
                mouseController.enabled = false;
            }
            
            // Create a separate game over handler BEFORE destroying this object
            GameObject gameOverHandler = new GameObject("FlagGameOverHandler");
            FlagGameOverHandler handler = gameOverHandler.AddComponent<FlagGameOverHandler>();
            handler.Initialize(gameManager);
            
            // Now it's safe to destroy the flag
            Destroy(gameObject);
        }
    }
    
    // You can still keep this method, but it's not needed anymore
    // since we're now using an instance variable
    public void ResetFlag()
    {
        flagTriggered = false;
    }
}

// Separate class to handle game over after flag is destroyed
public class FlagGameOverHandler : MonoBehaviour
{
    private GameManager gameManager;
    
    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        
        // Trigger game over with small delay
        Invoke("TriggerGameOver", 0.5f);
    }
    
    private void TriggerGameOver()
    {
        // Trigger game over through GameManager
        if (gameManager != null)
        {
            Debug.Log("Flag game over handler triggering game over");
            gameManager.GameOver();
        }
        else
        {
            // Fallback to MoveRight
            MoveRight player = FindObjectOfType<MoveRight>();
            if (player != null)
            {
                player.GameOver();
            }
        }
        
        // Destroy this handler after it's done its job
        Destroy(gameObject, 0.1f);
    }
    
    private void OnDestroy()
    {
        CancelInvoke();
    }
}