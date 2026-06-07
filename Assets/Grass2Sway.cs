using UnityEngine;

public class Grass2Sway : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Frames per second for the sprite animation")]
    public float frameRate = 12f;

    [Tooltip("If true, animation plays forward then backward for smooth looping")]
    public bool pingPong = true;

    [Header("Sprites (auto-loaded if left empty)")]
    [Tooltip("Drag all grasss2_0 to grasss2_20 sprites here, or leave empty to auto-load from Resources/grasss2")]
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
            grassSprites = Resources.LoadAll<Sprite>("grasss2");
        }

        if (grassSprites == null || grassSprites.Length <= 1)
        {
            Debug.LogWarning("Grass2Sway: No sprites found. Either assign them in the Inspector, " +
                "or place grasss2.png in Assets/Resources/");
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
