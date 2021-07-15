using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class ColorStrListBank : BaseListBank
    {
        [SerializeField]
        private ColorString[] _contents;

        public override object GetListContent(int index)
        {
            return _contents[index];
        }

        public override int GetListLength()
        {
            return _contents.Length;
        }
    }

    [Serializable]
    public class ColorString
    {
        public Color color;
        public string name;
    }
}
