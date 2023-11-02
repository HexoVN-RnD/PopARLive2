using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCanvas : MonoBehaviour
{
    public List<GameObject> canvasesToDestroy;
    public List<GameObject> canvasesToEnable;
    public float delay = 5f;

    private void Start()
    {
        StartCoroutine(SwitchAfterDelay());
    }

    IEnumerator SwitchAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        // Destroy the current canvases
        foreach (var canvas in canvasesToDestroy)
        {
            Destroy(canvas);
        }

        // Enable the other canvases
        foreach (var canvas in canvasesToEnable)
        {
            canvas.SetActive(true);
        }
    }
}
