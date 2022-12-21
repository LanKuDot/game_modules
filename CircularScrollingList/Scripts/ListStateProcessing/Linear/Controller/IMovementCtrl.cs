namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
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
        /// Is this movement ended?
        /// </summary>
        bool IsMovementEnded();
        /// <summary>
        /// Get the moving distance for the next delta time
        /// </summary>
        /// <param name="deltaTime">The delta time passed</param>
        /// <returns>The moving distance during the period</returns>
        float GetDistance(float deltaTime);
        /// <summary>
        /// Discard the current movement
        /// </summary>
        void EndMovement();
    }
}
