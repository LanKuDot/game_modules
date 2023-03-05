using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class StringListBox : ListBox
    {
        [SerializeField]
        private Text _text;

        protected override void UpdateDisplayContent(IListContent content)
        {
            var dataWrapper = (AdjustableStringListBank.DataWrapper) content;
            _text.text = dataWrapper.Data;
        }
    }
}
