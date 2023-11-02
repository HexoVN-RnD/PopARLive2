using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using static CMSProjectImageImport;
using TMPro;
using System.Linq;
using System;

public class CMSProjectImageLoad : MonoBehaviour
{
    public GameObject imagePrefab;
    public Transform parentTransform;

    [SerializeField]
    private bool enableTesting = true;
    [SerializeField]
    private bool enablePublic = true;

    public static event Action OnImagesDisplayed;


    private void Start()
    {
        CMSProjectImageImport.OnDataDownloaded += DisplayImages;
    }

    private void OnDestroy()
    {
        CMSProjectImageImport.OnDataDownloaded -= DisplayImages;
    }


    public Data[] LoadData()
    {
        Debug.Log("Loading data...");

        string fullPath = Path.Combine(Application.persistentDataPath, "ProjectImage/Data.json");
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
                    title = jsonObject["title"],
                    image = jsonObject["image"],
                    order = jsonObject["order"],
                    visibility = jsonObject["visibility"],
                    start_date = jsonObject["start_date"],
                    end_date = jsonObject["end_date"],
                    time_zone = jsonObject["time_zone"],
                    experiences_size = jsonObject["experiences_size"],
                    status = jsonObject["status"]
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
            if (enableTesting && !enablePublic && item.visibility == "TESTING" && IsSupportedImageExtension(item.image))
            {
                Debug.Log("Displaying item with ID: " + item.id);
            }
            else if (!enableTesting && enablePublic && item.visibility == "PUBLIC" && IsSupportedImageExtension(item.image))
            {
                Debug.Log("Displaying item with ID: " + item.id);
            }
            else if (enableTesting && enablePublic && IsSupportedImageExtension(item.image))
            {
                Debug.Log("Displaying item with ID: " + item.id);
            }
            else
            {
                Debug.Log("Skipping item with ID: " + item.id);
                continue;
            }

            PlayerPrefs.SetInt(item.title, item.id);

            // Instantiate the prefab and get the Image component
            GameObject imageObject = Instantiate(imagePrefab);
            Image imageComponent = imageObject.GetComponent<Image>();

            // Get the Text components
            TextMeshProUGUI titleText = imageObject.transform.Find("TitleText").gameObject.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = imageObject.transform.Find("DateText").gameObject.GetComponent<TextMeshProUGUI>();

            // Parse the start and end dates
            DateTime startDate = DateTime.Parse(item.start_date);
            DateTime endDate = DateTime.Parse(item.end_date);

            // Format the dates
            string formattedStartDate = startDate.ToString("dd/MM/yyyy");
            string formattedEndDate = endDate.ToString("dd/MM/yyyy");

            // Set the text
            dateText.text = formattedStartDate + " - " + formattedEndDate;
            titleText.text = item.title;

            // Set the parent and position in hierarchy
            imageObject.transform.SetParent(parentTransform, false);
            imageObject.transform.SetSiblingIndex(parentTransform.childCount - 2);

            // Load the image
            string imagePath = Path.Combine(Application.persistentDataPath, "ProjectImage", item.image.Split('=')[1].Split('?')[0]);
            Texture2D texture = LoadImage(imagePath);

            // Convert the Texture2D to a Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Display the image
            imageComponent.sprite = sprite;
            imageComponent.preserveAspect = true;

            Debug.Log("Displayed image with ID: " + item.id);
        }

        Debug.Log("Finished displaying images");
        // Call the event
        OnImagesDisplayed?.Invoke();
    }
}
