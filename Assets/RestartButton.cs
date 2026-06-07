using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleRestart : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(RestartNow);
            Debug.Log("Simple restart button setup - onClick listener added");
        }
        else
        {
            // If there's no button component, add one
            button = gameObject.AddComponent<Button>();
            button.onClick.AddListener(RestartNow);
            Debug.Log("Added Button component and onClick listener");
            
            // Make sure we have an image component for the button to work
            if (GetComponent<Image>() == null)
            {
                Image img = gameObject.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.01f); // Almost transparent
                Debug.Log("Added Image component for button to work");
            }
        }
        
        Debug.Log("SimpleRestart script initialized on " + gameObject.name);
    }
    
    public void RestartNow()
    {
        Debug.Log("RESTART BUTTON CLICKED - RESTARTING GAME!");
        
        // Try to tell GameManager we're restarting (if it exists)
        GameManager manager = FindObjectOfType<GameManager>();
        if (manager != null)
        {
            Debug.Log("Using GameManager to restart");
            
            // Try to access the RestartGame method safely
            try {
                manager.RestartGame();
                return; // If this worked, we're done
            }
            catch (System.Exception e) {
                Debug.LogError("Error using GameManager.RestartGame: " + e.Message);
                // Continue to fallback
            }
        }
        
        // Direct scene reload as fallback
        Debug.Log("Directly reloading current scene...");
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
    
    // Make this update run every frame to check for keyboard shortcuts
    void Update()
    {
        // Add keyboard shortcut (R key) for restarting
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed - restarting game");
            RestartNow();
        }
    }
}