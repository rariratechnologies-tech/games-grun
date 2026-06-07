using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameManager Instance;
    
    // Game state
    public bool gameIsOver = false;
    
    // References
    public GameObject gameOverPanel;
    public GameObject coinPanel;      // Reference to your coin/score display panel
    public GameObject pauseButton;    // Reference to your pause button
    
    [Header("Coin System")]
    public int coinCount = 0;
    public TextMeshProUGUI coinText;
    public Image coinIcon;
    
    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Find UI components if not already assigned
        if (gameOverPanel == null)
        {
            Transform canvasTransform = GameObject.Find("Canvas")?.transform;
            if (canvasTransform != null)
            {
                Transform gameOverTransform = canvasTransform.Find("Game Over");
                if (gameOverTransform != null)
                {
                    gameOverPanel = gameOverTransform.gameObject;
                    Debug.Log("GameManager: Found Game Over panel automatically");
                }
            }
        }
        
        // Try to find coin UI components if not assigned
        if (coinText == null)
        {
            coinText = GameObject.Find("CoinText")?.GetComponent<TextMeshProUGUI>();
            if (coinText != null)
            {
                Debug.Log("GameManager: Found Coin Text automatically");
            }
        }
        
        // Try to find CoinPanel if not assigned
        if (coinPanel == null)
        {
            coinPanel = GameObject.Find("CoinPanel");
            if (coinPanel != null)
            {
                Debug.Log("GameManager: Found CoinPanel automatically");
            }
        }
        
        // Try to find PauseButton if not assigned
        if (pauseButton == null)
        {
            pauseButton = GameObject.Find("PauseButton");
            if (pauseButton != null)
            {
                Debug.Log("GameManager: Found PauseButton automatically");
            }
        }
    }
    
    void Start()
    {
        // Make sure game starts in the correct state
        ResetGame();
        
        Debug.Log($"GameManager started. Game Over panel reference: {(gameOverPanel != null ? "Found" : "Missing")}");
        
        // Try to find the Game Over panel again if we haven't found it yet
        if (gameOverPanel == null)
        {
            // More thorough search - find all Canvas objects in the scene
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                // Look for child named "Game Over"
                Transform gameOverTransform = FindChildRecursively(canvas.transform, "Game Over");
                if (gameOverTransform != null)
                {
                    gameOverPanel = gameOverTransform.gameObject;
                    Debug.Log("GameManager: Found Game Over panel with recursive search");
                    break;
                }
            }
        }
        
        // Initialize coin display
        UpdateCoinDisplay();
    }
    
    // Helper method to recursively search for a child by name
    private Transform FindChildRecursively(Transform parent, string childName)
    {
        // First check direct children
        Transform directChild = parent.Find(childName);
        if (directChild != null)
            return directChild;
        
        // Then check all children recursively
        foreach (Transform child in parent)
        {
            Transform found = FindChildRecursively(child, childName);
            if (found != null)
                return found;
        }
        
        return null;
    }
    
    // Method to add coins
    public void AddCoins(int amount)
    {
        coinCount += amount;
        UpdateCoinDisplay();
        
        Debug.Log($"Collected coin! Total: {coinCount}");
    }
    
    // Method to update coin UI
    private void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = coinCount.ToString();
        }
        else
        {
            Debug.LogWarning("Coin Text UI element not assigned in GameManager");
        }
    }
    
    public void GameOver()
    {
        gameIsOver = true;
        
        // Hide gameplay UI elements when game is over
        if (coinPanel != null)
        {
            coinPanel.SetActive(false);
            Debug.Log("GameManager: Hid CoinPanel for Game Over screen");
        }
        
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
            Debug.Log("GameManager: Hid PauseButton for Game Over screen");
        }
        
        // Show game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over panel activated");
            
            // Ensure buttons in game over screen are centered
            HorizontalLayoutGroup layout = gameOverPanel.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.childAlignment = TextAnchor.MiddleCenter;
                Debug.Log("GameManager: Centered Game Over buttons");
            }
        }
        else
        {
            Debug.LogWarning("Game Over panel not assigned in GameManager");
            
            // Try to find it if not assigned
            GameObject panel = GameObject.Find("Game Over");
            if (panel != null)
            {
                gameOverPanel = panel;
                panel.SetActive(true);
                Debug.Log("Found and activated Game Over panel");
            }
            else
            {
                // Final attempt - look inside Canvas objects
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    Transform gameOverTransform = FindChildRecursively(canvas.transform, "Game Over");
                    if (gameOverTransform != null)
                    {
                        gameOverPanel = gameOverTransform.gameObject;
                        gameOverPanel.SetActive(true);
                        Debug.Log("GameManager: Found and activated Game Over panel with recursive search");
                        break;
                    }
                }
                
                if (gameOverPanel == null)
                {
                    Debug.LogError("Could not find any GameObject named 'Game Over' - make sure it exists in your scene!");
                }
            }
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("GameManager: Restarting game...");
        ResetGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("GameManager: Going to main menu...");
        SceneManager.LoadScene("MainMenu");
    }
    
    private void ResetGame()
    {
        gameIsOver = false;
        
        // Reset coin count
        coinCount = 0;
        UpdateCoinDisplay();
        
        // Show gameplay UI elements
        if (coinPanel != null)
        {
            coinPanel.SetActive(true);
        }
        
        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }
        
        // Hide game over UI if it exists
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}