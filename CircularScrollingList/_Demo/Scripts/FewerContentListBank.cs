using AirFishLab.ScrollingList.ContentManagement;

namespace AirFishLab.ScrollingList.Demo
{
    public class FewerContentListBank : BaseListBank
    {
        private int[] contents = {
            1, 2, 3,
        };

        private readonly ListBank.Content _contentWrapper = new ListBank.Content();

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
