using AirFishLab.ScrollingList.ContentManagement;

namespace AirFishLab.ScrollingList.Demo
{
    public class FewerContentListBank : BaseListBank
    {
        private int[] contents = {
            1, 2, 3,
        };

        private readonly IntListBank.Content _contentWrapper = new IntListBank.Content();

        public override IListContent GetListContent(int index)
        {
            _contentWrapper.Value = contents[index];
            return _contentWrapper;
        }

        public override int GetContentCount()
        {
            return contents.Length;
        }
    }
}
