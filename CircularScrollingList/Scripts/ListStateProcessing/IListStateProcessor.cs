namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The interface of the processor for controlling the list behaviour
    /// </summary>
    public interface IListStateProcessor
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
        /// Get the current movement state for the boxes
        /// </summary>
        /// <param name="detailTime">The time passed in seconds in this call</param>
        void GetMovement(float detailTime);

        /// <summary>
        /// Is the movement ended?
        /// </summary>
        bool IsMovementEnded();

        /// <summary>
        /// Discard the current movement
        /// </summary>
        void EndMovement();
    }
}
