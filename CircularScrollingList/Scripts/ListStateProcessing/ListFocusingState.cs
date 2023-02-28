using System;

namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The state of the list
    /// </summary>
    [Flags]
    public enum ListFocusingState
    {
        None = 0,
        /// <summary>
        /// The list reaches the top
        /// </summary>
        Top = 1 << 0,
        /// <summary>
        /// The list doesn't reach either end
        /// </summary>
        Middle = 1 << 1,
        /// <summary>
        /// The list reaches the bottom
        /// </summary>
        Bottom = 1 << 2,
        /// <summary>
        /// The list is showing the top and the bottom at the same time
        /// </summary>
        TopAndBottom = Top | Bottom
    }
}
