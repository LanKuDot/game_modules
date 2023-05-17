using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The interface for controlling the box transform
    /// </summary>
    public interface IBoxTransformController
    {
        /// <summary>
        /// Set the initial local transform
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="boxID">The id of the box</param>
        void SetInitialLocalTransform(IListBox box, int boxID);

        /// <summary>
        /// Update the local transform of the box according to the moving distance
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="deltaPos">The moving distance</param>
        /// <returns>The final status of the transform position</returns>
        BoxPositionState UpdateLocalTransform(IListBox box, float deltaPos);
    }
}
