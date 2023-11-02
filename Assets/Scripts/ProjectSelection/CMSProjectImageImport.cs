using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class CMSProjectImageImport : MonoBehaviour
{
    [SerializeField]
    private bool alwaysDownload; // Set this to true if you want to always download the image

    public static event Action OnDataDownloaded;

    private string apiLink = "https://popar-backend.acstech.vn/api/v3/project?pageNo=0&pageSize=1000";

    [System.Serializable]
    public class ResponseData
    {
        public bool status;
        public string message;
        public Data[] data;
    }

    [System.Serializable]
    public class Data
    {
        public int id;
        public string title;
        public string image;
        public string order;
        public string visibility;
        public string start_date;
        public string end_date;
        public string time_zone;
        public string experiences_size;
        public string status;
    }

    void Start()
    {
        Debug.Log("Starting CSM Project Image Import Script...");
        StartCoroutine(GetDataFromAPI(apiLink));
    }

    IEnumerator GetDataFromAPI(string uri)
    {
        Debug.Log("Sending GET request to API: " + uri);

        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Received response from API");

            var jsonData = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            List<Data> dataList = new List<Data>();
            List<Coroutine> downloadCoroutines = new List<Coroutine>();
            foreach (var item in jsonData.data)
            {
                Debug.Log("Processing item with ID: " + item.id + ", Title: " + item.title + ", Image: " + item.image + ", Order: " + item.order + ", Visibility: " + item.visibility + ", Start Date: " + item.start_date + ", End Date: " + item.end_date + ", Time Zone: " + item.time_zone + ", Experiences Size: " + item.experiences_size + ", Status: " + item.status);

                string originalFileName = item.image.Split('=')[1].Split('?')[0];
                string fileName = "ProjectImage/" + originalFileName;

                string fileExtension = Path.GetExtension(fileName);
                string filePath = Application.persistentDataPath + "/" + fileName;
                Debug.Log("Local File path: " + filePath);


                if (fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    dataList.Add(item);
                    Debug.Log("File extension is supported: " + fileExtension);

                    // Replace http with https in the URL
                    string secureUrl = item.image.Replace("http://", "https://");

                    if (alwaysDownload || !File.Exists(filePath))
                    {
                        Debug.Log("File does not exist or has been updated. Starting download...");
                        Coroutine downloadCoroutine = StartCoroutine(DownloadFile(secureUrl, fileName));
                        downloadCoroutines.Add(downloadCoroutine);
                    }
                    else
                    {
                        Debug.Log("File already exists and has not been updated.");
                    }
                }
                else
                {
                    Debug.Log("File extension is not supported: " + fileExtension);
                }
            }
            // Wait for all downloads to finish
            foreach (Coroutine coroutine in downloadCoroutines)
            {
                yield return coroutine;
            }

            // Save the data
            SaveData(dataList.ToArray());

            // Call the event after the data has been downloaded
            OnDataDownloaded?.Invoke();
        }
        else
        {
            Debug.LogError("Error sending GET request to API: " + request.error);
        }
    }

    IEnumerator DownloadFile(string uri, string fileName)
    {
        Debug.Log("Downloading file from URI: " + uri);

        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string fullPath = Application.persistentDataPath + "/" + fileName;
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, request.downloadHandler.data);
            Debug.Log("File successfully downloaded and saved to " + fullPath);
        }
        else
        {
            Debug.LogError("Error downloading file from URI: " + request.error);
        }
    }

    public void SaveData(Data[] data)
    {
        JSONArray jsonArray = new JSONArray();
        foreach (var item in data)
        {
            JSONObject jsonObject = new JSONObject();
            jsonObject["id"] = item.id;
            jsonObject["title"] = item.title;
            jsonObject["image"] = item.image;
            jsonObject["order"] = item.order;
            jsonObject["visibility"] = item.visibility;
            jsonObject["start_date"] = item.start_date;
            jsonObject["end_date"] = item.end_date;
            jsonObject["time_zone"] = item.time_zone;
            jsonObject["experiences_size"] = item.experiences_size;
            jsonObject["status"] = item.status;

            jsonArray.Add(jsonObject);
        }

        string fullPath = Path.Combine(Application.persistentDataPath, "ProjectImage/Data.json");
        File.WriteAllText(fullPath, jsonArray.ToString());
    }
}

