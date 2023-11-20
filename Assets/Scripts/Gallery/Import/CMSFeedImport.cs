using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class CMSFeedImport : MonoBehaviour
{
    public delegate void ImportHandler();
    public static event ImportHandler OnImportCompleted;

    [SerializeField]
    private bool alwaysDownload; // Set to true to always download the files, even if they already exist

    private string apiLink = "https://popar-backend.acstech.vn/api/v3/feed?pageNo=0&pageSize=1000";

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
        public string media;
        public string caption;
        public string grid_type;
        public string media_type;
        public string createdAt;
        public string updatedAt;
    }

    void Start()
    {
        Debug.Log("Starting CSM Feed Import Script...");
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
            
            foreach (var item in jsonData.data)
            {
                Debug.Log("Processing item with ID: " + item.id + ", Caption: " + item.caption + ", Media Type: " + item.media_type + ", Grid Type: " + item.grid_type + " and Created Date: " + item.createdAt);

                string originalFileName = item.media.Split('=')[1].Split('?')[0];
                string fileName = "Feed/" + originalFileName;

                string fileExtension = Path.GetExtension(fileName);
                string filePath = Application.persistentDataPath + "/" + fileName;
                Debug.Log("Local File path: " + filePath);


                if (fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
                {
                    dataList.Add(item);
                    Debug.Log("File extension is supported: " + fileExtension);

                    // Replace http with https in the URL
                    string secureUrl = item.media.Replace("http://", "https://");

                    // Check if the file exists and if it has been updated
                    System.DateTime localUpdateTime = File.GetLastWriteTime(filePath);
                    System.DateTime serverUpdateTime = System.DateTime.Parse(item.updatedAt);
                    Debug.Log("Local update time: " + localUpdateTime + " Server update time " + serverUpdateTime);
                    if (alwaysDownload || !File.Exists(filePath) || localUpdateTime < serverUpdateTime)
                    {
                        Debug.Log("File does not exist or has been updated. Starting download...");
                        yield return StartCoroutine(DownloadFile(secureUrl, fileName));
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
            // Save the data
            SaveData(dataList.ToArray());

            OnImportCompleted?.Invoke();
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
            jsonObject["media"] = item.media;
            jsonObject["caption"] = item.caption;
            jsonObject["grid_type"] = item.grid_type;
            jsonObject["media_type"] = item.media_type;
            jsonObject["createdAt"] = item.createdAt;
            jsonObject["updatedAt"] = item.updatedAt;

            jsonArray.Add(jsonObject);
        }

        string fullPath = Path.Combine(Application.persistentDataPath, "Feed/Data.json");
        File.WriteAllText(fullPath, jsonArray.ToString());
    }
}
