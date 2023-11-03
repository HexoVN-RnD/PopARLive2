using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestImport : MonoBehaviour
{
    [SerializeField]
    private string CMSPath;
    [SerializeField]
    private string localPath;
    [SerializeField]
    private bool CMSImport;
    [SerializeField]
    private bool localImport;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (CMSImport)
        {
            yield return StartCoroutine(ImportFromCMS(CMSPath));
        }
        if (localImport)
        {
            yield return StartCoroutine(ImportFromLocal(localPath));
        }
    }

    IEnumerator ImportFromCMS(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to load AssetBundle: " + request.error);
        }
        else
        {
            Debug.Log("Successfully loaded AssetBundle");
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            // Use GetAllAssetNames()
            string[] assetNames = bundle.GetAllAssetNames();
            foreach (string name in assetNames)
            {
                Debug.Log("Asset Name: " + name);
                GameObject prefab = bundle.LoadAsset<GameObject>(name);
                Instantiate(prefab);
            }
        }
    }

    IEnumerator ImportFromLocal(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is null or empty.");
            yield break;
        }

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
        yield return request;

        AssetBundle bundle = request.assetBundle;
        if (bundle == null)
        {
            Debug.LogError("Failed to load AssetBundle from disk.");
            yield break;
        }

        Debug.Log("Successfully loaded AssetBundle from disk.");
        string[] assetNames = bundle.GetAllAssetNames();
        foreach (string name in assetNames)
        {
            Debug.Log("Asset Name: " + name);
            GameObject prefab = bundle.LoadAsset<GameObject>(name);
            Instantiate(prefab);
        }
    }
}
