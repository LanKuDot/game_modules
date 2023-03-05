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
        /// <param name="focusingContentID">The new focusing content ID</param>
        void RefreshBoxes(int focusingContentID = -1);

        /// <summary>
        /// Get the focusing box
        /// </summary>
        /// <returns>The current focusing box</returns>
        IListBox GetFocusingBox();
    }
}
