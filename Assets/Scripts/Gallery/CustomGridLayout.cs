using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public class ReadOnlyAttribute : PropertyAttribute { }
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif

#if !UNITY_EDITOR
public class ReadOnlyAttribute : Attribute { }
#endif

public class CustomGridLayout : MonoBehaviour
{
    [SerializeField]
    private CMSFeedLoad cmsFeedLoad;
    [SerializeField]
    private int columnCount = 4;
    [SerializeField, ReadOnly]
    private float cellSize;
    [SerializeField]
    private float spacing = 10f;

    private void OnEnable()
    {
        CMSFeedLoad.OnLoadCompleted += PlaceImages;
    }

    private void OnDestroy()
    {
        CMSFeedLoad.OnLoadCompleted -= PlaceImages;
    }

    private void PlaceImages()
    {
        //Calculate cell size, grabbing grid types, imageID, and calculate image position
        cellSize = (GetComponent<RectTransform>().rect.width - (columnCount + 1) * spacing) / columnCount;
        List<int> imageIDs = cmsFeedLoad.imageIDs;
        Dictionary<int, string> gridTypes = cmsFeedLoad.gridTypes;
        Dictionary<int, (int, int)> tupleGridTypes = new Dictionary<int, (int, int)>();

        foreach (var pair in gridTypes)
        {
            var parts = pair.Value.Split('*');
            tupleGridTypes.Add(pair.Key, (int.Parse(parts[0]), int.Parse(parts[1])));
        }

        Dictionary<int, int> imageDesignatedPosition = PositionMatrix.Main(columnCount, tupleGridTypes);

        //Log the image position
        foreach (var pair in imageDesignatedPosition)
        {
            Debug.Log("Image " + pair.Key + " is at position " + pair.Value);
        }

        //Grab all children gameobjects
        List<GameObject> children = new List<GameObject>();
        int counter = 0;
        foreach (Transform child in transform)
        {   
            //set anchor to top left
            children.Add(child.gameObject);
            child.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            child.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);

            //set pivot to top left
            child.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            //set size based on cell size and grid type
            int imageId = imageIDs[counter];
            if (tupleGridTypes.TryGetValue(imageId, out var gridType))
            {   //bigger size will be expand further to compensate for the spacing (2*1 will + spacing to width, 1*2 will + spacing to height, 2*2 will + spacing to both)
                child.GetComponent<RectTransform>().sizeDelta = new Vector2(gridType.Item1 * cellSize + (gridType.Item1 - 1) * spacing, gridType.Item2 * cellSize + (gridType.Item2 - 1) * spacing);
            }
            else
            {
                Debug.LogError($"Grid type not found for image ID {imageId}");
            }

            //set position based on calculated position
            if (imageDesignatedPosition.TryGetValue(imageId, out var position))
            {
                int row = (position - 1) / columnCount;
                int col = (position - 1) % columnCount;
                child.GetComponent<RectTransform>().anchoredPosition = new Vector2(col * (cellSize + spacing) + spacing, -row * (cellSize + spacing) - spacing);
            }
            else
            {
                Debug.LogError($"Position not found for image ID {imageId}");
            }
            counter++;
        }


    }
}
