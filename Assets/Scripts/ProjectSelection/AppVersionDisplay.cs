using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppVersionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;

    void Start()
    {
        versionText.text = "Version: " + Application.version;
    }
}
