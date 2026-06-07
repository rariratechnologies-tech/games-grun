using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private string[] levelSceneNames;
    [SerializeField] private GameObject[] comingSoonOverlays;

    void Start()
    {
        // Initialize buttons
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i; // Create a local copy for the closure
            
            // If this is a "Coming Soon" level
            if (i >= levelSceneNames.Length || string.IsNullOrEmpty(levelSceneNames[i]))
            {
                // Disable button interactability
                levelButtons[i].interactable = false;
                
                // Show "Coming Soon" overlay if it exists
                if (comingSoonOverlays.Length > i && comingSoonOverlays[i] != null)
                {
                    comingSoonOverlays[i].SetActive(true);
                }
            }
            else
            {
                // Set up clickable level
                levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
            }
        }
    }

    void LoadLevel(int levelIndex)
    {
        if (levelIndex < levelSceneNames.Length && !string.IsNullOrEmpty(levelSceneNames[levelIndex]))
        {
            // Load the selected level scene
            SceneManager.LoadScene(levelSceneNames[levelIndex]);
        }
    }
}