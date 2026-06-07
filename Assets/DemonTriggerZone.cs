using UnityEngine;

public class DemonTriggerZone : MonoBehaviour
{
    private DemonEnemy parentDemon;

    void Start()
    {
        parentDemon = GetComponentInParent<DemonEnemy>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mouse"))
        {
            Rigidbody2D mouseRb = other.GetComponent<Rigidbody2D>();
            if (mouseRb != null && mouseRb.linearVelocity.y < 0f)
            {
                parentDemon.SquashAndDestroy();
            }
        }
    }
}
