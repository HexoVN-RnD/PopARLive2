using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using static CMSFeedImport;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class CMSFeedLoad : MonoBehaviour
{
    [SerializeField]
    private GameObject imagePrefab;
    [SerializeField]
    private Transform parentTransform;

    public delegate void LoadHandler();
    public static event LoadHandler OnLoadCompleted;
    public Dictionary<int, string> gridTypes = new Dictionary<int, string>();
    [System.NonSerialized]
    public List<int> imageIDs = new List<int>();

    private void OnEnable()
    {
        CMSFeedImport.OnImportCompleted += DisplayImages;
    }

    private void OnDestroy()
    {
        CMSFeedImport.OnImportCompleted -= DisplayImages;
    }

    public Data[] LoadData()
    {
        Debug.Log("Loading data...");

        string fullPath = Path.Combine(Application.persistentDataPath, "Feed/Data.json");
        if (File.Exists(fullPath))
        {
            Debug.Log("Found data file at " + fullPath);

            string json = File.ReadAllText(fullPath);
            JSONArray jsonArray = JSON.Parse(json).AsArray;

            Data[] data = new Data[jsonArray.Count];
            for (int i = 0; i < jsonArray.Count; i++)
            {
                JSONObject jsonObject = jsonArray[i].AsObject;
                data[i] = new Data
                {
                    id = jsonObject["id"],
                    media = jsonObject["media"],
                    caption = jsonObject["caption"],
                    grid_type = jsonObject["grid_type"],
                    media_type = jsonObject["media_type"],
                    createdAt = jsonObject["createdAt"],
                    updatedAt = jsonObject["updatedAt"]
                };
            }

            Debug.Log("Loaded " + data.Length + " items from data file");
            return data;
        }
        else
        {
            Debug.LogError("Save file not found at " + fullPath);
            return null;
        }
    }

    public Texture2D LoadImage(string imagePath)
    {
        Debug.Log("Loading image from " + imagePath);

        byte[] imageData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        Debug.Log("Loaded image from " + imagePath);
        return texture;
    }

    public bool IsSupportedImageExtension(string filePath)
    {
        string[] supportedExtensions = new string[] { ".jpg", ".png", ".jpeg" };
        string fileName = filePath.Split('=')[1].Split('?')[0];
        string fileExtension = Path.GetExtension(fileName).ToLower();
        Debug.Log("File name: " + fileName);
        Debug.Log("File path: " + filePath);
        Debug.Log("File extension: " + fileExtension);
        return supportedExtensions.Contains(fileExtension);
    }

    public void DisplayImages()
    {
        Debug.Log("Displaying images...");

        Data[] data = LoadData();
        foreach (var item in data)
        {
            if (!(item.media_type == "Image" || IsSupportedImageExtension(item.media)))
            {
                Debug.Log("Skipping item with ID: " + item.id);
                continue;
            }

            Debug.Log("Displaying image with ID: " + item.id);

            // Instantiate the prefab and get the Image component
            GameObject imageObject = Instantiate(imagePrefab, parentTransform);
            Image imageComponent = imageObject.GetComponentInChildren<Image>();
            AspectRatioFitter aspectRatioFitter = imageComponent.GetComponentInChildren<AspectRatioFitter>();

            // Load the image
            string imagePath = Path.Combine(Application.persistentDataPath, "Feed", item.media.Split('=')[1].Split('?')[0]);
            Texture2D texture = LoadImage(imagePath);

            // Convert the Texture2D to a Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Display the image
            imageComponent.sprite = sprite;
            //imageComponent.preserveAspect = true; // Ensure this is checked

            //Set the aspect ratio
            aspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; // Or AspectMode.EnvelopeParent

            Debug.Log("Displayed image with ID: " + item.id + " and Grid Type: " + item.grid_type);

            // // Add the item to the custom grid layout
            // RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            // GetComponent<CustomGridLayout>().AddItem(rectTransform, item.grid_type);
            if (!imageIDs.Contains(item.id))
            {
                imageIDs.Add(item.id);
            }
            else
            {
                Debug.Log("ID already exists in the list: " + item.id);
            }

            if (!gridTypes.ContainsKey(item.id))
            {
                gridTypes.Add(item.id, item.grid_type);
            }
            else
            {
                Debug.Log("ID already exists in the dictionary: " + item.id);
            }
        }
        // foreach (var pair in gridTypes)
        // {
        //     Debug.Log("ID: " + pair.Key);
        //     Debug.Log("Grid Type: " + pair.Value);
        // }

        Debug.Log("Finished displaying images");
        OnLoadCompleted?.Invoke();
    }
}