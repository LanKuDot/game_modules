namespace AirFishLab.ScrollingList.ContentManagement
{
    /// <summary>
    /// The interface for providing the list content to the boxes
    /// </summary>
    public interface IListContentProvider
    {
        /// <summary>
        /// Initialize the content provider
        /// </summary>
        /// <param name="setupData">The data for setting up the list</param>
        void Initialize(ListSetupData setupData);

        /// <summary>
        /// Get the initial content ID for the specified box
        /// </summary>
        /// <param name="listBoxID">The id of the box</param>
        /// <returns>The content ID</returns>
        int GetInitialContentID(int listBoxID);

        /// <summary>
        /// Get the content ID according to the content ID of the next box
        /// </summary>
        /// <param name="nextBoxContentID">The content ID of the next box</param>
        /// <returns>The content ID</returns>
        int GetContentIDByNextBox(int nextBoxContentID);

        /// <summary>
        /// Get the content ID according to the content ID of the last box
        /// </summary>
        /// <param name="lastBoxContentID">The content ID of the last box</param>
        /// <returns>The content ID</returns>
        int GetContentIDByLastBox(int lastBoxContentID);

        /// <summary>
        /// Try to get the content of the list
        /// </summary>
        /// <param name="contentID">The id of the content</param>
        /// <param name="content">
        /// The content. If the content is not available, it will be null.
        /// </param>
        /// <returns>It the content available?</returns>
        bool TryGetContent(int contentID, out object content);

        /// <summary>
        /// Is the content id valid for getting the list content?
        /// </summary>
        /// <param name="contentID">The content id</param>
        /// <returns>The content id is valid or not</returns>
        bool IsIDValid(int contentID);

        /// <summary>
        /// Get the shortest length for starting from one id to another id
        /// </summary>
        /// <param name="fromContentID">The starting content id</param>
        /// <param name="toContentID">The target content id</param>
        /// <returns>The shortest length</returns>
        int GetShortestLength(int fromContentID, int toContentID);
    }
}
