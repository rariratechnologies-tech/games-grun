using UnityEngine;
using System.Collections;

public class DemonEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Rigidbody2D rb;
    private bool isMoving = false;
    private Renderer myRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myRenderer = GetComponent<Renderer>();

        if (myRenderer.isVisible)
        {
            isMoving = true;
        }
    }

    void Update()
    {
        if (isMoving)
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y); // Use velocity instead of linearVelocity
        }
    }

    void OnBecameVisible()
    {
        isMoving = true;
    }

    void OnBecameInvisible()
    {
        isMoving = false;
    }

    public void SquashAndDestroy()
    {
        StartCoroutine(SquashRoutine());
    }

    private IEnumerator SquashRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }

    // 🎯 This is now correct
    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Mouse"))
    {
        Rigidbody2D mouseRb = other.GetComponent<Rigidbody2D>();

        if (mouseRb != null && mouseRb.linearVelocity.y < -1f) // Falling down
        {
            Debug.Log("Mouse bounced off arrow");
            Destroy(gameObject); // Destroy arrow

            // Apply bounce to mouse
            mouseRb.linearVelocity = new Vector2(mouseRb.linearVelocity.x, 10f); // Adjust bounce height
        }
        else
        {
            Debug.Log("Mouse hit by arrow. Mouse destroyed.");
            Destroy(other.gameObject); // Game over or similar logic
        }
    }
}


}
