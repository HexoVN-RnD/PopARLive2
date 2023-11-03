using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public class CMSImportAssets : MonoBehaviour
{
    // Define the delegate and event for data download start
    public delegate void DataDownloadStartHandler();
    public static event DataDownloadStartHandler OnDataDownloadStart;

    // Define the delegate and event for data download end
    public delegate void DataDownloadEndHandler();
    public static event DataDownloadEndHandler OnDataDownloadEnd;

    [SerializeField]
    private PlaceAssets placeARObject;
    [SerializeField]
    private bool redownloadAssets = true;
    public static Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    [Serializable]
    public class ResponseData
    {
        public bool status;
        public string message;
        public ProjectData data;
    }

    [Serializable]
    public class ProjectData
    {
        public int id;
        public string title;
        public string desc;
        public string visibility;
        public string image;
        public string hash_tag;
        public Schedule schedule;
        public Experience[] experiences;
    }

    [Serializable]
    public class Schedule
    {
        public int id;
        public string startDate;
        public string endDate;
        public string time_zone;
        public Item[] items; // Define the Item class based on your actual data
    }

    [Serializable]
    public class Experience
    {
        public int id;
        public string name;
        public string ar_content_type;
        public string apple_image;
        public string ch_play_image;
        public string name_file_apple_image;
        public string name_file_ch_play_image;
        public string ar_image;
        public float x_tracking;
        public float y_tracking;
        public float fade_in;
        public float fade_out;
        public float opacity;
        public float scale;
        public float rotation_offset;
        public string light_color;
        public string light_brightness;
        public bool? main;
        public float gps;
        public Schedule schedule;
    }

    [Serializable]
    public class Item
    {
    }

    IEnumerator Start()
    {
        // Get the project ID from the player prefs
        //int projectID = PlayerPrefs.GetInt("ProjectID", 0);
        int projectID = 2;
        string apiURL = "https://popar-backend.acstech.vn/api/v3/project/" + projectID;
        Debug.Log("API URL: " + apiURL);
        UnityWebRequest request = UnityWebRequest.Get(apiURL);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("API request failed: " + request.error);
        }
        else
        {
            Debug.Log("Successfully received API response");
            OnDataDownloadStart?.Invoke();
            // Parse the JSON response into a ProjectData object
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            // Save the data
            SaveData(responseData.data, projectID);
            int counter = 1;
            foreach (Experience experience in responseData.data.experiences)
            {
                string bundleLink;
                string markerLink = experience.ar_image;

                // Get the filename of the bundle image for the current platform
                string filename = experience.name_file_apple_image;
                if (Application.platform == RuntimePlatform.Android)
                {
                    filename = experience.name_file_ch_play_image;
                }
                //string imagename = ((string)experience["ar_image"]).Split('=')[1].Split('?')[0];

                // Construct the bundle link
                bundleLink = "http://popar-backend.acstech.vn/filename=" + filename + "?bucket=projects";

                Debug.Log("Bundle link: " + bundleLink);
                Debug.Log("Marker link: " + markerLink);

                // Now you can use these links to download and import your asset bundles
                StartCoroutine(DownloadAndCacheAssetBundle(bundleLink, counter.ToString(), projectID));
                StartCoroutine(DownloadAndCacheImage(markerLink, counter.ToString() + ".png", projectID, experience.x_tracking));

                // StartCoroutine(DownloadAndCacheAssetBundle(bundleLink, filename));
                // StartCoroutine(DownloadAndCacheImage(markerLink, imagename));

                counter++;
            }
            OnDataDownloadEnd?.Invoke();
        }
    }

    public void SaveData(ProjectData projectData, int projectID)
    {
        // Convert the ProjectData object to a JSON string
        string jsonData = JsonUtility.ToJson(projectData);

        // Save the JSON string to a file
        string tempPath1 = Path.Combine(Application.persistentDataPath, "ProjectAssets");
        string tempPath2 = Path.Combine(tempPath1, projectID.ToString());
        Directory.CreateDirectory(tempPath2);
        string fullPath = Path.Combine(tempPath2, "Data.json");
        File.WriteAllText(fullPath, jsonData);
    }

    IEnumerator DownloadAndCacheAssetBundle(string url, string fileName, int projectID)
    {
        // Replace 'http' with 'https' in the URL
        url = url.Replace("http://", "https://");

        string folderPath1 = Path.Combine(Application.persistentDataPath, "ProjectAssets");
        string folderPath2 = Path.Combine(folderPath1, projectID.ToString());
        string localPath = Path.Combine(folderPath2, fileName);
        Directory.CreateDirectory(folderPath2);

        if (File.Exists(localPath) && !redownloadAssets)
        {
            Debug.Log("Loading file from cache: " + localPath);

            // Load and process AssetBundle
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(localPath); // Load the AssetBundle asynchronously
            yield return request; // Wait for the request to finish

            if (request.assetBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle from cache");
            }
            else
            {
                AddAssetsToPlaceARObject(request.assetBundle, fileName); // Add the assets to the place AR object
            }
        }
        else
        {
            // Download and process AssetBundle
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download AssetBundle: " + request.error);
            }
            else
            {
                Debug.Log("Successfully downloaded AssetBundle");
                File.WriteAllBytes(localPath, request.downloadHandler.data);

                AssetBundle bundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
                AddAssetsToPlaceARObject(bundle, fileName);
            }
        }
    }

    IEnumerator DownloadAndCacheImage(string url, string fileName, int projectID, float x_tracking)
    {
        // Replace 'http' with 'https' in the URL
        url = url.Replace("http://", "https://");

        string folderPath1 = Path.Combine(Application.persistentDataPath, "ProjectAssets");
        string folderPath2 = Path.Combine(folderPath1, projectID.ToString());
        string localPath = Path.Combine(folderPath2, fileName);
        Directory.CreateDirectory(folderPath2);

        if (File.Exists(localPath) && !redownloadAssets)
        {
            Debug.Log("Loading file from cache: " + localPath);

            // Load and process regular image file
            byte[] imageData = File.ReadAllBytes(localPath); // Read the image data from the file
            Texture2D texture = new Texture2D(2, 2); // Create a new texture
            texture.LoadImage(imageData); // Load the image data into the texture
            Debug.Log("Loading image: " + fileName);
            AddImageToReferenceLibrary(texture, fileName, x_tracking); // Add the image to the reference library
        }
        else
        {
            // Download and process regular image file
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download image: " + request.error);
            }
            else
            {
                Debug.Log("Successfully downloaded image");
                File.WriteAllBytes(localPath, request.downloadHandler.data);

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                AddImageToReferenceLibrary(texture, fileName, x_tracking);
            }
        }
    }

    void AddAssetsToPlaceARObject(AssetBundle bundle, string fileName)
    {
#if !UNITY_EDITOR
        // Use GetAllAssetNames()
        string[] assetNames = bundle.GetAllAssetNames();
        foreach (string name in assetNames)
        {
            Debug.Log("Asset Name: " + name);
            GameObject prefab = bundle.LoadAsset<GameObject>(name);
            string key = fileName.Replace(".png", "");
            // After loading a prefab from the AssetBundle:
            prefabDictionary.Add(key, prefab);
        }

        // Log the list of prefab names
        foreach (var key in prefabDictionary.Keys)
        {
            Debug.Log("Prefabs: " + key);
        }
#endif
    }

    void AddImageToReferenceLibrary(Texture2D image, string name, float x_tracking)
    {
#if !UNITY_EDITOR
        // Get the reference image library
        var imageManager = GetComponent<ARTrackedImageManager>();
        var mutableLibrary = imageManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

        // Add the image to the library
        mutableLibrary.ScheduleAddImageWithValidationJob(image, name, x_tracking, default);

        // Log the contents of the reference image library
        Debug.Log("Reference Image Library:");
        for (int i = 0; i < mutableLibrary.count; i++)
        {
            var referenceImage = mutableLibrary[i];
            Debug.Log("Image " + i + ": " + referenceImage.name);
        }
#endif
    }
}