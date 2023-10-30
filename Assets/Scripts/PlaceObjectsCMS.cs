using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Playables;

namespace JohnBui
{
    public class PlaceObjectsCMS : MonoBehaviour
    {
        public List<Texture2D> ArImages;

        [SerializeField] ARTrackedImageManager m_TrackedImageManager;

        public static PlaceObjectsCMS Instance;

        public List<GameObject> ArPrefabs;

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
                var jobHandle = mutableLibrary.ScheduleAddImageWithValidationJob(texture, texture.name, 0.1f, default);
                yield return jobHandle;  // Wait for the job to complete
            }
        }

        // Store the key of the last tracked image
        private string _lastTrackedImageKey = null;

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

                    Debug.Log("Tracked image added: " + trackedImage.referenceImage.name);

                    string key = trackedImage.referenceImage.name.Replace(".png", "");
                    if (ImportFromCMS.prefabDictionary.ContainsKey(key))
                    {
                        // If the currently tracked image is the same as the last tracked image, do nothing
                        if (_lastTrackedImageKey == key)
                        {
                            continue;
                        }

                        foreach (var instantiatedPrefab in _instantiatedPrefabs.Values)
                        {
                            instantiatedPrefab.SetActive(false);
                        }

                        // When a new image is detected, check if a prefab for that image already exists
                        if (_instantiatedPrefabs.ContainsKey(key))
                        {
                            // If it does, enable it and update its position and rotation
                            GameObject instance = _instantiatedPrefabs[key];
                            instance.transform.position = trackedImage.transform.position;
                            instance.transform.rotation = trackedImage.transform.rotation;
                            instance.SetActive(true);
                        }
                        else
                        {
                            // If it doesn't, instantiate a new prefab
                            GameObject prefab = ImportFromCMS.prefabDictionary[key];
                            if (prefab == null)
                            {
                                Debug.LogError("Prefab is null for key: " + key);
                                continue;
                            }

                            GameObject instance = Instantiate(prefab, trackedImage.transform.position, trackedImage.transform.rotation);
                            instance.transform.parent = trackedImage.transform;

                            _instantiatedPrefabs.Add(key, instance);
                        }

                        _lastTrackedImageKey = key;
                    }
                }
            }
        }
    }
}