using System;
using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class ColorStrListBank : BaseListBank
    {
        [SerializeField]
        private ColorString[] _contents;

        public override IListContent GetListContent(int index)
        {
            return _contents[index];
        }

        public override int GetContentCount()
        {
            return _contents.Length;
        }
    }

    [Serializable]
    public class ColorString : IListContent
    {
        public Color color;
        public string name;
    }
}
