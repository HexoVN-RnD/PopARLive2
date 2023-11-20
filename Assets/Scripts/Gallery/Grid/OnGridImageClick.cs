using UnityEngine;
using UnityEngine.EventSystems;

public class OnGridImageClick : MonoBehaviour, IPointerClickHandler
{
    // Define a delegate type for the event
    public delegate void FocusViewClickedHandler(int imageID);

    // Define the event
    public static event FocusViewClickedHandler OnFocusViewClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        int imageID = int.Parse(gameObject.name);
        Debug.Log("Clicked on " + imageID);
        OnFocusViewClicked?.Invoke(imageID);
    }
}