using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject portraitCanvas;
    [SerializeField]
    private GameObject landscapeCanvas;
    // [SerializeField]
    // private GameObject portraitController;
    // [SerializeField]
    // private GameObject landscapeController;

    void Update()
    {
        if (Screen.width > Screen.height)
        {
            // Landscape mode
            portraitCanvas.SetActive(false);
            landscapeCanvas.SetActive(true);
            //portraitController.SetActive(false);
            //landscapeController.SetActive(true);
        }
        else
        {
            // Portrait mode
            portraitCanvas.SetActive(true);
            landscapeCanvas.SetActive(false);
            //portraitController.SetActive(true);
            //landscapeController.SetActive(false);
        }
    }
}
