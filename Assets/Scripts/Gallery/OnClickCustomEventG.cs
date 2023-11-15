using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class OnClickCustomEventG : MonoBehaviour
{
    [SerializeField]
    private GameObject infoPanel;

    public void HomeOnClick()
    {
        Debug.Log("Home button clicked");
        SceneManager.LoadScene("ProjectSelection");
    }

    public void InfoOnClick()
    {
        Debug.Log("Info button clicked");
        infoPanel.SetActive(true);
        StartCoroutine(HandleBackButton());
    }

    public void InfoPanelBackButtonOnClick()
    {
        Debug.Log("Info panel back button clicked");
        infoPanel.SetActive(false);
    }

    public void GalleryBackButtonOnClick()
    {
        Debug.Log("Gallery back button clicked");
        SceneManager.LoadScene("ProjectSelection");
    }

    IEnumerator HandleBackButton()
    {
        while (infoPanel.activeSelf)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                InfoPanelBackButtonOnClick();
                yield break; // Stop the coroutine
            }
            yield return null; // Wait for the next frame
        }
    }
}
