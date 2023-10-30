using UnityEngine;
using System.Collections;

public class ScrollbarAutoHide : MonoBehaviour
{
    [SerializeField] private float fadeInTime = 1f;
    [SerializeField] private float fadeOutTime = 1f;
    [SerializeField] private float fadeInAlpha = 1f;
    [SerializeField] private float fadeOutAlpha = 0f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HideScrollbar();
    }

    public void OnScroll(float value)
    {
        ShowScrollbar();
    }

    void ShowScrollbar()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(fadeInAlpha, fadeInTime));
        Invoke("HideScrollbar", fadeInTime);
    }

    void HideScrollbar()
    {
        StartCoroutine(Fade(fadeOutAlpha, fadeOutTime));
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
