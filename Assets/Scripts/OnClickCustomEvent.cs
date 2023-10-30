using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
    }

    public void ImageOnClick()
    {
        Debug.Log("Image button clicked. Loading scene: " + imageTitle.text);
        SceneManager.LoadScene(imageTitle.text);
    }

    public void InfoPanelBackButtonOnClick()
    {
        Debug.Log("Info panel back button clicked");
        infoPanel.SetActive(false);
    }
}
