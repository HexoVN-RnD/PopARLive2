using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using static CMSFeedImport;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Video;
using System.Collections;

public class CMSFeedLoad : MonoBehaviour
{
    [SerializeField]
    private GameObject imagePrefab;
    [SerializeField]
    private GameObject videoThumbnailPrefab;
    [SerializeField]
    private Transform parentTransform;

    public delegate void LoadHandler();
    public static event LoadHandler OnLoadCompleted;
    public Dictionary<int, string> gridTypes = new Dictionary<int, string>();
    [System.NonSerialized]
    public List<int> imageIDs = new List<int>();

    private void Start()
    {
        // Get a reference to the other script
        CMSFeedImport otherScript = GetComponent<CMSFeedImport>();

        // Check if the other script is enabled
        if (otherScript == null || !otherScript.enabled)
        {
            StartDisplayMedia();
        }
    }

    private void OnEnable()
    {
        // Only subscribe to the event if the other script is enabled
        CMSFeedImport importScript = GetComponent<CMSFeedImport>();
        if (importScript != null && importScript.enabled)
        {
            CMSFeedImport.OnImportCompleted += StartDisplayMedia;
        }
    }

    private void OnDestroy()
    {
        CMSFeedImport.OnImportCompleted -= StartDisplayMedia;
    }

    private void StartDisplayMedia()
    {
        StartCoroutine(DisplayMedia());
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
        return supportedExtensions.Contains(fileExtension);
    }

    public bool IsSupportedVideoExtension(string filePath)
    {
        string[] supportedExtensions = new string[] { ".mp4", ".mov", ".avi" };
        string fileName = filePath.Split('=')[1].Split('?')[0];
        string fileExtension = Path.GetExtension(fileName).ToLower();
        return supportedExtensions.Contains(fileExtension);
    }

    IEnumerator ExtractThumbnail(string videoPath, string thumbnailPath)
    {
        // Create a new GameObject
        GameObject videoPlayerGameObject = new GameObject("VideoPlayer");

        // Add the VideoPlayer component to the GameObject
        VideoPlayer videoPlayer = videoPlayerGameObject.AddComponent<VideoPlayer>();

        // Set the VideoPlayer properties
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.playOnAwake = false;

        // Prepare the VideoPlayer (load the video in memory without playing it)
        videoPlayer.Prepare();

        // Wait until the video is prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // Create a RenderTexture with the same resolution as the video
        RenderTexture renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 24);
        videoPlayer.targetTexture = renderTexture;

        // Play the video
        videoPlayer.Play();

        // Wait one frame
        yield return null;

        // Pause the video
        videoPlayer.Pause();

        // Create a Texture2D and read the RenderTexture image into it
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Convert the Texture2D to a .PNG image and save it to disk
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(thumbnailPath, pngData);

        // Clean up
        videoPlayer.Stop();
        Destroy(videoPlayer);
        Destroy(renderTexture);
    }


    IEnumerator DisplayMedia()
    {
        Debug.Log("Displaying images...");

        Data[] data = LoadData();
        foreach (var item in data)
        {
            if (item.media_type == "Image" || IsSupportedImageExtension(item.media))
            {
                yield return StartCoroutine(DisplayImage(item));
            }
            else if (item.media_type == "Video" || IsSupportedVideoExtension(item.media))
            {
                yield return StartCoroutine(DisplayVideo(item));
            }
            else
            {
                Debug.LogError("Unsupported media type: " + item.media_type);
            }
        }

        Debug.Log("Finished displaying images");
        OnLoadCompleted?.Invoke();
    }

    IEnumerator DisplayImage(Data item)
    {
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

        yield return null;
    }

    IEnumerator DisplayVideo(Data item)
    {
        Debug.Log("Displaying video with ID: " + item.id);

        // Extract the first frame of the video as thumbnail
        string videoPath = Path.Combine(Application.persistentDataPath, "Feed", item.media.Split('=')[1].Split('?')[0]);
        string thumbnailPath = Path.Combine(Application.persistentDataPath, "Feed", "Thumbnail_" + item.media.Split('=')[1].Split('?')[0]);
        thumbnailPath = thumbnailPath.Substring(0, thumbnailPath.Length - 3) + "png";
        Debug.Log("Video Path: " + videoPath);
        Debug.Log("Thumbnail Path: " + thumbnailPath);
        yield return StartCoroutine(ExtractThumbnail(videoPath, thumbnailPath));

        // Instantiate the video thumbnail prefab and get the Image component
        GameObject videoThumbnailObject = Instantiate(videoThumbnailPrefab, parentTransform);
        Image imageComponent = videoThumbnailObject.GetComponentInChildren<Image>();
        AspectRatioFitter aspectRatioFitter = imageComponent.GetComponentInChildren<AspectRatioFitter>();

        // Load the thumbnail image
        Texture2D thumbnailTexture = LoadImage(thumbnailPath);

        // Convert the Texture2D to a Sprite
        Sprite sprite = Sprite.Create(thumbnailTexture, new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), new Vector2(0.5f, 0.5f));

        // Display the thumbnail image
        imageComponent.sprite = sprite;

        //Set the aspect ratio
        aspectRatioFitter.aspectRatio = (float)thumbnailTexture.width / thumbnailTexture.height;
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; // Or AspectMode.EnvelopeParent

        Debug.Log("Displayed video thumbnail with ID: " + item.id + " and Grid Type: " + item.grid_type);

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

        yield return null;
    }
}
