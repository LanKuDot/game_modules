﻿using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class DisplayAndSelectExample : MonoBehaviour
    {
        [SerializeField]
        private CircularScrollingList _list;
        [SerializeField]
        private Text _displayText;
        [SerializeField]
        private Text _centeredContentText;

        public void DisplayCenteredContent()
        {
            var contentID = _list.GetCenteredContentID();
            var centeredContent =
                (ListBank.Content)_list.ListBank.GetListContent(contentID);
            _displayText.text = "Centered content: " + centeredContent.Value;
        }

        public void GetSelectedContentID(int selectedContentID)
        {
            var content =
                (ListBank.Content)_list.ListBank.GetListContent(selectedContentID);
            Debug.Log($"Selected content ID: {selectedContentID}, "
                      + $"Content: {content.Value}");
        }

        public void OnCenteredBoxChanged(
            ListBox prevCenteredBox, ListBox curCenteredBox)
        {
            _centeredContentText.text =
                "(Auto updated)\nCentered content: "
                + ((IntListBox) curCenteredBox).content;
        }

        public void OnMovementEnd()
        {
            Debug.Log("Movement Ends");
        }
    }
}
