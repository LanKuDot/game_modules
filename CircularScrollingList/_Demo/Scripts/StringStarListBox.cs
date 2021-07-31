﻿using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class StringStarListBox : ListBox
    {
        [SerializeField]
        private Text _text;
        [SerializeField]
        private GameObject[] _stars;

        protected override void UpdateDisplayContent(object content)
        {
            var stringStarData = (StringStarData) content;
            _text.text = stringStarData.title;
            for (var i = 0; i < _stars.Length; ++i)
                _stars[i].SetActive(i < stringStarData.numOfStars);
        }
    }
}
