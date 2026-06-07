using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [System.Serializable]
    public class LevelInfo
    {
        public Button levelButton;
        public string sceneName;
        public bool isLocked = false;
        public GameObject comingSoonOverlay;
    }
    
    public LevelInfo[] levels = new LevelInfo[2]; // Initialize with default size of 2
    public string mainMenuSceneName = "MainMenu";
    
    void Start()
    {
        // Initialize the array if it's null
        if (levels == null)
        {
            Debug.LogWarning("Levels array was null. Creating a new array.");
            levels = new LevelInfo[2];
        }
        
        // Initialize each LevelInfo if it's null
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] == null)
            {
                Debug.LogWarning("Level " + i + " was null. Creating a new LevelInfo.");
                levels[i] = new LevelInfo();
            }
        }
        
        InitializeLevelButtons();
    }
    
    void InitializeLevelButtons()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            // Skip if the button is null
            if (levels[i].levelButton == null)
            {
                Debug.LogWarning("Level " + i + " button is null. Skipping setup.");
                continue;
            }
            
            int levelIndex = i;
            
            if (levels[i].isLocked)
            {
                levels[i].levelButton.interactable = false;
                
                if (levels[i].comingSoonOverlay != null)
                {
                    levels[i].comingSoonOverlay.SetActive(true);
                }
            }
            else
            {
                levels[i].levelButton.interactable = true;
                levels[i].levelButton.onClick.AddListener(() => LoadLevel(levelIndex));
                
                if (levels[i].comingSoonOverlay != null)
                {
                    levels[i].comingSoonOverlay.SetActive(false);
                }
            }
        }
    }
    
    void LoadLevel(int levelIndex)
    {
        if (levelIndex < levels.Length && levels[levelIndex] != null && !levels[levelIndex].isLocked)
        {
            SceneManager.LoadScene(levels[levelIndex].sceneName);
        }
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}