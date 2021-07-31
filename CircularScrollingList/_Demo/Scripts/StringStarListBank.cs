using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class StringStarListBank : BaseListBank
    {
        [SerializeField]
        private StringStarData[] _datas;

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
    public class StringStarData
    {
        [SerializeField]
        private string _title;
        [SerializeField]
        private int _numOfStars;

        public string title => _title;
        public int numOfStars => _numOfStars;
    }
}
