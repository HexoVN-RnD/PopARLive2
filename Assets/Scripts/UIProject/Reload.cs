using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement;

public class Reload : MonoBehaviour
{
    public void OnClickReload()
    {
        // ARSession arSession = FindObjectOfType<ARSession>();
        // if (arSession != null)
        // {
        //     arSession.Reset();
        //     Debug.Log("ARSession reset");
        // }
        // var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        // xrManagerSettings.DeinitializeLoader();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name, LoadSceneMode.Single);
        // xrManagerSettings.InitializeLoaderSync();
    }
}
