using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class SpriteStringListBox : ListBox
    {
        [SerializeField]
        private Image _image;
        [SerializeField]
        private Text _title;

        protected override void UpdateDisplayContent(IListContent content)
        {
            var data = (SpriteStringData) content;
            _image.sprite = data.sprite;
            _title.text = data.title;
        }
    }
}
