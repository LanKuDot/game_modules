using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    /// <summary>
    /// Control the movement for the unit movement
    /// </summary>
    /// It is evaluated by the distance movement which moves for the given distance.
    /// If the list reaches the end in the linear mode, it will controlled by the
    /// bouncing movement which performs a back and forth movement.
    public class UnitMovementCtrl : IMovementCtrl
    {
        /// <summary>
        /// The curve for evaluating the unit movement
        /// </summary>
        private readonly DistanceMovementCurve _unitMovementCurve;
        /// <summary>
        /// The curve for bouncing off to the exceeding limit
        /// </summary>
        private readonly DistanceMovementCurve _bouncingOffCurve;
        /// <summary>
        /// The curve for bouncing back to the aligned position
        /// </summary>
        private readonly DistanceMovementCurve _bouncingBackCurve;
        /// <summary>
        /// The time in seconds for the bouncing curve
        /// </summary>
        private const float BOUNCING_INTERVAL = 0.125f;
        /// <summary>
        /// How far could the 1ist exceed the end?
        /// </summary>
        private readonly float _exceedingDistanceLimit;
        /// <summary>
        /// The function that returns the focusing distance offset
        /// </summary>
        private readonly Func<float> _getFocusingDistanceOffset;
        /// <summary>
        /// The function that returns the focusing state of the list
        /// </summary>
        private readonly Func<ListFocusingState> _getFocusingStateFunc;

        /// <summary>
        /// Create a movement control for the unit distance moving
        /// </summary>
        /// <param name="movementCurve">
        /// The curve that defines the distance factor.
        /// The x axis is the moving duration, and y axis is the factor value.
        /// </param>
        /// <param name="exceedingDistanceLimit">
        /// How far could the 1ist exceed the end?
        /// </param>
        /// <param name="getFocusingDistanceOffset">
        /// The function that returns the focusing distance offset
        /// </param>
        /// <param name="getFocusingStateFunc">
        /// The function that returns the focusing state of the list
        /// </param>
        public UnitMovementCtrl(
            AnimationCurve movementCurve,
            float exceedingDistanceLimit,
            Func<float> getFocusingDistanceOffset,
            Func<ListFocusingState> getFocusingStateFunc)
        {
            var bouncingOff = new AnimationCurve(
                new Keyframe(0.0f, 0.0f, 0.0f, 5.0f),
                new Keyframe(BOUNCING_INTERVAL, 1.0f, 0.0f, 0.0f));
            var bouncingBack = new AnimationCurve(
                new Keyframe(0.0f, 0.0f, 0.0f, 0.0f),
                new Keyframe(BOUNCING_INTERVAL, 1.0f, -5.0f, 0.0f));

            _unitMovementCurve = new DistanceMovementCurve(movementCurve);
            _bouncingOffCurve = new DistanceMovementCurve(bouncingOff);
            _bouncingBackCurve = new DistanceMovementCurve(bouncingBack);
            _exceedingDistanceLimit = exceedingDistanceLimit;
            _getFocusingDistanceOffset = getFocusingDistanceOffset;
            _getFocusingStateFunc = getFocusingStateFunc;
        }

        /// <summary>
        /// Set the moving distance for this new movement
        /// </summary>
        /// If there has the distance left in the last movement,
        /// the moving distance will be accumulated.<para/>
        /// If the list reaches the end in the linear mode, the moving distance
        /// will be ignored and use `_bouncingDeltaPos` for the bouncing movement.
        /// <param name="distanceAdded">Set the additional moving distance</param>
        /// <param name="flag">No usage</param>
        public void SetMovement(float distanceAdded, bool flag)
        {
            // Ignore any movement when the list is aligning
            if (!_bouncingOffCurve.IsMovementEnded()
                || !_bouncingBackCurve.IsMovementEnded())
                return;

            var curDistance = _getFocusingDistanceOffset();
            var state = _getFocusingStateFunc();
            var movingDirection = Mathf.Sign(distanceAdded);

            if ((state.HasFlag(ListFocusingState.Top) && movingDirection < 0)
                || (state.HasFlag(ListFocusingState.Bottom) && movingDirection > 0)) {
                _bouncingOffCurve.SetMovement(
                    movingDirection * _exceedingDistanceLimit - curDistance);
                _unitMovementCurve.EndMovement();
            } else {
                var distanceRemaining = _unitMovementCurve.distanceRemaining;
                // If the moving direction is the same, just add it
                if (!Mathf.Approximately(distanceRemaining, 0f)
                    && Mathf.Approximately(
                        Mathf.Sign(distanceRemaining), Math.Sign(distanceAdded)))
                    distanceAdded += _unitMovementCurve.distanceRemaining;
                // If it is not, reverse the moving direction immediately
                else
                    distanceAdded -= curDistance;
                _unitMovementCurve.SetMovement(distanceAdded);
            }
        }

        /// <summary>
        /// Is the movement ended?
        /// </summary>
        public bool IsMovementEnded()
        {
            return _bouncingOffCurve.IsMovementEnded()
                   && _bouncingBackCurve.IsMovementEnded()
                   && _unitMovementCurve.IsMovementEnded();
        }

        /// <summary>
        /// Get the moving distance for the next delta time
        /// </summary>
        /// <param name="deltaTime">The next delta time</param>
        /// <returns>The moving distance in this period</returns>
        public float GetDistance(float deltaTime)
        {
            var curDistance = _getFocusingDistanceOffset();
            var distance = 0f;

            // ===== Bouncing Off ===== //
            if (!_bouncingOffCurve.IsMovementEnded()) {
                distance = _bouncingOffCurve.GetDistance(deltaTime);
                if (_bouncingOffCurve.IsMovementEnded())
                    _bouncingBackCurve.SetMovement(-(curDistance + distance));
                return distance;
            }

            if (!_bouncingBackCurve.IsMovementEnded())
                return _bouncingBackCurve.GetDistance(deltaTime);

            // ===== Unit Movement ===== //
            var state = _getFocusingStateFunc();
            distance = _unitMovementCurve.GetDistance(deltaTime);

            if (!MovementUtility.IsGoingToFar(
                    state, _exceedingDistanceLimit, curDistance + distance))
                return distance;

            // ===== Bouncing Back ===== //
            _unitMovementCurve.EndMovement();
            _bouncingBackCurve.SetMovement(-curDistance);
            return _bouncingBackCurve.GetDistance(deltaTime);
        }

        public void EndMovement()
        {
            _unitMovementCurve.EndMovement();
            _bouncingOffCurve.EndMovement();
            _bouncingBackCurve.EndMovement();
        }
    }
}
