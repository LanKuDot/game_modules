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
        /// <param name="boxes">The boxes</param>
        void Initialize(IEnumerable<ListBox> boxes);

        /// <summary>
        /// Update the state of the boxes
        /// </summary>
        void UpdateBoxes();
    }
}
