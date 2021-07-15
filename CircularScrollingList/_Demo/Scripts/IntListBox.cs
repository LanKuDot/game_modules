using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;

public class IntListBox : ListBox
{
    [SerializeField]
    private Text _contentText;

    protected override void UpdateDisplayContent(object content)
    {
        _contentText.text = ((int) content).ToString();
    }
}
