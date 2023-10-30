using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class enablePopUp : MonoBehaviour
{
    public Button yourButton; // Assign in inspector
    public List<GameObject> togglePopups; // Assign in inspector

    void Start()
    {
        yourButton.onClick.AddListener(ToggleVisibility);
    }

    void ToggleVisibility()
    {
        foreach (GameObject objectToToggle in togglePopups)
        {
            CanvasGroup canvasGroup = objectToToggle.GetComponent<CanvasGroup>();

            if (objectToToggle.activeSelf)
            {
                // If the object is active, just deactivate it
                objectToToggle.SetActive(false);
            }
            else
            {
                // If the object is not active, stop any ongoing fade out and delay
                canvasGroup.DOKill();
                // Then activate the object and make sure it's visible
                objectToToggle.SetActive(true);
                canvasGroup.alpha = 1f;
            }
        }
    }
}
