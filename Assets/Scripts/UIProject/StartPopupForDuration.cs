using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class StartPopupForDuration : MonoBehaviour
{
    [SerializeField] 
    private List<GameObject> objectsToActivate;
    [SerializeField]
    private float duration = 5f;

    void Start()
    {
        foreach (GameObject objectToActivate in objectsToActivate)
        {
            CanvasGroup canvasGroup = objectToActivate.GetComponent<CanvasGroup>();

            objectToActivate.SetActive(true);
            canvasGroup.DOFade(1f, 0.5f).SetDelay(1f).OnComplete(() =>
            {
                canvasGroup.DOFade(0f, 0.5f).SetDelay(duration).OnComplete(() =>
                {
                    objectToActivate.SetActive(false);
                });
            });
        }
    }
}
