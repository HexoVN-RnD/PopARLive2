using System.Collections;
using UnityEngine;

public class SwitchCanvas : MonoBehaviour
{
    public GameObject canvasToDestroy;
    public GameObject canvasToEnable;
    public float delay = 5f;

    private void Start()
    {
        StartCoroutine(SwitchAfterDelay());
    }

    IEnumerator SwitchAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        // Destroy the current canvas
        Destroy(canvasToDestroy);

        // Enable the other canvas
        canvasToEnable.SetActive(true);
    }
}