using AirFishLab.ScrollingList.ContentManagement;
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
        /// The event to be invoked when the box is clicked
        /// </summary>
        ListBoxIntEvent OnBoxClick { get; }

        /// <summary>
        /// The transform of the box
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Is the box activated?
        /// </summary>
        bool IsActivated { get; set; }

        /// <summary>
        /// The list which this box belongs to
        /// </summary>
        CircularScrollingList ScrollingList { get; }

        /// <summary>
        /// Initialize the list box
        /// </summary>
        /// <param name="scrollingList">The list which this box belongs to</param>
        /// <param name="listBoxID">The id of the box</param>
        /// <param name="lastListBox">The last box</param>
        /// <param name="nextListBox">The next box</param>
        void Initialize(
            CircularScrollingList scrollingList,
            int listBoxID, IListBox lastListBox, IListBox nextListBox);

        /// <summary>
        /// Set the content id of the box
        /// </summary>
        /// <param name="contentID">The content id</param>
        void SetContentID(int contentID);

        /// <summary>
        /// Set the content for the box
        /// </summary>
        /// <param name="content">The content</param>
        void SetContent(IListContent content);

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
