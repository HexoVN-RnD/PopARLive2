using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CMSSpawnObject : MonoBehaviour
{
    private bool hasSpawnedAllEffects = false;
    private bool isImageDetected = false;
    public ARTrackedImageManager m_TrackedImageManager;

    // Store the key of the last tracked image
    private string _lastTrackedImageKey = null;

    // Store a reference to the instantiated prefabs
    private Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        CMSImportSingleExperience.OnPrefabDownloaded += HandlePrefabDownloaded;
    }

    private void OnDisable()
    {
        CMSImportSingleExperience.OnPrefabDownloaded -= HandlePrefabDownloaded;
    }

    private void HandlePrefabDownloaded(string key, GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        instance.SetActive(false);

        _instantiatedPrefabs.Add(key, instance);
    }

    private void Update()
    {
        if (m_TrackedImageManager == null)
        {
            Debug.LogError("m_TrackedImageManager is null");
            return;
        }

        foreach (var trackedImage in m_TrackedImageManager.trackables)
        {
            if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                if (trackedImage?.referenceImage.name == null)
                {
                    Debug.LogError("Tracked image or reference image name is null");
                    continue;
                }

                string key = trackedImage.referenceImage.name;

                // Special case for "Banner"
                if (key == "Banner")
                {
                    // If the coroutine is running, disable the "Banner" prefab
                    if (isImageDetected)
                    {
                        if (_instantiatedPrefabs.ContainsKey(key))
                        {
                            _instantiatedPrefabs[key].SetActive(false);
                        }
                        continue;
                    }

                    // If the coroutine is not running, spawn the "Banner" prefab indefinitely
                    if (_instantiatedPrefabs.ContainsKey(key))
                    {
                        GameObject instance = _instantiatedPrefabs[key];
                        instance.transform.position = trackedImage.transform.position;
                        //instance.transform.rotation = trackedImage.transform.rotation;
                        instance.SetActive(true);
                    }
                    continue;
                }

                if (key != "Effect1")
                {
                    continue;
                }

                isImageDetected = true;

                if (_instantiatedPrefabs.ContainsKey(key))
                {
                    // Update the position and rotation of the prefab to match the tracked image
                    GameObject instance = _instantiatedPrefabs[key];
                    instance.transform.position = trackedImage.transform.position;
                    //instance.transform.rotation = trackedImage.transform.rotation;

                    // If the currently tracked image is the same as the last tracked image, do nothing
                    if (_lastTrackedImageKey == key)
                    {
                        continue;
                    }

                    Debug.Log("Starting coroutine to spawn prefab based on time");
                    StartCoroutine(StartSpawningEffects(key, trackedImage.transform));

                    _lastTrackedImageKey = key;

                    // foreach (var instantiatedPrefab in _instantiatedPrefabs.Values)
                    // {
                    //     instantiatedPrefab.SetActive(false);
                    // }
                    // // Enable the prefab for the currently tracked image and update its position and rotation
                    // GameObject instance = _instantiatedPrefabs[key];
                    // instance.transform.position = trackedImage.transform.position;
                    // //instance.transform.rotation = trackedImage.transform.rotation;
                    // instance.SetActive(true);
                }
            }
            else
            {
                isImageDetected = false;
            }
        }
    }

    private IEnumerator StartSpawningEffects(string key, Transform trackedImageTransform)
    {
        while (true)
        {
            yield return StartCoroutine(SpawnPrefabBasedOnTime(key, trackedImageTransform));
        }
    }

    private IEnumerator SpawnPrefabBasedOnTime(string key, Transform trackedImageTransform)
    {
        while (!hasSpawnedAllEffects && isImageDetected)
        {
            // Get the current time
            System.DateTime now = System.DateTime.Now;
            Debug.Log("Current time: " + now);

            // Define the time ranges
            System.TimeSpan groupAStart1 = new System.TimeSpan(19, 0, 0);  // 7 PM
            System.TimeSpan groupAEnd1 = new System.TimeSpan(23, 54, 59);  // 11:55 PM
            System.TimeSpan groupAStart2 = new System.TimeSpan(0, 10, 0);  // 12:10 AM
            System.TimeSpan groupAEnd2 = new System.TimeSpan(1, 9, 59);  // 1:10 AM
            System.TimeSpan groupBStart = new System.TimeSpan(23, 55, 0);  // 11:55 PM
            System.TimeSpan groupBEnd = new System.TimeSpan(23, 59, 59);  // Midnight
            System.TimeSpan groupCStart = new System.TimeSpan(0, 0, 0);  // Midnight
            System.TimeSpan groupCEnd = new System.TimeSpan(0, 9, 59);  // 12:10 AM

            if ((now.TimeOfDay >= groupAStart1 && now.TimeOfDay <= groupAEnd1) || (now.TimeOfDay >= groupAStart2 && now.TimeOfDay <= groupAEnd2)) // 7PM-11:55PM or 12:10PM-1:10PM
            {
                Debug.Log("Between 7PM-11:55PM or 12:10PM-1:10PM");
                SpawnPrefab("Effect1", trackedImageTransform);

                // The for loop is for the transition of each phase. For example at exactly 11:55 PM, phase 1 will force stop and phase 2 will begin.

                for (int i = 0; i < 15; i++) // Effect 1 will last for 60 seconds (i < x seconds)
                {
                    now = System.DateTime.Now;
                    if (now.TimeOfDay >= groupBStart && now.TimeOfDay < groupBEnd)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(1); // Checking transition every second
                }

                SpawnPrefab("Effect2", trackedImageTransform);

                for (int i = 0; i < 15; i++) // Effect 2 will last for 180 seconds (i < x seconds)
                {
                    now = System.DateTime.Now;
                    if (now.TimeOfDay >= groupBStart && now.TimeOfDay < groupBEnd)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(1); // Checking transition every second
                }

                hasSpawnedAllEffects = true;
                StartCoroutine(HandleCooldown());
            }
            else if (now.TimeOfDay >= groupBStart && now.TimeOfDay < groupBEnd) // 11:55PM-Midnight
            {
                Debug.Log("Between 11:55PM-12AM");
                SpawnPrefab("Effect1", trackedImageTransform);
                for (int i = 0; i < 5; i++) // Effect 1 will last for 30 seconds (i < x seconds)
                {
                    now = System.DateTime.Now;
                    if (now.TimeOfDay >= groupBStart && now.TimeOfDay < groupBEnd)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(1); // Checking transition every second
                }
                SpawnPrefab("Effect2", trackedImageTransform);
                for (int i = 0; i < 5; i++) // Effect 2 will last for 180 seconds (i < x seconds)
                {
                    now = System.DateTime.Now;
                    if (now.TimeOfDay >= groupBStart && now.TimeOfDay < groupBEnd)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(1); // Checking transition every second
                }
                SpawnPrefab("Effect3", trackedImageTransform);
                for (int i = 0; i < 5; i++) // Effect 3 will last for 60 seconds (i < x seconds)
                {
                    now = System.DateTime.Now;
                    if (now.TimeOfDay >= groupBStart && now.TimeOfDay < groupBEnd)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(1); // Checking transition every second
                }

                hasSpawnedAllEffects = true;
                StartCoroutine(HandleCooldown());
            }
            else if (now.TimeOfDay >= groupCStart && now.TimeOfDay < groupCEnd) // 12:00AM-12:10AM
            {
                Debug.Log("Between 12:00AM-12:10AM");
                SpawnPrefab("Effect4", trackedImageTransform);
                yield return new WaitForSeconds(600);

                hasSpawnedAllEffects = true;
                StartCoroutine(HandleCooldown());
            }

            yield return null;
        }
    }


    private IEnumerator HandleCooldown()
    {
        // Disable all currently displayed effects
        foreach (var instantiatedPrefab in _instantiatedPrefabs.Values)
        {
            instantiatedPrefab.SetActive(false);
        }

        Debug.Log("Starting cooldown");
        yield return new WaitForSeconds(5); // 10s cooldown

        // Reset the flag after the cooldown
        hasSpawnedAllEffects = false;
        Debug.Log("Cooldown finished");
    }


    private void SpawnPrefab(string experienceName, Transform trackedImageTransform)
    {
        if (ExperienceDictionary.prefabDictionary.ContainsKey(experienceName))
        {
            Debug.Log("Spawning prefab for experience: " + experienceName);

            foreach (var instantiatedPrefab in _instantiatedPrefabs.Values)
            {
                instantiatedPrefab.SetActive(false);
            }

            //GameObject prefab = ExperienceDictionary.prefabDictionary[experienceName];

            if (_instantiatedPrefabs.ContainsKey(experienceName))
            {
                GameObject instance = _instantiatedPrefabs[experienceName];
                instance.transform.position = trackedImageTransform.position;
                instance.transform.rotation = trackedImageTransform.rotation;
                instance.SetActive(true);

                // Debug.Log("Tracked image position: " + trackedImageTransform.position);
                // Debug.Log("Prefab position: " + instance.transform.position);
            }
        }
    }


}
