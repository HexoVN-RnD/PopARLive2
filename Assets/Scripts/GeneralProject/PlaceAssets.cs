using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Playables;
using static CMSImportAssets;

public class PlaceAssets : MonoBehaviour
{
    [SerializeField]
    private List<Texture2D> ArImages;

    [SerializeField]
    private ARTrackedImageManager m_TrackedImageManager;

    [SerializeField]
    private static PlaceAssets Instance;

    [SerializeField]
    private List<GameObject> ArPrefabs;

    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);  // Wait for ARSession to initialize

        var mutableLibrary = m_TrackedImageManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

        foreach (var texture in ArImages)
        {
            Debug.Log("Adding image to library: " + texture.name);
            var jobHandle = mutableLibrary.ScheduleAddImageWithValidationJob(texture, texture.name, 0.1f, default);
            yield return jobHandle;
        }
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            string key = trackedImage.referenceImage.name.Replace(".png", "");
            if (CMSImportAssets.prefabDictionary.ContainsKey(key))
            {
                Debug.Log("Instantiating prefab for key: " + key);
                GameObject prefab = CMSImportAssets.prefabDictionary[key];
                if (prefab == null)
                {
                    Debug.LogError("Prefab for key: " + key + " is null");
                    continue;
                }
                //GameObject instance = Instantiate(prefab, trackedImage.transform.position, Quaternion.identity);
                GameObject instance = Instantiate(prefab, trackedImage.transform.position, prefab.transform.rotation);
                instance.transform.parent = trackedImage.transform;
                instance.SetActive(true);
                Debug.Log("Scaling up: " + key);

                StartCoroutine(ScaleUp(instance, key));

                _instantiatedPrefabs.Add(key, instance);
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            string key = trackedImage.referenceImage.name.Replace(".png", "");
            if (_instantiatedPrefabs.ContainsKey(key))
            {
                //_instantiatedPrefabs[key].SetActive(trackedImage.trackingState == TrackingState.Tracking);

                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    _instantiatedPrefabs[key].transform.position = trackedImage.transform.position;
                }
            }
        }

        // foreach (var trackedImage in eventArgs.removed)
        // {
        //     string key = trackedImage.referenceImage.name.Replace(".png", "");
        //     Debug.Log("Removing prefab for key: " + key);
        //     Destroy(_instantiatedPrefabs[key]);
        //     _instantiatedPrefabs.Remove(key);
        // }
    }

    IEnumerator ScaleUp(GameObject instance, string key)
    {
        yield return new WaitForSeconds(1);
        // Get the corresponding Experience object
        Debug.Log(instance.name + " key " + key);
        Experience experience = CMSImportAssets.experienceDictionary.ContainsKey(key) ? CMSImportAssets.experienceDictionary[key] : null;
        // Use the fade_in value as the duration
        float duration = experience != null ? experience.fade_in : 3.0f;
        // Use the scale value as the maximum scale
        Vector3 maxScale = experience != null ? new Vector3(experience.scale/100, experience.scale/100, experience.scale/100) : Vector3.one;
        Debug.Log("Scaling up for " + duration + " seconds" + " to " + maxScale);
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            instance.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        instance.transform.localScale = maxScale;
    }



    // IEnumerator FadeIn(GameObject instance)
    // {
    //     if (instance == null)
    //     {
    //         Debug.LogError("Instance is null");
    //         yield break;
    //     }

    //     // Get all renderers in the instance and its children
    //     Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();

    //     foreach (Renderer renderer in renderers)
    //     {
    //         if (renderer == null)
    //         {
    //             Debug.LogError("Renderer is null");
    //             continue;
    //         }

    //         // Get all materials of the current renderer
    //         Material[] materials = renderer.materials;

    //         foreach (Material material in materials)
    //         {
    //             if (material == null)
    //             {
    //                 Debug.LogError("Material is null");
    //                 continue;
    //             }

    //             // Set the initial alpha to 0
    //             Color color = material.color;
    //             color.a = 0;
    //             material.color = color;

    //             // Gradually increase the alpha to 1 over x seconds
    //             float duration = 5.0f; // Duration in seconds
    //             for (float t = 0; t < duration; t += Time.deltaTime)
    //             {
    //                 color.a = Mathf.Lerp(0, 1, t / duration);
    //                 material.color = color;
    //                 yield return null;
    //             }

    //             // Ensure the alpha is set to 1
    //             color.a = 1;
    //             material.color = color;
    //         }
    //     }
    // }

    // IEnumerator FadeOut(GameObject instance)
    // {
    //     if (instance == null)
    //     {
    //         Debug.LogError("Instance is null");
    //         yield break;
    //     }

    //     // Get all renderers in the instance and its children
    //     Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();

    //     foreach (Renderer renderer in renderers)
    //     {
    //         if (renderer == null)
    //         {
    //             Debug.LogError("Renderer is null");
    //             continue;
    //         }

    //         // Get all materials of the current renderer
    //         Material[] materials = renderer.materials;

    //         foreach (Material material in materials)
    //         {
    //             if (material == null)
    //             {
    //                 Debug.LogError("Material is null");
    //                 continue;
    //             }

    //             // Set the initial alpha to 1
    //             Color color = material.color;
    //             color.a = 1;
    //             material.color = color;

    //             // Gradually decrease the alpha to 0 over x seconds
    //             float duration = 3.0f; // Duration in seconds
    //             for (float t = 0; t < duration; t += Time.deltaTime)
    //             {
    //                 color.a = Mathf.Lerp(1, 0, t / duration);
    //                 material.color = color;
    //                 yield return null;
    //             }

    //             // Ensure the alpha is set to 0
    //             color.a = 0;
    //             material.color = color;
    //         }
    //     }
    // }
}
