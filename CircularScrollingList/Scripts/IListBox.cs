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
        /// The last list box
        /// </summary>
        IListBox LastListBox { get; }

        /// <summary>
        /// The next list box
        /// </summary>
        IListBox NextListBox { get; }

        /// <summary>
        /// The transform of the box
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Is the box activated?
        /// </summary>
        bool IsActivated { get; set; }

        /// <summary>
        /// Initialize the list box
        /// </summary>
        /// <param name="listBoxID">The id of the box</param>
        /// <param name="lastListBox">The last box</param>
        /// <param name="nextListBox">The next box</param>
        void Initialize(int listBoxID, IListBox lastListBox, IListBox nextListBox);

        /// <summary>
        /// Set the content for the box
        /// </summary>
        /// <param name="contentID">The content ID</param>
        /// <param name="content">
        /// The content. If the content ID is invalid, it will be null.
        /// </param>
        void SetContent(int contentID, object content);

        /// <summary>
        /// Pop the box to the front
        /// </summary>
        void PopToFront();

        /// <summary>
        /// Push the box to the back
        /// </summary>
        void PushToBack();
    }
}
