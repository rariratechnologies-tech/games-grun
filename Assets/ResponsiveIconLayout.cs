using UnityEngine;
using UnityEngine.UI;

public class ResponsiveIconLayout : MonoBehaviour
{
    void Start()
    {
        // Make sure your panel has a Grid Layout Group component
        GridLayoutGroup gridLayout = GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gameObject.AddComponent<GridLayoutGroup>();
        
        // Set responsive cell sizing
        float screenHeight = Screen.height;
        float iconSize = screenHeight * 0.15f; // 15% of screen height
        
        gridLayout.cellSize = new Vector2(iconSize, iconSize);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 1; // Single column
    }
}