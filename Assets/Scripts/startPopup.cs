using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class startPopup : MonoBehaviour
{
    public List<GameObject> objectsToActivate;

    void Start()
    {
        foreach (GameObject objectToActivate in objectsToActivate)
        {
            CanvasGroup canvasGroup = objectToActivate.GetComponent<CanvasGroup>();

            objectToActivate.SetActive(true);
            canvasGroup.DOFade(1f, 0.5f).SetDelay(1f).OnComplete(() =>
            {
                canvasGroup.DOFade(0f, 0.5f).SetDelay(6f).OnComplete(() =>
                {
                    objectToActivate.SetActive(false);
                });
            });
        }
    }
}
