using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class OnClickHyperlink : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private List<TextMeshProUGUI> textMeshProObjects;

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach (TextMeshProUGUI textMeshPro in textMeshProObjects)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
                Debug.Log("Link ID: " + linkInfo.GetLinkID());
                Application.OpenURL(linkInfo.GetLinkID());
                break;
            }
        }
    }
}
