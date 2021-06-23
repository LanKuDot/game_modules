using UnityEngine;
using UnityEngine.EventSystems;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// Manage and control the circular scrolling list
    /// </summary>
    public class CircularScrollingList : MonoBehaviour
    {
        #region Enum Definitions

        /// <summary>
        /// The type of the list
        /// </summary>
        public enum ListType
        {
            Circular,
            Linear
        };

        /// <summary>
        /// The controlling mode of the list
        /// </summary>
        public enum ControlMode
        {
            /// <summary>
            /// Control the list by the mouse pointer or finger
            /// </summary>
            Drag,
            /// <summary>
            /// Control the list by invoking functions
            /// </summary>
            Function,
            /// <summary>
            /// Control the list by the mouse wheel
            /// </summary>
            MouseWheel
        };

        /// <summary>
        /// The major moving direction of the list
        /// </summary>
        public enum Direction
        {
            Vertical,
            Horizontal
        };

        /// <summary>
        /// The state of the position of the list
        /// </summary>
        public enum PositionState
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
        };

        #endregion
    }
}
