using AirFishLab.ScrollingList.ContentManagement;

namespace AirFishLab.ScrollingList
{
    public interface IListBank
    {
        /// <summary>
        /// Get the list content
        /// </summary>
        /// <param name="index">The index of the content</param>
        /// <returns>The content</returns>
        IListContent GetListContent(int index);

        /// <summary>
        /// Get the number of the contents
        /// </summary>
        /// <returns>The number of content</returns>
        int GetContentCount();
    }
}
