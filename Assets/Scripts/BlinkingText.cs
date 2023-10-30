using System.Collections;
using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float speed = 1f;

    private void Start()
    {
        StartCoroutine(BlinkText());
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            // Fade out
            while (text.color.a > 0.01f)
            {
                Color color = text.color;
                color.a = Mathf.Lerp(color.a, 0f, speed * Time.deltaTime);
                text.color = color;
                yield return null;
            }

            // Fully transparent
            Color transparent = text.color;
            transparent.a = 0f;
            text.color = transparent;

            yield return new WaitForSeconds(0.1f);

            // Fade in
            while (text.color.a < 0.99f)
            {
                Color color = text.color;
                color.a = Mathf.Lerp(color.a, 1f, speed * Time.deltaTime);
                text.color = color;
                yield return null;
            }

            // Fully opaque
            Color opaque = text.color;
            opaque.a = 1f;
            text.color = opaque;

            yield return new WaitForSeconds(0.1f);
        }
    }
}
