using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class StringListBox : ListBox
    {
        [SerializeField]
        private Text _text;

        protected override void UpdateDisplayContent(object content)
        {
            var dataWrapper = (VariableStringListBank.DataWrapper) content;
            _text.text = dataWrapper.data;
        }
    }
}
