using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;

public class DisplayAndSelectExample : MonoBehaviour
{
    [SerializeField]
    private CircularScrollingList _list;
    [SerializeField]
    private Text _displayText;

    public void DisplayCenteredContent()
    {
        var contentID = _list.GetCenteredContentID();
        var centeredContent = (int) _list.listBank.GetListContent(contentID);
        _displayText.text = "Centered content: " + centeredContent;
    }

    public void GetSelectedContentID(int selectedContentID)
    {
        Debug.Log("Selected content ID: " + selectedContentID +
                  ", Content: " + (int) _list.listBank.GetListContent(selectedContentID));
    }
}
