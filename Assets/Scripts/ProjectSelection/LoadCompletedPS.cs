using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadCompletedPS : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI loadingText;
    [SerializeField]
    private float delay = 2f;
    [SerializeField]
    private List<GameObject> canvasesToDestroy;
    [SerializeField]
    private List<GameObject> canvasesToEnable;
    private void Start()
    {
        CMSProjectImageImport.OnDataDownloaded += OnDataDownloaded;
        CMSProjectImageLoad.OnImagesDisplayed += OnImagesDisplayed;
    }

    private void OnDestroy()
    {
        CMSProjectImageImport.OnDataDownloaded -= OnDataDownloaded;
        CMSProjectImageLoad.OnImagesDisplayed -= OnImagesDisplayed;
    }

    private void OnDataDownloaded()
    {
        loadingText.text = "Loading projects...";
    }

    private void OnImagesDisplayed()
    {
        loadingText.text = "Loading completed!";
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
