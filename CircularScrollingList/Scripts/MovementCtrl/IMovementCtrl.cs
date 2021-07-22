namespace AirFishLab.ScrollingList.MovementCtrl
{
    /// <summary>
    /// The interface for performing a movement control
    /// </summary>
    public interface IMovementCtrl
    {
        /// <summary>
        /// Set the value for a movement action
        /// </summary>
        /// <param name="baseValue">
        /// The base value for this movement, such as velocity or distance
        /// </param>
        /// <param name="flag">The additional flag for this movement</param>
        void SetMovement(float baseValue, bool flag);
        /// <summary>
        /// Set the movement for certain distance
        /// for aligning the selected box to the center
        /// </summary>
        /// <param name="distance">The specified distance</param>
        void SetSelectionMovement(float distance);
        /// <summary>
        /// Is this movement ended?
        /// </summary>
        bool IsMovementEnded();
        /// <summary>
        /// Get the moving distance for the next delta time
        /// </summary>
        /// <param name="deltaTime">The delta time passed</param>
        /// <returns>The moving distance during the period</returns>
        float GetDistance(float deltaTime);
    }
}
