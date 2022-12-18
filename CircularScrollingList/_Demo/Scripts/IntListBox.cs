using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class IntListBox : ListBox
    {
        [SerializeField]
        private Text _contentText;

        public int content { get; private set; }

        protected override void UpdateDisplayContent(IListContent listContent)
        {
            content = ((IntListBank.Content)listContent).Value;
            _contentText.text = content.ToString();
        }
    }
}
