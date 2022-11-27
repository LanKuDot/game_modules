namespace AirFishLab.ScrollingList.ContentManagement
{
    /// <summary>
    /// The state of the content id
    /// </summary>
    public enum ContentIDState
    {
        /// <summary>
        /// The id is valid for getting the content
        /// </summary>
        Valid,
        /// <summary>
        /// The id is less than 0
        /// </summary>
        Underflow,
        /// <summary>
        /// The id is greater than or equals to the number of the contents
        /// </summary>
        Overflow,
        /// <summary>
        /// There has no content in the list bank
        /// </summary>
        NoContent,
    }
}
