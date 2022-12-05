﻿using AirFishLab.ScrollingList.ContentManagement;

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
        void Initialize(ListSetupData setupData, IListContentProvider contentProvider);

        /// <summary>
        /// Update the state of the boxes
        /// </summary>
        /// <param name="movementValue">The value for moving the boxes</param>
        void UpdateBoxes(float movementValue);

        /// <summary>
        /// Get the box which is closet to the center
        /// </summary>
        /// <returns>The box which is closet to the center</returns>
        IListBox GetCenteredBox();
    }
}
