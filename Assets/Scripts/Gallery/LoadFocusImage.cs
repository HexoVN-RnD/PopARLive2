using UnityEngine;
using UnityEngine.UI;
using static CMSFeedLoad;

public class LoadFocusImage : MonoBehaviour
{
    public delegate void LoadFocusImageHandler();
    public static event LoadFocusImageHandler OnFocusImageLoadCompleted;

    private void OnEnable()
    {
        CMSFeedLoad.OnLoadCompleted += LoadImages;
    }
    private void OnDisable()
    {
        CMSFeedLoad.OnLoadCompleted -= LoadImages;
    }

    [SerializeField]
    private GameObject parentObject;
    [SerializeField]
    private CMSFeedLoad cmsFeedLoad;

    void LoadImages()
    {
        foreach (var item in cmsFeedLoad.mediaTextures)
        {
            // Create a new GameObject for each image
            GameObject imageObject = new GameObject(item.Key.ToString());

            // Add the GameObject as a child of the parentObject
            imageObject.transform.SetParent(parentObject.transform);

            // Add an Image component to the GameObject
            Image imageComponent = imageObject.AddComponent<Image>();

            // Convert the Texture2D to a Sprite
            Texture2D texture = item.Value;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Set the sprite of the Image component to the sprite
            imageComponent.sprite = sprite;

            // preserve the aspect ratio of the image
            imageComponent.preserveAspect = true;

            // set the image width to be screen size, calculate height to preserve aspect ratio
            float aspectRatio = (float)texture.height / (float)texture.width;
            imageComponent.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.width * aspectRatio);
        }
        OnFocusImageLoadCompleted?.Invoke();
    }
}
