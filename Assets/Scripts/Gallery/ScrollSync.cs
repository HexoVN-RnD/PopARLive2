using UnityEngine;
using UnityEngine.UI;

public class ScrollSync : MonoBehaviour
{
    public Scrollbar scrollbar1;
    public Scrollbar scrollbar2;

    void Start()
    {
        scrollbar1.onValueChanged.AddListener(OnScrollbar1ValueChanged);
    }

    void OnScrollbar1ValueChanged(float value)
    {
        scrollbar2.value = value;
    }
}
