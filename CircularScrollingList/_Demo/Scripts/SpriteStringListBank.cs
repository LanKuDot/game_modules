using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class SpriteStringListBank : BaseListBank
    {
        [SerializeField]
        private SpriteStringData[] _datas;

        public override object GetListContent(int index)
        {
            return _datas[index];
        }

        public override int GetListLength()
        {
            return _datas.Length;
        }
    }

    [Serializable]
    public class SpriteStringData
    {
        [SerializeField]
        private Sprite _sprite;
        [SerializeField]
        private string _title;

        public Sprite sprite => _sprite;
        public string title => _title;
    }
}
