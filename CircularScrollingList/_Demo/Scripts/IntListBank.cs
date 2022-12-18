using AirFishLab.ScrollingList.ContentManagement;

namespace AirFishLab.ScrollingList.Demo
{
    public class IntListBank : BaseListBank
    {
        private readonly int[] _contents = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };

        private readonly Content _contentWrapper = new Content();

        public override IListContent GetListContent(int index)
        {
            _contentWrapper.Value = index;
            return _contentWrapper;
        }

        public override int GetContentCount()
        {
            return _contents.Length;
        }

        public class Content : IListContent
        {
            public int Value;
        }
    }
}
