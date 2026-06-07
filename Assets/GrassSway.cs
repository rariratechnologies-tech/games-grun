using UnityEngine;

public class GrassSway : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Frames per second for the sprite animation")]
    public float frameRate = 12f;

    [Tooltip("If true, animation plays forward then backward for smooth looping")]
    public bool pingPong = true;

    [Header("Sprites (auto-loaded if left empty)")]
    [Tooltip("Drag all grass_0 to grass_20 sprites here, or leave empty to auto-load from Resources/grass")]
    public Sprite[] grassSprites;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int currentFrame;
    private bool playingForward = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // If no sprites assigned in Inspector, try loading from Resources
        if (grassSprites == null || grassSprites.Length == 0)
        {
            grassSprites = Resources.LoadAll<Sprite>("grass");
        }

        if (grassSprites == null || grassSprites.Length <= 1)
        {
            Debug.LogWarning("GrassSway: No sprites found. Either assign them in the Inspector, " +
                "or place grass.png in Assets/Resources/");
            enabled = false;
            return;
        }

        // Start on frame 0
        currentFrame = 0;
        spriteRenderer.sprite = grassSprites[currentFrame];
    }

    void Update()
    {
        if (grassSprites == null || grassSprites.Length <= 1) return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;

            if (pingPong)
            {
                if (playingForward)
                {
                    currentFrame++;
                    if (currentFrame >= grassSprites.Length - 1)
                    {
                        currentFrame = grassSprites.Length - 1;
                        playingForward = false;
                    }
                }
                else
                {
                    currentFrame--;
                    if (currentFrame <= 0)
                    {
                        currentFrame = 0;
                        playingForward = true;
                    }
                }
            }
            else
            {
                currentFrame = (currentFrame + 1) % grassSprites.Length;
            }

            spriteRenderer.sprite = grassSprites[currentFrame];
        }
    }
}
