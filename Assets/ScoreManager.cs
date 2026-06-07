using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;      // Use Text instead if not using TextMeshPro
    public GameObject coinIconPrefab;     // Optional: for animated coin icon
    
    [Header("Score Settings")]
    public int coinCount = 0;
    public bool saveHighScore = true;
    
    private const string HIGHSCORE_KEY = "HighScore";
    
    void Start()
    {
        // Initialize display
        UpdateCoinDisplay();
        
        // Load high score if needed
        if (saveHighScore)
        {
            LoadHighScore();
        }
    }
    
    public void AddCoins(int amount)
    {
        coinCount += amount;
        UpdateCoinDisplay();
        
        // Optional: Trigger coin collection animation/effect
        if (coinIconPrefab != null)
        {
            AnimateCoinCollection();
        }
        
        // Save high score if needed
        if (saveHighScore && coinCount > PlayerPrefs.GetInt(HIGHSCORE_KEY, 0))
        {
            PlayerPrefs.SetInt(HIGHSCORE_KEY, coinCount);
            PlayerPrefs.Save();
        }
    }
    
    private void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = coinCount.ToString();
        }
    }
    
    private void LoadHighScore()
    {
        // Implementation depends on your game's high score system
        int highScore = PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
        Debug.Log("High Score: " + highScore);
    }
    
    private void AnimateCoinCollection()
    {
        // Simple animation effect for coin collection
        // Implementation depends on your game's visual style
        Transform coinDisplay = coinText?.transform.parent;
        if (coinDisplay != null)
        {
            // Simple scale animation
            StartCoroutine(PulseAnimation(coinDisplay));
        }
    }
    
    private System.Collections.IEnumerator PulseAnimation(Transform target)
    {
        Vector3 originalScale = target.localScale;
        float duration = 0.2f;
        
        // Scale up
        float timer = 0;
        while (timer < duration/2)
        {
            timer += Time.deltaTime;
            float t = timer / (duration/2);
            target.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t);
            yield return null;
        }
        
        // Scale down
        timer = 0;
        while (timer < duration/2)
        {
            timer += Time.deltaTime;
            float t = timer / (duration/2);
            target.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, t);
            yield return null;
        }
        
        // Ensure we end at the original scale
        target.localScale = originalScale;
    }
}