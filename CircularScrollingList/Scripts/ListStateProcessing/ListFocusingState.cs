namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The state of the list
    /// </summary>
    public enum ListFocusingState
    {
        /// <summary>
        /// The list reaches the top
        /// </summary>
        Top,
        /// <summary>
        /// The list doesn't reach either end
        /// </summary>
        Middle,
        /// <summary>
        /// The list reaches the bottom
        /// </summary>
        Bottom
    }
}
