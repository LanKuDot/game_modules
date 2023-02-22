using System;

namespace AirFishLab.ScrollingList.ContentManagement
{
    /// <summary>
    /// The state of the content id
    /// </summary>
    [Flags]
    public enum ContentIDState
    {
        /// <summary>
        /// There has no content in the list bank
        /// </summary>
        NoContent = 0,
        /// <summary>
        /// The id is valid for getting the content
        /// </summary>
        Valid = 1 << 0,
        /// <summary>
        /// The id is less than 0
        /// </summary>
        Underflow = 1 << 1,
        /// <summary>
        /// The id is greater than or equals to the number of the contents
        /// </summary>
        Overflow = 1 << 2,
        /// <summary>
        /// The id is the first content
        /// </summary>
        First = 1 << 3,
        /// <summary>
        /// The id is the last content
        /// </summary>
        Last = 1 << 4,
    }
}
