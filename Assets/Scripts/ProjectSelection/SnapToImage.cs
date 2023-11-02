using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SnapToImage : MonoBehaviour
{
    // Image will scale from maxScale to minScale if it's near the center of the screen
    [SerializeField]
    private float maxDistance; // Maximum distance an image can be from the center before it is not scaled
    [SerializeField]
    private float minScale;
    [SerializeField]
    private float maxScale;
    [SerializeField]
    private float leanAmount; // Amount of lean for unfocused images
    [SerializeField]
    private float leanSpeed; // Speed of the leaning animation


    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private float lerpTime; // Time it takes to lerp (linear interpolation) to image

    private List<RectTransform> images;
    private Vector2 targetPosition;
    private RectTransform focusedImage;

    private void Start()
    {
        CMSProjectImageLoad.OnImagesDisplayed += RunSnapToImage;
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

    private void OnDestroy()
    {
        CMSProjectImageLoad.OnImagesDisplayed -= RunSnapToImage;
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
            float distance = Mathf.Abs(scrollRect.content.anchoredPosition.x + image.anchoredPosition.x);
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

            // Calculate lean angle based on position
            float leanFactor = distance / maxDistance; // Normalized distance from center
            float leanAngle = -(leanAmount * leanFactor * Mathf.Sign(scrollRect.content.anchoredPosition.x + image.anchoredPosition.x));
            float currentLeanAngle = image.localEulerAngles.z > 180 ? image.localEulerAngles.z - 360 : image.localEulerAngles.z; // Convert angle to -180 to 180 range
            float newLeanAngle = Mathf.Lerp(currentLeanAngle, leanAngle, Time.deltaTime * leanSpeed); // Interpolate between current and target angle
            image.localRotation = Quaternion.Euler(0, 0, newLeanAngle);

            // Fade in text based on distance from center
            float alpha = Mathf.Lerp(1f, 0f, distance / maxDistance); // Adjust these values as needed

            // Get the TextMeshProUGUI components and set their alpha
            TextMeshProUGUI[] texts = image.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
        }

        // Lerp to target position
        float newX = Mathf.Lerp(scrollRect.content.anchoredPosition.x, targetPosition.x, Time.deltaTime / lerpTime);
        Vector2 newPosition = new Vector2(newX, scrollRect.content.anchoredPosition.y);
        scrollRect.content.anchoredPosition = newPosition;
    }
}
