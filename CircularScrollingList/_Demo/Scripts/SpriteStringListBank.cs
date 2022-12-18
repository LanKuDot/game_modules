using System;
using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class SpriteStringListBank : BaseListBank
    {
        [SerializeField]
        private SpriteStringData[] _datas;

        public override IListContent GetListContent(int index)
        {
            return _datas[index];
        }

        public override int GetContentCount()
        {
            return _datas.Length;
        }
    }

    [Serializable]
    public class SpriteStringData : IListContent
    {
        [SerializeField]
        private Sprite _sprite;
        [SerializeField]
        private string _title;

        public Sprite sprite => _sprite;
        public string title => _title;
    }
}
