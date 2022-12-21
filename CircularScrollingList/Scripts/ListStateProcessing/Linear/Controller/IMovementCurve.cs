namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    /// <summary>
    /// The movement curve for evaluating the moving status
    /// </summary>
    internal interface IMovementCurve
    {
        /// <summary>
        /// Set the moving distance for this new movement
        /// </summary>
        /// <param name="factor">The factor for this movement</param>
        void SetMovement(float factor);

        /// <summary>
        /// Is the movement ended?
        /// </summary>
        /// <returns></returns>
        bool IsMovementEnded();

        /// <summary>
        /// End the movement forcefully
        /// </summary>
        void EndMovement();

        /// <summary>
        /// Get the moving distance in the next delta time
        /// </summary>
        /// <param name="deltaTime">The next delta time</param>
        /// <returns>The moving distance for this period</returns>
        float GetDistance(float deltaTime);
    }
}
