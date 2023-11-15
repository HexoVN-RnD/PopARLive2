using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using SimpleJSON;
using UnityEngine.UI;

public class TextImport : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;    
    public Image backGround;
    public string tutorialName = "TopTutorial";

    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://popar-backend.acstech.vn/api/v3/tutorial-message/");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Parse the JSON response
            var N = JSON.Parse(www.downloadHandler.text);

            // Loop through the data array
            for (int i = 0; i < N["data"].Count; i++)
            {
                if (N["data"][i]["name"].Value == tutorialName)
                {
                    string message_text = N["data"][i]["items"][0]["message_text"];
                    string text_color = N["data"][i]["items"][0]["text_color"];
                    string bg_color = N["data"][i]["items"][0]["bg_color"];
                    int size = N["data"][i]["items"][0]["size"].AsInt;
                    string style = N["data"][i]["items"][0]["style"];

                    //Debug.Log(message_text);

                    textMeshPro.text = message_text;

                    Color color;
                    if (ColorUtility.TryParseHtmlString(text_color, out color))
                        textMeshPro.color = color;
                    else
                        textMeshPro.color = Color.black;

                    Color bgColor;
                    if (ColorUtility.TryParseHtmlString(bg_color, out bgColor))
                        backGround.color = bgColor;
                    else
                        backGround.color = Color.white;

                    textMeshPro.fontSize = size;

                    if (style == "bold")
                        textMeshPro.fontStyle = FontStyles.Bold;  
                    else if (style == "italic")
                        textMeshPro.fontStyle = FontStyles.Italic;  
                    else if (style == "underline")
                        textMeshPro.fontStyle = FontStyles.Underline; 
                    else
                        textMeshPro.fontStyle = FontStyles.Normal;

                }
            }
        }
    }
}
