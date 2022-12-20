using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// Store the contents for the list boxes to display
    /// </summary>
    public abstract class BaseListBank : MonoBehaviour, IListBank
    {
        public abstract IListContent GetListContent(int index);
        public abstract int GetContentCount();
    }

/* The example of the ListBank
 */
    public class ListBank : BaseListBank
    {
        private int[] contents = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };

        private readonly Content _contentWrapper = new Content();

        public override IListContent GetListContent(int index)
        {
            _contentWrapper.Value = contents[index];
            return _contentWrapper;
        }

        public override int GetContentCount()
        {
            return contents.Length;
        }

        public class Content : IListContent
        {
            public int Value;
        }
    }
}
