using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigScenePS : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 120;
        Screen.orientation = ScreenOrientation.Portrait;
    }
}
