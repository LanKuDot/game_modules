namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The interface for controlling the list boxes
    /// </summary>
    public interface IListBoxController
    {
        /// <summary>
        /// Set the boxes to the controller
        /// </summary>
        /// <param name="setupData">The setup data of the list</param>
        void Initialize(ListSetupData setupData);

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
        /// Get the focusing box
        /// </summary>
        /// <returns>The current focusing box</returns>
        IListBox GetFocusingBox();
    }
}
