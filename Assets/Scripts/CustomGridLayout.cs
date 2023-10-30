using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomGridLayout : MonoBehaviour
{
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 spacing = Vector2.zero;

    private RectTransform rectTransform;
    private List<RectTransform> cells = new List<RectTransform>();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void AddItem(RectTransform item, string gridType)
    {
        Vector2 size = cellSize;
        switch (gridType)
        {
            case "1*1":
                size = cellSize;
                break;
            case "1*2":
                size = new Vector2(cellSize.x, cellSize.y * 2);
                break;
            case "2*1":
                size = new Vector2(cellSize.x * 2, cellSize.y);
                break;
            case "2*2":
                size = new Vector2(cellSize.x * 2, cellSize.y * 2);
                break;
        }

        item.sizeDelta = size;

        // Find an empty spot in the grid for the item
        Vector2 position = FindEmptySpot(size);
        item.anchoredPosition = position;

        // Add the item to the list of cells
        cells.Add(item);
    }

    private Vector2 FindEmptySpot(Vector2 size)
    {
        // This is where you'll need to implement your logic for finding an empty spot in the grid.
        // For now, this just places items from left to right, top to bottom.
        int x = cells.Count % (int)(rectTransform.rect.width / cellSize.x);
        int y = cells.Count / (int)(rectTransform.rect.width / cellSize.x);
        return new Vector2(x * (cellSize.x + spacing.x), -y * (cellSize.y + spacing.y));
    }
}
