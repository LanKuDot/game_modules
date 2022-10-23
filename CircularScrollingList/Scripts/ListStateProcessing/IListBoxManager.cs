using System.Collections.Generic;

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
        void Initialize(ListSetupData setupData);

        /// <summary>
        /// Initialize the layout of the boxes
        /// </summary>
        void InitializeBoxes();

        /// <summary>
        /// Update the state of the boxes
        /// </summary>
        /// <param name="movementValue">The value for moving the boxes</param>
        void UpdateBoxes(float movementValue);
    }
}
