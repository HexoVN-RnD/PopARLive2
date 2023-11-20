using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnFocusImageClick : MonoBehaviour, IPointerClickHandler
{
    //List of game objects to disable/enable
    [SerializeField]
    private List<GameObject> objectsToDisable;

    public void OnPointerClick(PointerEventData eventData)
    {
        //Disable/enable the objects
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }
}
