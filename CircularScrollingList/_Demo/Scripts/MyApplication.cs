using UnityEngine;
using UnityEngine.UI;

public class MyApplication : MonoBehaviour
{
    public ListPositionCtrl list;
    public Text displayText;

    public void DisplayCenteredContent()
    {
        int contentID = list.GetCenteredContentID();
        string centeredContent = list.listBank.GetListContent(contentID);
        displayText.text = "Centered content: " + centeredContent;
    }

    public void GetSelectedContentID(int selectedContentID)
    {
        Debug.Log("Selected content ID: " + selectedContentID +
                  ", Content: " + list.listBank.GetListContent(selectedContentID));
    }
}
