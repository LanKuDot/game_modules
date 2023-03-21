namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The interface of the processor for controlling the list movement
    /// </summary>
    public interface IListMovementProcessor
    {
        /// <summary>
        /// Initialize the controller
        /// </summary>
        /// <param name="setupData">The data for setting up the list</param>
        void Initialize(ListSetupData setupData);

        /// <summary>
        /// Set the movement
        /// </summary>
        /// <param name="inputInfo">The information of the input value</param>
        void SetMovement(InputInfo inputInfo);

        /// <summary>
        /// Set the movement for moving several times of units
        /// </summary>
        /// <param name="unit">The number of units to move</param>
        void SetUnitMovement(int unit);

        /// <summary>
        /// Set the movement for centering the selected box
        /// </summary>
        /// <param name="units">The number of units to move</param>
        void SetSelectionMovement(int units);

        /// <summary>
        /// Get the current movement for the boxes
        /// </summary>
        /// <param name="detailTime">The time passed in seconds in this call</param>
        /// <returns>The value for moving the boxes</returns>
        float GetMovement(float detailTime);

        /// <summary>
        /// Is the movement ended?
        /// </summary>
        bool IsMovementEnded();

        /// <summary>
        /// Whether need to align a box or not
        /// </summary>
        bool NeedToAlign();

        /// <summary>
        /// Discard the current movement
        /// </summary>
        /// <param name="toAlign">Whether to align a box</param>
        void EndMovement(bool toAlign);
    }
}
