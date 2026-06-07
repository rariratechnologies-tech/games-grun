using UnityEngine;
using UnityEngine.UI;

public class IconScaler : MonoBehaviour
{
    void Start()
    {
        // Get all icon images
        Image[] icons = GetComponentsInChildren<Image>();
        
        // Calculate proper size based on screen resolution
        float referenceHeight = 1920f; // Your reference resolution height
        float currentHeight = Screen.height;
        float scaleFactor = currentHeight / referenceHeight;
        
        foreach (Image icon in icons)
        {
            // Skip the panel background itself
            if (icon.gameObject == gameObject) continue;
            
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            float baseSizePercent = 0.1f; // 10% of panel height
            float adaptiveSize = Screen.height * baseSizePercent;
            
            iconRect.sizeDelta = new Vector2(adaptiveSize, adaptiveSize);
        }
    }
}