using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;

public class MyListBox : ListBox
{
    [SerializeField]
    private Text _contentText;

    protected override void UpdateDisplayContent(object content)
    {
        _contentText.text = (string) content;
    }
}
