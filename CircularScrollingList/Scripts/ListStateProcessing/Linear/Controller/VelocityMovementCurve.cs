using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    /// <summary>
    /// Evaluate the moving distance within the given delta time
    /// according to the velocity factor curve
    /// </summary>
    internal class VelocityMovementCurve : IMovementCurve
    {
        /// <summary>
        /// The curve that evaluating the velocity factor at the accumulated delta time
        /// </summary>
        /// The evaluated value will be multiplied by the `_baseVelocity` to get the
        /// final velocity.
        private readonly DeltaTimeCurve _velocityFactorCurve;

        /// <summary>
        /// The referencing velocity for a movement
        /// </summary>
        private float _baseVelocity;

        /// <summary>
        /// The velocity at the last `GetDistance()` call or the last `SetMovement()` call
        /// </summary>
        public float lastVelocity { get; private set; }

        /// <summary>
        /// Create a velocity curve based movement curve
        /// </summary>
        /// <param name="factorCurve">
        /// The curve that defines the velocity factor
        /// The x axis is the moving duration, and the y axis is the factor.
        /// </param>
        public VelocityMovementCurve(AnimationCurve factorCurve)
        {
            _velocityFactorCurve = new DeltaTimeCurve(factorCurve);
        }

        #region IMovementCurve

        /// <summary>
        /// Set the base velocity for this new movement
        /// </summary>
        public void SetMovement(float baseVelocity)
        {
            _velocityFactorCurve.Reset();
            _baseVelocity = baseVelocity;
            lastVelocity = _velocityFactorCurve.CurrentEvaluate() * _baseVelocity;
        }

        public bool IsMovementEnded()
        {
            return _velocityFactorCurve.IsTimeOut();
        }

        public void EndMovement()
        {
            _velocityFactorCurve.Evaluate(_velocityFactorCurve.TotalTime);
        }

        public float GetDistance(float deltaTime)
        {
            lastVelocity = _velocityFactorCurve.Evaluate(deltaTime) * _baseVelocity;
            return lastVelocity * deltaTime;
        }

        #endregion
    }
}
