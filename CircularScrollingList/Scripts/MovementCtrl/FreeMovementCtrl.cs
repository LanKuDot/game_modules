using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.MovementCtrl
{
    using PositionState = ListPositionCtrl.PositionState;

    /// <summary>
    /// Control the movement for the free movement
    /// </summary>
    ///
    /// There are three statuses of the movement:<para />
    /// - Dragging: The moving distance is the same as the dragging distance<para />
    /// - Released: When the list is released after being dragged, the moving distance
    ///   is decided by the releasing velocity and a velocity factor curve<para />
    /// - Aligning: If the aligning option is set or the list reaches the end
    ///   in the linear mode, the movement will switch to this status to make the list
    ///   move to the desired position.
    public class FreeMovementCtrl : IMovementCtrl
    {
        #region Private Variables

        /// <summary>
        /// The curve for evaluating the free movement after releasing
        /// </summary>
        private readonly VelocityMovementCurve _releasingMovementCurve;
        /// <summary>
        /// The curve for evaluating the movement of aligning the list
        /// </summary>
        private readonly DistanceMovementCurve _aligningMovementCurve;
        /// <summary>
        /// Is the list being dragged?
        /// </summary>
        private bool _isDragging;
        /// <summary>
        /// The dragging distance of the list
        /// </summary>
        private float _draggingDistance;
        /// <summary>
        /// Does it need to align the list after a movement?
        /// </summary>
        private readonly bool _toAlign;
        /// <summary>
        /// How far could the 1ist exceed the end?
        /// </summary>
        private readonly float _exceedingDistanceLimit;
        /// <summary>
        /// The velocity threshold that stops the list to align it
        /// </summary>
        /// It is used when `_alignMiddle` is true.
        private const float _stopVelocityThreshold = 200.0f;
        /// <summary>
        /// The function that returning the distance to align the list
        /// </summary>
        private readonly Func<float> _getAligningDistance;
        /// <summary>
        /// The function that getting the state of the list position
        /// </summary>
        private readonly Func<PositionState> _getPositionState;

        #endregion

        /// <summary>
        /// Create the movement control for the free list movement
        /// </summary>
        /// <param name="releasingCurve">
        /// The curve that defines the velocity factor for the releasing movement.
        /// The x axis is the moving duration, and the y axis is the factor.
        /// </param>
        /// <param name="toAlign">Is it need to aligning after a movement?</param>
        /// <param name="exceedingDistanceLimit">
        /// How far could the list exceed the end?
        /// </param>
        /// <param name="getAligningDistance">
        /// The function that evaluates the distance for aligning
        /// </param>
        /// <param name="getPositionState">
        /// The function that returns the state of the list position
        /// </param>
        public FreeMovementCtrl(
            AnimationCurve releasingCurve,
            bool toAlign,
            float exceedingDistanceLimit,
            Func<float> getAligningDistance,
            Func<PositionState> getPositionState)
        {
            _releasingMovementCurve = new VelocityMovementCurve(releasingCurve);
            _aligningMovementCurve =
                new DistanceMovementCurve(
                    new AnimationCurve(
                        new Keyframe(0.0f, 0.0f, 0.0f, 8.0f),
                        new Keyframe(0.25f, 1.0f, 0.0f, 0.0f)
                    ));
            _toAlign = toAlign;
            _exceedingDistanceLimit = exceedingDistanceLimit;
            _getAligningDistance = getAligningDistance;
            _getPositionState = getPositionState;
        }

        /// <summary>
        /// Set the base value for this new movement
        /// </summary>
        /// <param name="value">
        /// If `isDragging` is true, this value is the dragging distance.
        /// Otherwise, this value is the base velocity for the releasing movement.
        /// </param>
        /// <param name="isDragging">Is the list being dragged?</param>
        public void SetMovement(float value, bool isDragging)
        {
            if (isDragging) {
                _isDragging = true;
                _draggingDistance = value;

                // End the last movement when start dragging
                _aligningMovementCurve.EndMovement();
                _releasingMovementCurve.EndMovement();
            } else if (_getPositionState() != PositionState.Middle) {
                _aligningMovementCurve.SetMovement(_getAligningDistance());
            } else {
                _releasingMovementCurve.SetMovement(value);
            }
        }

        /// <summary>
        /// Set the movement for certain distance
        /// for aligning the selected box to the center
        /// </summary>
        /// <param name="distance">The specified distance</param>
        public void SetSelectionMovement(float distance)
        {
            EndMovement();
            _aligningMovementCurve.SetMovement(distance);
        }

        /// <summary>
        /// Is the movement ended?
        /// </summary>
        public bool IsMovementEnded()
        {
            return !_isDragging &&
                   _aligningMovementCurve.IsMovementEnded() &&
                   _releasingMovementCurve.IsMovementEnded();
        }

        /// <summary>
        /// Get the moving distance for the next delta time
        /// </summary>
        public float GetDistance(float deltaTime)
        {
            var distance = 0.0f;

            /* If it's dragging, return the dragging distance set from `SetMovement()` */
            if (_isDragging) {
                _isDragging = false;
                distance = _draggingDistance;

                if (!IsGoingTooFar(distance))
                    return distance;

                var exceedingDistance = _getAligningDistance() * -1;
                var limit =
                    _exceedingDistanceLimit * Mathf.Sign(exceedingDistance);
                distance = limit - exceedingDistance;
            }
            /* Aligning */
            else if (!_aligningMovementCurve.IsMovementEnded()) {
                distance = _aligningMovementCurve.GetDistance(deltaTime);
            }
            /* Releasing */
            else if (!_releasingMovementCurve.IsMovementEnded()) {
                distance = _releasingMovementCurve.GetDistance(deltaTime);

                if (!IsGoingTooFar(distance) && !IsTooSlow())
                    return distance;

                // Make the releasing movement end
                _releasingMovementCurve.EndMovement();

                // Start the aligning movement instead
                _aligningMovementCurve.SetMovement(_getAligningDistance());
                distance = _aligningMovementCurve.GetDistance(deltaTime);
            }

            return distance;
        }

        public void EndMovement()
        {
            _isDragging = false;
            _releasingMovementCurve.EndMovement();
            _aligningMovementCurve.EndMovement();
        }

        /// <summary>
        /// Check if the movement is too slow
        /// </summary>
        /// <returns>
        /// Return true if the last movement speed is too slow
        /// when the <c>_toAlign</c> is set.
        /// </returns>
        private bool IsTooSlow()
        {
            return
                _toAlign &&
                Mathf.Abs(_releasingMovementCurve.lastVelocity) < _stopVelocityThreshold;
        }

        /// <summary>
        /// Check if the moving distance of list exceeds the over-going threshold or not
        /// </summary>
        /// <param name="nextDistance">The next moving distance</param>
        private bool IsGoingTooFar(float nextDistance)
        {
            return
                (_getPositionState() != PositionState.Middle)
                && Mathf.Abs(_getAligningDistance() * -1 + nextDistance)
                    > _exceedingDistanceLimit;
        }
    }
}
