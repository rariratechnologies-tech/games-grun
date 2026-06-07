using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    // Audio source component
    private AudioSource audioSource;
    
    void Awake()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        
        // Make sure the AudioSource is playing when the game starts
        if (audioSource != null && !audioSource.isPlaying && audioSource.enabled)
        {
            audioSource.Play();
        }
    }
    
    // Pause the background music
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    
    // Resume the background music
    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    
    // Optional: Method to change the background music
    public void ChangeMusic(AudioClip newMusic)
    {
        if (audioSource != null)
        {
            bool wasPlaying = audioSource.isPlaying;
            audioSource.clip = newMusic;
            
            if (wasPlaying)
            {
                audioSource.Play();
            }
        }
    }
}