using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static CMSImportAssets;
using System.IO;
using System.Collections.Generic;

public class TutorialMessages : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMeshPro;
    [SerializeField]
    private Image background;

    [SerializeField]
    private string tutorialName;

    enum ScreenPosition
    {
        mid_hight,
        center,
        bottom
    }
    [SerializeField] 
    ScreenPosition screenPosition;

    private void OnEnable()
    {
        CMSImportAssets.OnDataDownloadEnd += DisplayMessage;
    }

    private void OnDestroy()
    {
        CMSImportAssets.OnDataDownloadEnd -= DisplayMessage;
    }

    public void DisplayMessage()
    {
        string screenPosition = this.screenPosition.ToString().Replace("_", "-");
        // Get the project ID from the player prefs
        int projectID = PlayerPrefs.GetInt("ProjectID", 0);
        // Get the path of the saved JSON file
        string path = Path.Combine(Application.persistentDataPath, "ProjectAssets", projectID.ToString(), "Data.json");

        // Read the JSON string from the file
        string jsonData = File.ReadAllText(path);

        // Parse the JSON string into a ProjectData object
        CMSImportAssets.ProjectData projectData = JsonUtility.FromJson<CMSImportAssets.ProjectData>(jsonData);

        // Get the tutorial messages from the ProjectData object
        TutorialMessage[] tutorialMessages = projectData.tutorial_message;

        // Find the first tutorial message with the specified name
        TutorialMessage tutorialMessage = System.Array.Find(tutorialMessages, message => message.name == tutorialName);

        if (tutorialMessage != null)
        {
            // Find the first message item with the specified screen position
            MessageItem messageItem = System.Array.Find(tutorialMessage.items, item => item.screen_position == screenPosition);

            if (messageItem != null)
            {
                // Display the message text
                textMeshPro.text = messageItem.message_text;

                // Parse and set the text color
                Color color;
                if (ColorUtility.TryParseHtmlString(messageItem.text_color, out color))
                    textMeshPro.color = color;
                else
                    textMeshPro.color = Color.black;

                // Parse and set the background color
                Color bgColor;
                if (ColorUtility.TryParseHtmlString(messageItem.bg_color, out bgColor))
                    background.color = bgColor;
                else
                    background.color = Color.white;

                // Set the font size
                textMeshPro.fontSize = messageItem.size;

                // Set the font style
                if (messageItem.style == "bold")
                    textMeshPro.fontStyle = FontStyles.Bold;  
                else if (messageItem.style == "italic")
                    textMeshPro.fontStyle = FontStyles.Italic;  
                else if (messageItem.style == "underline")
                    textMeshPro.fontStyle = FontStyles.Underline; 
                else
                    textMeshPro.fontStyle = FontStyles.Normal;
            }
            else
            {
                Debug.Log("No message item found with screen position: " + screenPosition);
            }
        }
        else
        {
            Debug.Log("No tutorial message found with name: " + tutorialName);
        }
    }
}
