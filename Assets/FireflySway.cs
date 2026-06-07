using UnityEngine;
using System.Linq;

public class FireflySway : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Frames per second for the sprite animation")]
    public float frameRate = 14f;

    [Tooltip("If true, animation plays forward then backward for smooth looping")]
    public bool pingPong = true;

    [Header("Floating Movement")]
    [Tooltip("Enable gentle floating movement")]
    public bool enableFloat = true;

    [Tooltip("How far the firefly floats up and down")]
    public float floatAmplitude = 0.15f;

    [Tooltip("Speed of the floating motion")]
    public float floatSpeed = 2f;

    [Header("Sprites (auto-loaded if left empty)")]
    [Tooltip("Drag all fire_0 to fire_20 sprites here, or leave empty to auto-load from Resources/fire")]
    public Sprite[] fireSprites;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int currentFrame;
    private bool playingForward = true;
    private Vector3 startPosition;
    private float floatOffset;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.localPosition;

        // Random offset so multiple fireflies don't sync up
        floatOffset = Random.Range(0f, Mathf.PI * 2f);

        // If no sprites assigned in Inspector, try loading from the current sprite's texture
        if (fireSprites == null || fireSprites.Length == 0)
        {
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Load all sprites that share the same texture as the current sprite
                Sprite currentSprite = spriteRenderer.sprite;
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                fireSprites = allSprites
                    .Where(s => s.texture == currentSprite.texture)
                    .OrderBy(s => s.name)
                    .ToArray();

                Debug.Log($"FireflySway: Auto-loaded {fireSprites.Length} sprites from texture '{currentSprite.texture.name}'");
            }
        }

        if (fireSprites == null || fireSprites.Length <= 1)
        {
            Debug.LogWarning("FireflySway: No sprites found. Assign them in the Inspector.");
            enabled = false;
            return;
        }

        // Start on frame 0
        currentFrame = 0;
        spriteRenderer.sprite = fireSprites[currentFrame];
    }

    void Update()
    {
        // Sprite animation
        if (fireSprites != null && fireSprites.Length > 1)
        {
            timer += Time.deltaTime;

            if (timer >= 1f / frameRate)
            {
                timer -= 1f / frameRate;

                if (pingPong)
                {
                    if (playingForward)
                    {
                        currentFrame++;
                        if (currentFrame >= fireSprites.Length - 1)
                        {
                            currentFrame = fireSprites.Length - 1;
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
                    currentFrame = (currentFrame + 1) % fireSprites.Length;
                }

                spriteRenderer.sprite = fireSprites[currentFrame];
            }
        }

        // Gentle floating movement
        if (enableFloat)
        {
            float newY = startPosition.y + Mathf.Sin((Time.time * floatSpeed) + floatOffset) * floatAmplitude;
            transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
}
