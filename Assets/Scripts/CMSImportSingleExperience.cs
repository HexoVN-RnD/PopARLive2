using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Collections;
using SimpleJSON;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public class CMSImportSingleExperience : MonoBehaviour
{
    public static event Action<string, GameObject> OnPrefabDownloaded;

    public CMSSpawnObject placeARObject;
    public static Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    public string projectTitle;
    public bool isTesting = true;
    public string experienceName;
    public bool downloadAssetOnly = false;

    private string projectAPI = "https://popar-backend.acstech.vn/api/v3/project?pageNo=0&pageSize=1000&title=";
    private string projectIDAPI = "https://popar-backend.acstech.vn/api/v3/project/";

    [System.Obsolete]
    void Start()
    {
        StartCoroutine(GetProjectID());
    }

    [System.Obsolete]
    IEnumerator GetProjectID()
    {
        UnityWebRequest www = UnityWebRequest.Get(projectAPI + projectTitle);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            var jsonResponse = JSON.Parse(www.downloadHandler.text);
            var data = jsonResponse["data"].AsArray;
            foreach (JSONNode project in data)
            {
                if (project["visibility"].Value == (isTesting ? "TESTING" : "PUBLIC"))
                {
                    StartCoroutine(GetExperience(project["id"].AsInt));
                    break;
                }
            }
        }
    }

    [System.Obsolete]
    IEnumerator GetExperience(int id)
    {
        UnityWebRequest www = UnityWebRequest.Get(projectIDAPI + id);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            var jsonResponse = JSON.Parse(www.downloadHandler.text);
            var experiences = jsonResponse["data"]["experiences"].AsArray;
            foreach (JSONNode experience in experiences)
            {
                if (experience["name"].Value == experienceName)
                {
                    string appleImage = experience["apple_image"];
                    string chPlayImage = experience["ch_play_image"];
                    string arImage = experience["ar_image"];
                    string appleImageName = experience["name_file_apple_image"];
                    string chPlayImageName = experience["name_file_ch_play_image"];
                    float xTracking = experience["x_tracking"].AsFloat;
                    float yTracking = experience["y_tracking"].AsFloat;
                    int fadeIn = experience["fade_in"].AsInt;
                    int fadeOut = experience["fade_out"].AsInt;

                    string markerName = arImage.Split('=')[1].Split('?')[0];
                    string bundleName = appleImageName;
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        bundleName = chPlayImageName;
                    }

                    string markerLink = arImage;
                    string bundleLink = "http://popar-backend.acstech.vn/filename=" + bundleName + "?bucket=projects";

                    Debug.Log("Bundle link: " + bundleLink);
                    Debug.Log("Marker link: " + markerLink);

                    StartCoroutine(DownloadExperience(bundleLink, markerLink, bundleName, markerName, xTracking, yTracking));

                    break;
                }
            }
        }
    }

    [System.Obsolete]
    IEnumerator DownloadExperience(string bundleUrl, string markerUrl, string bundleName, string markerName, float width, float height)
    {
        bundleUrl = bundleUrl.Replace("http://", "https://");
        markerUrl = markerUrl.Replace("http://", "https://");

        // Download and load the asset bundle
        UnityWebRequest bundleRequest = UnityWebRequest.Get(bundleUrl);
        yield return bundleRequest.SendWebRequest();

        if (bundleRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download AssetBundle: " + bundleRequest.error);
        }
        else
        {
            string bundlePath = Path.Combine(Application.persistentDataPath, bundleName);
            File.WriteAllBytes(bundlePath, bundleRequest.downloadHandler.data);
            Debug.Log("Successfully downloaded AssetBundle: " + bundlePath + bundleName);

            AssetBundle bundle = AssetBundle.LoadFromMemory(bundleRequest.downloadHandler.data);
            string prefabName = AddAssetToDictionary(bundle, bundleName);

            if (!downloadAssetOnly)
            {
                // Download and add the marker image
                UnityWebRequest markerRequest = UnityWebRequestTexture.GetTexture(markerUrl);
                yield return markerRequest.SendWebRequest();

                if (markerRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download image: " + markerRequest.error);
                }
                else
                {
                    string markerPath = Path.Combine(Application.persistentDataPath, markerName);
                    Debug.Log("Successfully downloaded image");
                    File.WriteAllBytes(markerPath, markerRequest.downloadHandler.data);

                    Texture2D texture = DownloadHandlerTexture.GetContent(markerRequest);
                    AddMarkerToLibrary(texture, prefabName, width, height);
                }
            }
        }
    }


    [System.Obsolete]
    void AddMarkerToLibrary(Texture2D image, string name, float width, float height)
    {
#if !UNITY_EDITOR
        // Get the reference image library
        var imageManager = GetComponent<ARTrackedImageManager>();
        var mutableLibrary = imageManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

        mutableLibrary.ScheduleAddImageJob(image, experienceName, width); //only width of the marker is needed, height will scale according to aspect ratio

        // Log the contents of the reference image library
        Debug.Log("Reference Image Library:");
        for (int i = 0; i < mutableLibrary.count; i++)
        {
            var referenceImage = mutableLibrary[i];
            Debug.Log("Image " + i + ": " + referenceImage.name);
        }
#endif
    }


    string AddAssetToDictionary(AssetBundle bundle, string fileName)
    {
        string[] assetNames = bundle.GetAllAssetNames();

        // CMS only has 1 asset in each assetbundle so only get the first one
        string fullPath = assetNames[0];

        // Extract just the name from the full path
        string name = Path.GetFileNameWithoutExtension(fullPath);

        Debug.Log("Asset Name: " + name);

        GameObject prefab = bundle.LoadAsset<GameObject>(fullPath);

        ExperienceDictionary.prefabDictionary.Add(experienceName, prefab);

        // Invoke the event
        OnPrefabDownloaded?.Invoke(experienceName, prefab);
        return name;
    }
}
