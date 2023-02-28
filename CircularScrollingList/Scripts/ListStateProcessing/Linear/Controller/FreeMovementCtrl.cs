﻿using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
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
        /// The maximum delta distance for a movement
        /// </summary>
        private readonly float _maxDraggingDistance;
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
        /// The function that getting the focusing state of the list
        /// </summary>
        private readonly Func<ListFocusingState> _getFocusingStateFunc;

        #endregion

        /// <summary>
        /// Create the movement control for the free list movement
        /// </summary>
        /// <param name="releasingCurve">
        /// The curve that defines the velocity factor for the releasing movement.
        /// The x axis is the moving duration, and the y axis is the factor.
        /// </param>
        /// <param name="toAlign">Is it need to aligning after a movement?</param>
        /// <param name="maxDraggingDistance">
        /// The maximum delta distance for a movement
        /// </param>
        /// <param name="exceedingDistanceLimit">
        /// How far could the list exceed the end?
        /// </param>
        /// <param name="getAligningDistance">
        /// The function that evaluates the distance for aligning
        /// </param>
        /// <param name="getFocusingStateFunc">
        /// The function that returns the focusing state of the list
        /// </param>
        public FreeMovementCtrl(
            AnimationCurve releasingCurve,
            bool toAlign,
            float maxDraggingDistance,
            float exceedingDistanceLimit,
            Func<float> getAligningDistance,
            Func<ListFocusingState> getFocusingStateFunc)
        {
            _releasingMovementCurve = new VelocityMovementCurve(releasingCurve);
            _aligningMovementCurve =
                new DistanceMovementCurve(
                    new AnimationCurve(
                        new Keyframe(0.0f, 0.0f, 0.0f, 8.0f),
                        new Keyframe(0.25f, 1.0f, 0.0f, 0.0f)
                    ));
            _toAlign = toAlign;
            _maxDraggingDistance = maxDraggingDistance;
            _exceedingDistanceLimit = exceedingDistanceLimit;
            _getAligningDistance = getAligningDistance;
            _getFocusingStateFunc = getFocusingStateFunc;
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
            _isDragging = isDragging;

            if (isDragging) {
                _draggingDistance =
                    Mathf.Min(Mathf.Abs(value), _maxDraggingDistance) * Mathf.Sign(value);

                // End the last movement when start dragging
                _aligningMovementCurve.EndMovement();
                _releasingMovementCurve.EndMovement();
            } else if (_getFocusingStateFunc() != ListFocusingState.Middle) {
                _aligningMovementCurve.SetMovement(_getAligningDistance());
            } else {
                _releasingMovementCurve.SetMovement(value);
            }
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
            var curDistance = _getAligningDistance() * -1;
            var state = _getFocusingStateFunc();

            /* If it's dragging, return the dragging distance set from `SetMovement()` */
            if (_isDragging) {
                distance = _draggingDistance;
                // The dragging distance is only valid for one frame
                _draggingDistance = 0;

                if (!MovementUtility.IsGoingToFar(
                        state, _exceedingDistanceLimit, curDistance + distance))
                    return distance;

                var limit = _exceedingDistanceLimit * Mathf.Sign(distance);
                distance = limit - curDistance;
            }
            /* Aligning */
            else if (!_aligningMovementCurve.IsMovementEnded()) {
                distance = _aligningMovementCurve.GetDistance(deltaTime);
            }
            /* Releasing */
            else if (!_releasingMovementCurve.IsMovementEnded()) {
                distance = _releasingMovementCurve.GetDistance(deltaTime);

                if (!MovementUtility.IsGoingToFar(
                        state, _exceedingDistanceLimit, curDistance + distance)
                    && !IsTooSlow())
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
    }
}
