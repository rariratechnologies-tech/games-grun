using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;
    
    // UI elements
    public GameObject pauseButton;
    public GameObject pauseMenuPanel;
    public GameObject exitDialog;
    
    // Reference to the BackgroundMusicManager
    private BackgroundMusicManager musicManager;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initially hide panels
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        if (exitDialog != null)
            exitDialog.SetActive(false);
            
        // Find the BackgroundMusicManager in the scene
        musicManager = FindObjectOfType<BackgroundMusicManager>();
    }
    
    public void PauseGame()
    {
        // Show pause menu and pause the game
        Time.timeScale = 0; // This freezes the game
        
        // Show pause menu, hide pause button
        pauseMenuPanel.SetActive(true);
        pauseButton.SetActive(false);
        
        // Hide exit dialog if it's open
        if (exitDialog != null)
            exitDialog.SetActive(false);
        
        // Pause the music
        if (musicManager != null)
        {
            musicManager.PauseMusic();
        }
    }
    
    public void ResumeGame()
    {
        // Hide pause menu and resume the game
        Time.timeScale = 1; // Normal time flow
        
        // Hide pause menu, show pause button
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
        
        // Hide exit dialog
        if (exitDialog != null)
            exitDialog.SetActive(false);
        
        // Resume the music
        if (musicManager != null)
        {
            musicManager.ResumeMusic();
        }
    }
    
    // Rest of your code remains the same
    public void RestartGame()
    {
        // Resume normal time flow
        Time.timeScale = 1;
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu()
    {
        // Resume normal time flow
        Time.timeScale = 1;
        
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
    
    public void ShowExitDialog()
    {
        // Show exit confirmation dialog
        if (exitDialog != null)
            exitDialog.SetActive(true);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }
    
    public void HideExitDialog()
    {
        // Hide exit dialog, show pause menu
        if (exitDialog != null)
            exitDialog.SetActive(false);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }
    
    public void ExitGame()
    {
        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Optional: Handle back button press on Android
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exitDialog != null && exitDialog.activeSelf)
            {
                HideExitDialog();
            }
            else if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
}