namespace AirFishLab.ScrollingList.ContentManagement
{
    /// <summary>
    /// The interface for the component holding the list contents
    /// </summary>
    public interface IListBank
    {
        /// <summary>
        /// Get the content in the bank
        /// </summary>
        /// <param name="index">The index of the content</param>
        /// <returns>The content</returns>
        IListContent GetListContent(int index);

        /// <summary>
        /// Get the number of the contents
        /// </summary>
        /// <returns>The number of the contents</returns>
        int GetContentCount();
    }
}
