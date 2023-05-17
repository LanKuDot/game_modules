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
        /// The event to be invoked when the box is selected
        /// </summary>
        ListBoxSelectedEvent OnBoxSelected { get; }

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
        /// Get the transform of the box
        /// </summary>
        /// <returns>The transform of the box</returns>
        Transform GetTransform();

        /// <summary>
        /// Get the box position factor in the list
        /// </summary>
        /// <returns>The box position factor</returns>
        float GetPositionFactor();

        /// <summary>
        /// The function to be invoked when the box is moved
        /// NOTE: This function is for the future feature
        /// </summary>
        /// <param name="positionRatio">
        /// The ratio of the position in the list, which is from -1 to 1.
        /// 0 means that thw box is at the center of the list
        /// </param>
        void OnBoxMoved(float positionRatio);

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
