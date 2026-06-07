using UnityEngine;

public class PanelScaler : MonoBehaviour
{
    [SerializeField] private float widthPercentage = 0.2f; // 20% of screen width
    
    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        
        // Anchor to right side
        rectTransform.anchorMin = new Vector2(1 - widthPercentage, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        
        // Reset position
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}