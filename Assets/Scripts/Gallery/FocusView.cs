using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CMSFeedImport;
using static CMSFeedLoad;

public class FocusView : MonoBehaviour
{
    [SerializeField]
    private GameObject focusViewCanvas;
    [SerializeField]
    private GameObject focusImagePrefab;
    [SerializeField]
    private GameObject focusVideoPrefab;
    [SerializeField]
    private CMSFeedLoad cmsFeedLoad;
    [SerializeField]
    private float verticalPadding;
    private Vector2 startTouchPosition, endTouchPosition;
    private int currentImageIndex = 0;
    private Data[] allData;

    private void Start()
    {
        // Load all data at the start
        allData = cmsFeedLoad.LoadData();
    }

    private void OnEnable()
    {
        OnGridImageClick.OnFocusViewClicked += ShowMedia;
    }

    private void OnDisable()
    {
        OnGridImageClick.OnFocusViewClicked -= ShowMedia;
    }

    private void ShowMedia(int imageID)
    {
        Data item = Array.Find(allData, i => i.id == imageID);

        if (item != null && item.media_type == "Image")
        {
            // Load the image
            string imagePath = Path.Combine(Application.persistentDataPath, "Feed", item.media.Split('=')[1].Split('?')[0]);
            Texture2D texture = LoadImage(imagePath);

            // Convert the Texture2D to a Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Get the Image component in the focusImagePrefab and set the sprite
            Image imageComponent = focusImagePrefab.GetComponentInChildren<Image>();
            imageComponent.sprite = sprite;
            imageComponent.preserveAspect = true;
            float aspectRatio = (float)texture.width / (float)texture.height;
            imageComponent.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
            //Set the width of the image to be the same as the device screen width minus the padding
            RectTransform imageRectTransform = imageComponent.GetComponent<RectTransform>();
            imageRectTransform.sizeDelta = new Vector2(Screen.width - verticalPadding, imageRectTransform.sizeDelta.y);

            // Get the TextMeshProUGUI components in the focusImagePrefab and set the text
            TextMeshProUGUI[] textComponents = focusImagePrefab.GetComponentsInChildren<TextMeshProUGUI>();
            textComponents[0].text = item.caption;
            //format created At date to Month/Year format
            DateTime createdAt = DateTime.Parse(item.createdAt);
            textComponents[1].text = createdAt.ToString("MMMM yyyy");
        }

        focusViewCanvas.SetActive(true);
    }

    private Texture2D LoadImage(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }

    public void CloseFocusView()
    {
        focusViewCanvas.SetActive(false);
    }

    public void DownloadMedia()
    {
        Debug.Log("Downloading media");
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startTouchPosition = Input.GetTouch(0).position;
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            endTouchPosition = Input.GetTouch(0).position;

            if (endTouchPosition.x > startTouchPosition.x)
            {
                // Swipe right
                ShowNextImage();
            }
            else if (endTouchPosition.x < startTouchPosition.x)
            {
                // Swipe left
                ShowPreviousImage();
            }
        }
    }

    void ShowNextImage()
    {
        // Increment the current image index
        currentImageIndex++;

        // Wrap around to the start if necessary
        if (currentImageIndex >= allData.Length)
        {
            currentImageIndex = 0;
        }

        // Show the image at the current index
        ShowMedia(allData[currentImageIndex].id);
    }

    void ShowPreviousImage()
    {
        // Decrement the current image index
        currentImageIndex--;

        // Wrap around to the end if necessary
        if (currentImageIndex < 0)
        {
            currentImageIndex = allData.Length - 1;
        }

        // Show the image at the current index
        ShowMedia(allData[currentImageIndex].id);
    }
}
