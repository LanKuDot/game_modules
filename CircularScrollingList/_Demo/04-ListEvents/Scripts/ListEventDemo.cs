using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class ListEventDemo : MonoBehaviour
    {
        [SerializeField]
        private CircularScrollingList _list;
        [SerializeField]
        private Text _selectedContentText;
        [SerializeField]
        private Text _requestedContentText;
        [SerializeField]
        private Text _autoUpdatedContentText;

        public void DisplayCenteredContent()
        {
            var contentID = _list.GetCenteredContentID();
            var centeredContent =
                (IntListBank.Content)_list.ListBank.GetListContent(contentID);
            _requestedContentText.text =
                $"Centered content: {centeredContent.Value}";
        }

        public void GetSelectedContentID(int selectedContentID)
        {
            var content =
                (IntListBank.Content)_list.ListBank.GetListContent(selectedContentID);
            _selectedContentText.text =
                $"Selected content ID: {selectedContentID}, Content: {content.Value}";
        }

        public void OnCenteredBoxChanged(
            ListBox prevCenteredBox, ListBox curCenteredBox)
        {
            _autoUpdatedContentText.text =
                "(Auto updated)\nCentered content: "
                + $"{((IntListBox) curCenteredBox).Content}";
        }

        public void OnMovementEnd()
        {
            Debug.Log("Movement Ends");
        }
    }
}
