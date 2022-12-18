using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class StringStarListBox : ListBox
    {
        [SerializeField]
        private Text _text;
        [SerializeField]
        private GameObject[] _stars;

        protected override void UpdateDisplayContent(IListContent content)
        {
            var stringStarData = (StringStarData) content;
            _text.text = stringStarData.Title;
            for (var i = 0; i < _stars.Length; ++i)
                _stars[i].SetActive(i < stringStarData.NumOfStars);
        }
    }
}
