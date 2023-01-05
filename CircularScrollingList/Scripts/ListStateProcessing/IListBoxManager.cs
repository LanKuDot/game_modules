using AirFishLab.ScrollingList.ContentManagement;

namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The interface for managing the list boxes
    /// </summary>
    public interface IListBoxManager
    {
        /// <summary>
        /// Set the boxes to the manager
        /// </summary>
        /// <param name="setupData">The setup data of the list</param>
        /// <param name="contentProvider">
        /// The component for getting the list content
        /// </param>
        void Initialize(ListSetupData setupData, ListContentProvider contentProvider);

        /// <summary>
        /// Update the state of the boxes
        /// </summary>
        /// <param name="movementValue">The value for moving the boxes</param>
        void UpdateBoxes(float movementValue);

        /// <summary>
        /// Make the boxes recalculate their content ID and reacquire the content
        /// </summary>
        /// <param name="centeredContentID">
        /// The new centered content ID.
        /// If it is negative, it will take current centered content ID. <para />
        /// If current centered content ID is int.MinValue, it will be 0. <para />
        /// If current centered content ID is larger than the number of contents,
        /// it will be the ID of the last content.
        /// </param>
        void RefreshBoxes(int centeredContentID = -1);

        /// <summary>
        /// Get the box which is closet to the center
        /// </summary>
        /// <returns>The box which is closet to the center</returns>
        IListBox GetCenteredBox();
    }
}
