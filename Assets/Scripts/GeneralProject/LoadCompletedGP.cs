using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadCompletedGP : MonoBehaviour
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
        CMSImportAssets.OnDataDownloadStart += OnDataDownloadStart;
        CMSImportAssets.OnDataDownloadEnd += OnDataDownloadEnd;
    }

    private void OnDestroy()
    {
        CMSImportAssets.OnDataDownloadStart -= OnDataDownloadStart;
        CMSImportAssets.OnDataDownloadEnd -= OnDataDownloadEnd;
    }

    private void OnDataDownloadStart()
    {
        loadingText.text = "Downloading assets...";
    }

    private void OnDataDownloadEnd()
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
