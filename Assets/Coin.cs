using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public float rotationSpeed = 100f;
    public AudioClip collectSound;
    
    // Reference to a global audio manager that won't be destroyed
    private static GameObject audioPlayer;
    
    // This static HashSet tracks which coins have been collected by their instance IDs
    private static System.Collections.Generic.HashSet<int> collectedCoins = 
        new System.Collections.Generic.HashSet<int>();
    
    void Start()
    {
        // Set proper tag
        gameObject.tag = "Coin";
        
        // Add a collider if none exists
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
        }
        
        // Check existing colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length > 1)
        {
            Debug.LogWarning("Coin has multiple colliders (" + colliders.Length + 
                            "). This may cause multiple trigger events!");
        }
        
        // Create persistent audio player if it doesn't exist
        if (audioPlayer == null)
        {
            audioPlayer = new GameObject("CoinAudioPlayer");
            // Make it persistent between scene loads
            GameObject.DontDestroyOnLoad(audioPlayer);
            // Add audio source
            AudioSource audioSource = audioPlayer.AddComponent<AudioSource>();
            // Configure audio source
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.volume = 1.0f;
        }
    }
    
    void Update()
    {
        // Simple rotation animation
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Get this coin's unique ID
        int coinID = gameObject.GetInstanceID();
        
        // Check if already collected using static tracking
        if (collectedCoins.Contains(coinID))
        {
            Debug.Log("Coin already in collected set, ignoring trigger");
            return;
        }
        
        // Only react to the mouse
        if (other.CompareTag("Mouse"))
        {
            // Add to collected set immediately
            collectedCoins.Add(coinID);
            
            // Disable all colliders immediately
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }
            
            Debug.Log("Coin " + coinID + " collected by: " + other.gameObject.name);
            Collect();
        }
    }
    
    public void Collect()
    {
        // Add to player's score via GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(value);
            Debug.Log("Collected coin! Total: " + value);
        }
        
        // Play sound through the persistent audio player
        if (collectSound != null && audioPlayer != null)
        {
            Debug.Log("Playing coin sound: " + collectSound.name);
            AudioSource source = audioPlayer.GetComponent<AudioSource>();
            if (source != null)
            {
                source.clip = collectSound;
                source.Play();
            }
        }
        else
        {
            Debug.LogWarning("No coin collect sound assigned!");
        }
        
        // Hide the coin
        GetComponent<Renderer>().enabled = false;
        
        // Destroy the coin after a short delay to ensure everything is processed
        Destroy(gameObject, 0.1f);
    }
}