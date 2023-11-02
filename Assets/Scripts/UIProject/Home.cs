using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Home : MonoBehaviour
{
    public void OnClickHome()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ProjectSelection");
    }
}
