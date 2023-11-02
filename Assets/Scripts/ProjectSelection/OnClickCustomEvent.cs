using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class OnClickCustomEvent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI imageTitle;
    [SerializeField]
    private GameObject infoPanel;
    [SerializeField]
    private GameObject infoPanelBackButton;

    public void GalleryOnClick()
    {
        Debug.Log("Gallery button clicked");
    }

    public void InfoOnClick()
    {
        Debug.Log("Info button clicked");
        infoPanel.SetActive(true);
        StartCoroutine(HandleBackButton());
    }

    public void ImageOnClick()
    {
        Debug.Log("Image button clicked. Loading project: " + imageTitle.text);
        PlayerPrefs.SetInt("ProjectID", PlayerPrefs.GetInt(imageTitle.text, 0));
        SceneManager.LoadScene("GeneralARProject");
    }

    public void InfoPanelBackButtonOnClick()
    {
        Debug.Log("Info panel back button clicked");
        infoPanel.SetActive(false);
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
