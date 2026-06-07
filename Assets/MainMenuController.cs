using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "SampleScene"; // The name of your game scene
    
    [Header("Instruction Screens")]
    public GameObject instructionScreenPanel;
    public Image instructionImage;
    public Sprite[] instructionSprites; // Assign your 3 instruction images in the inspector
    private int currentInstructionIndex = 0;
    
    [Header("Exit Dialog")]
    public GameObject exitDialogPanel; // Assign the ExitDialog GameObject in the inspector
    
    private void Start()
    {
        Debug.Log("MainMenuController Start method called");
        
        // Always load instruction sprites from Resources to avoid broken references
        {
            Sprite s1 = Resources.Load<Sprite>("n1");
            Sprite s2 = Resources.Load<Sprite>("n2");
            Sprite s3 = Resources.Load<Sprite>("n3");
            
            if (s1 != null && s2 != null && s3 != null)
            {
                instructionSprites = new Sprite[] { s1, s2, s3 };
                Debug.Log("MainMenuController: Loaded 3 instruction sprites from Resources");
            }
            else
            {
                Debug.LogWarning($"MainMenuController: Could not load instruction sprites from Resources. s1={s1}, s2={s2}, s3={s3}");
            }
        }
        
        // Validate references
        if (instructionScreenPanel == null)
        {
            Debug.LogError("instructionScreenPanel is not assigned! Please assign it in the inspector.");
            // Try to find it automatically if possible
            instructionScreenPanel = GameObject.Find("InstructionPanel");
            if (instructionScreenPanel != null)
                Debug.Log("Found InstructionPanel automatically.");
        }
        
        if (instructionImage == null)
        {
            Debug.LogError("instructionImage is not assigned! Please assign it in the inspector.");
            // Try to find it automatically if possible
            if (instructionScreenPanel != null)
            {
                instructionImage = instructionScreenPanel.GetComponentInChildren<Image>();
                if (instructionImage != null)
                    Debug.Log("Found Image automatically.");
            }
        }
        
        if (instructionSprites == null || instructionSprites.Length == 0)
        {
            Debug.LogError("No instruction sprites assigned! Please assign them in the inspector.");
        }
        
        if (exitDialogPanel == null)
        {
            Debug.LogError("exitDialogPanel is not assigned! Please assign it in the inspector.");
            // Try to find it automatically if possible
            exitDialogPanel = GameObject.Find("ExitDialog");
            if (exitDialogPanel != null)
                Debug.Log("Found ExitDialog automatically.");
        }
        
        // Hide instruction panel and exit dialog initially
        if (instructionScreenPanel != null)
            instructionScreenPanel.SetActive(false);
            
        if (exitDialogPanel != null)
            exitDialogPanel.SetActive(false);
    }
    
    // This method will be called by the Play button
    public void PlayGame()
    {
        Debug.Log("PlayGame method called");
        
        if (instructionScreenPanel != null && instructionImage != null && 
            instructionSprites != null && instructionSprites.Length > 0)
        {
            // Show instructions
            ShowInstructions();
        }
        else
        {
            // Skip instructions and load the game directly if references are missing
            Debug.LogWarning("Missing references for instructions. Loading game scene directly.");
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OpenStageSelect()
{
    SceneManager.LoadScene("stage");
}
    
    
    // This method will be called by the Exit button
    public void ShowExitDialog()
    {
        Debug.Log("ShowExitDialog method called");
        
        if (exitDialogPanel != null)
        {
            exitDialogPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Exit dialog panel reference is missing!");
        }
    }
    
    // This method will be called by the "Yes" button in the exit dialog
    public void QuitApplication()
    {
        Debug.Log("QuitApplication method called");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // This method will be called by the "No" button in the exit dialog
    public void CloseExitDialog()
    {
        Debug.Log("CloseExitDialog method called");
        
        if (exitDialogPanel != null)
        {
            exitDialogPanel.SetActive(false);
        }
    }
    
    public void ShowInstructions()
    {
        Debug.Log("ShowInstructions method called");
        
        // Reset instruction index
        currentInstructionIndex = 0;
        
        // Show instruction panel
        instructionScreenPanel.SetActive(true);
        
        // Display first instruction
        if (instructionImage != null && instructionSprites.Length > 0)
            instructionImage.sprite = instructionSprites[currentInstructionIndex];
    }
    
    public void NextInstruction()
    {
        Debug.Log("NextInstruction method called");

        if (instructionImage != null && instructionSprites.Length > currentInstructionIndex)
        {
            instructionImage.sprite = instructionSprites[currentInstructionIndex];
            Debug.Log($"Set sprite to {instructionSprites[currentInstructionIndex].name}");
        }
        else
        {
            Debug.LogError($"Cannot set sprite. Image: {instructionImage}, Sprites count: {instructionSprites?.Length}, Index: {currentInstructionIndex}");
        }
        
        // Move to next instruction
        currentInstructionIndex++;
        Debug.Log($"Moving to instruction index {currentInstructionIndex}");
        
        // Check if we've shown all instructions
        if (currentInstructionIndex >= instructionSprites.Length)
        {
            Debug.Log("All instructions shown, loading game scene");
            
            // Hide instruction panel and start game
            if (instructionScreenPanel != null)
                instructionScreenPanel.SetActive(false);
                
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            // Show next instruction
            if (instructionImage != null && instructionSprites.Length > currentInstructionIndex)
                instructionImage.sprite = instructionSprites[currentInstructionIndex];
        }
    }
    
    // Add this method to the GameManager to return to main menu
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}