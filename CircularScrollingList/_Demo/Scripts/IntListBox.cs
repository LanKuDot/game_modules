using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class IntListBox : ListBox
    {
        [SerializeField]
        private Text _contentText;

        public int content { get; private set; }

        protected override void UpdateDisplayContent(object content)
        {
            this.content = (int) content;
            _contentText.text = this.content.ToString();
        }
    }
}
