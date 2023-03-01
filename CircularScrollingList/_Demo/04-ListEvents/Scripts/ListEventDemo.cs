﻿using UnityEngine;
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

        public void DisplayFocusingContent()
        {
            var contentID = _list.GetFocusingContentID();
            var centeredContent =
                (IntListContent)_list.ListBank.GetListContent(contentID);
            _requestedContentText.text =
                $"Focusing content: {centeredContent.Value}";
        }

        public void GetSelectedContentID(int selectedContentID)
        {
            var content =
                (IntListContent)_list.ListBank.GetListContent(selectedContentID);
            _selectedContentText.text =
                $"Selected content ID: {selectedContentID}, Content: {content.Value}";
        }

        public void OnFocusingBoxChanged(
            ListBox prevFocusingBox, ListBox curFocusingBox)
        {
            _autoUpdatedContentText.text =
                "(Auto updated)\nFocusing content: "
                + $"{((IntListBox) curFocusingBox).Content}";
        }

        public void OnMovementEnd()
        {
            Debug.Log("Movement Ends");
        }
    }
}
