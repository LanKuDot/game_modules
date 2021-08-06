using AirFishLab.ScrollingList.AnimationCurveUtils;
using UnityEngine;

namespace AirFishLab.ScrollingList.MovementCtrl
{
    /// <summary>
    /// Evaluate the moving distance within the given delta time
    /// according to the total moving distance
    /// </summary>
    internal class DistanceMovementCurve
    {
        /// <summary>
        /// The curve that evaluating the distance factor at the accumulated delta time
        /// </summary>
        /// The evaluated value will be multiplied by `_distanceTotal` to get the final
        /// moving distance.
        private readonly DeltaTimeCurve _distanceFactorCurve;

        /// <summary>
        /// The total moving distance in a movement
        /// </summary>
        private float _distanceTotal;
        /// <summary>
        /// The last target distance in a movement
        /// </summary>
        private float _lastDistance;

        /// <summary>
        /// The remaining moving distance in a movement
        /// </summary>
        public float distanceRemaining => _distanceTotal - _lastDistance;

        /// <summary>
        /// Create a movement curve based on the distance and factor curve
        /// </summary>
        /// <param name="factorCurve">
        /// The curve that defines the distance factor
        /// The x axis is the moving duration, and y axis is the factor value.
        /// </param>
        public DistanceMovementCurve(AnimationCurve factorCurve)
        {
            _distanceFactorCurve = new DeltaTimeCurve(factorCurve);
        }

        /// <summary>
        /// Set the moving distance for this new movement
        /// </summary>
        /// <param name="totalDistance">The total distance to be moved</param>
        public void SetMovement(float totalDistance)
        {
            _distanceFactorCurve.Reset();
            _distanceTotal = totalDistance;
            _lastDistance = 0.0f;
        }

        /// <summary>
        /// Is the movement ended?
        /// </summary>
        public bool IsMovementEnded()
        {
            return _distanceFactorCurve.IsTimeOut();
        }

        /// <summary>
        /// Force the movement to be ended
        /// </summary>
        public void EndMovement()
        {
            _distanceFactorCurve.Evaluate(_distanceFactorCurve.timeTotal);
            // Make the value of `distanceRemaining` be 0
            _lastDistance = _distanceTotal;
        }

        /// <summary>
        /// Get the moving distance in the next delta time
        /// </summary>
        /// <param name="deltaTime">The next delta time</param>
        /// <returns>The moving distance for this period</returns>
        public float GetDistance(float deltaTime)
        {
            var nextDistance = _distanceTotal * _distanceFactorCurve.Evaluate(deltaTime);
            var deltaDistance = nextDistance - _lastDistance;

            _lastDistance = nextDistance;
            return deltaDistance;
        }
    }
}
