using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.ListStateProcessing;
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
        /// <param name="transformController">
        /// The component for controlling the box transform
        /// </param>
        /// <param name="listBoxID">The id of the box</param>
        /// <param name="lastListBox">The last box</param>
        /// <param name="nextListBox">The next box</param>
        void Initialize(
            CircularScrollingList scrollingList,
            IBoxTransformController transformController,
            int listBoxID, IListBox lastListBox, IListBox nextListBox);

        /// <summary>
        /// Update the transform of the box
        /// </summary>
        /// <param name="deltaPos">The delta moving position</param>
        /// <returns>The final position state of the box</returns>
        BoxPositionState UpdateTransform(float deltaPos);

        /// <summary>
        /// Get the box position in the list
        /// </summary>
        /// <returns>The box position</returns>
        Vector3 GetPosition();

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

#if UNITY_EDITOR
        /// <summary>
        /// Get the transform of the box.
        /// This is used for previewing the box layout in the editor
        /// </summary>
        /// <returns>The transform of the box</returns>
        Transform GetTransform();
#endif
    }
}
