using UnityEngine;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The interface of the list box
    /// </summary>
    public interface IListBox
    {
        /// <summary>
        /// The id of the box
        /// </summary>
        int ListBoxID { get; }

        /// <summary>
        /// The id of the content that the box refers
        /// </summary>
        int ContentID { get; }

        /// <summary>
        /// The transform of the box
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Initialize the list box
        /// </summary>
        /// <param name="setupData">The setup data of the list</param>
        /// <param name="listBoxID">The id of the box</param>
        void Initialize(ListSetupData setupData, int listBoxID);

        /// <summary>
        /// Set the content for the box
        /// </summary>
        /// <param name="contentID">The content ID</param>
        /// <param name="content">
        /// The content. If the content ID is invalid, it will be null.
        /// </param>
        void SetContent(int contentID, object content);
    }
}
