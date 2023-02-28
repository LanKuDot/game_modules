using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    /// <summary>
    /// The utility for the movement calculation
    /// </summary>
    public static class MovementUtility
    {
        /// <summary>
        /// Check if the list is moving too far or not
        /// </summary>
        /// <param name="focusingState">Current focusing state</param>
        /// <param name="distanceLimit">The aligning distance limit</param>
        /// <param name="targetDistance">The target aligning distance</param>
        /// <returns>True if the list is going too far</returns>
        public static bool IsGoingToFar(
            ListFocusingState focusingState, float distanceLimit, float targetDistance)
        {
            if (focusingState == ListFocusingState.Middle)
                return false;

            if (focusingState == ListFocusingState.TopAndBottom)
                return Mathf.Abs(targetDistance) > distanceLimit;

            if ((focusingState.HasFlag(ListFocusingState.Bottom) && targetDistance < 0)
                || (focusingState.HasFlag(ListFocusingState.Top) && targetDistance > 0))
                return false;

            return Mathf.Abs(targetDistance) > distanceLimit;
        }
    }
}
