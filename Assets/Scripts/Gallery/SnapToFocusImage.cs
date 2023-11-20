using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SnapToFocusImag : MonoBehaviour
{
    // Image will scale from maxScale to minScale if it's near the center of the screen
    [SerializeField]
    private float maxDistance; // Maximum distance an image can be from the center before it is not scaled
    [SerializeField]
    private float minScale;
    [SerializeField]
    private float maxScale;
    
    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private float lerpTime; // Time it takes to lerp (linear interpolation) to image

    private List<RectTransform> images;
    private Vector2 targetPosition;
    private RectTransform focusedImage;

    // add event handler
    private void OnEnable()
    {
        LoadFocusImage.OnFocusImageLoadCompleted += RunSnapToImage;
    }
    private void OnDisable()
    {
        LoadFocusImage.OnFocusImageLoadCompleted -= RunSnapToImage;
    }

    void RunSnapToImage()
    {
        images = new List<RectTransform>();

        // Get all images
        foreach (Transform child in scrollRect.content)
        {
            images.Add(child as RectTransform);
        }

        // Set initial target position to current position
        targetPosition = scrollRect.content.anchoredPosition;
    }

    void Update()
    {
        // Find nearest image
        float nearestPos = float.MaxValue;
        RectTransform newFocusedImage = null;
        // check if images is null
        if (images == null)
        {
            return;
        }
        foreach (RectTransform image in images)
        {
            float distance = Mathf.Abs(scrollRect.content.anchoredPosition.x + image.anchoredPosition.x)/2;
            if (distance < nearestPos)
            {
                nearestPos = distance;
                targetPosition = new Vector2(-image.anchoredPosition.x, scrollRect.content.anchoredPosition.y);
                newFocusedImage = image;
            }
        }

        // Update focused image
        focusedImage = newFocusedImage;

        foreach (RectTransform image in images)
        {
            // Scale images based on distance from center
            float distance = Mathf.Abs(scrollRect.content.anchoredPosition.x + image.anchoredPosition.x);
            float scale = Mathf.Lerp(maxScale, minScale, distance / maxDistance);
            image.localScale = Vector3.one * scale;
        }

        // Lerp to target position
        float newX = Mathf.Lerp(scrollRect.content.anchoredPosition.x, targetPosition.x, Time.deltaTime / lerpTime);
        Vector2 newPosition = new Vector2(newX, scrollRect.content.anchoredPosition.y);
        scrollRect.content.anchoredPosition = newPosition;
    }
}
