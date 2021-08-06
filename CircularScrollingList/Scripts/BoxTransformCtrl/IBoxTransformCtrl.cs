using UnityEngine;

namespace AirFishLab.ScrollingList.BoxTransformCtrl
{
    /// <summary>
    /// The interface for setting the transform of a box
    /// </summary>
    public interface IBoxTransformCtrl
    {
        /// <summary>
        /// Set the initial transform for the specified box
        /// </summary>
        /// <param name="transform">The transform of the box</param>
        /// <param name="boxID">The ID of the box</param>
        /// <param name="numOfBoxes">The number of boxes</param>
        void SetInitialTransform(Transform transform, int boxID, int numOfBoxes);

        /// <summary>
        /// Set the next local transform according to the moving delta
        /// </summary>
        /// <param name="boxTransform">The transform of the box</param>
        /// <param name="delta">The delta moving distance</param>
        /// <param name="needToUpdateToLastContent">
        /// Does the box need to update to the last content?
        /// </param>
        /// <param name="needToUpdateToNextContent">
        /// Does the box need to update to the next content?
        /// </param>
        void SetLocalTransform(
            Transform boxTransform,
            float delta,
            out bool needToUpdateToLastContent,
            out bool needToUpdateToNextContent);
    }
}
