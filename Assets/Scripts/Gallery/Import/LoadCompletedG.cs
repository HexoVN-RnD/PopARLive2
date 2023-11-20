using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadCompletedG : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI loadingText;
    [SerializeField]
    private float delay = 1f;
    [SerializeField]
    private List<GameObject> canvasesToDestroy;
    [SerializeField]
    private List<GameObject> canvasesToEnable;
    private void Start()
    {
        CMSFeedImport.OnImportCompleted += OnFeedImportComplete;
        CMSFeedLoad.OnLoadCompleted += OnFeedLoadComplete;
    }

    private void OnDestroy()
    {
        CMSFeedImport.OnImportCompleted -= OnFeedImportComplete;
        CMSFeedLoad.OnLoadCompleted -= OnFeedLoadComplete;
    }

    private void OnFeedImportComplete()
    {
        loadingText.text = "Loading images...";
    }

    private void OnFeedLoadComplete()
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
