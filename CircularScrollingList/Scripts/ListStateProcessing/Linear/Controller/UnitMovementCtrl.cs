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
        /// The curve for bouncing the movement when the list reaches the end
        /// </summary>
        private readonly DistanceMovementCurve _bouncingMovementCurve;
        /// <summary>
        /// How far could the 1ist exceed the end?
        /// </summary>
        private readonly float _exceedingDistanceLimit;
        /// <summary>
        /// The function that returns the distance for aligning
        /// </summary>
        private readonly Func<float> _getAligningDistance;
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
        /// <param name="getAligningDistance">
        /// The function that evaluates the distance for aligning
        /// </param>
        /// <param name="getFocusingStateFunc">
        /// The function that returns the focusing state of the list
        /// </param>
        public UnitMovementCtrl(
            AnimationCurve movementCurve,
            float exceedingDistanceLimit,
            Func<float> getAligningDistance,
            Func<ListFocusingState> getFocusingStateFunc)
        {
            var bouncingCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f, 0.0f, 5.0f),
                new Keyframe(0.125f, 1.0f, 0.0f, 0.0f),
                new Keyframe(0.25f, 0.0f, -5.0f, 0.0f));

            _unitMovementCurve = new DistanceMovementCurve(movementCurve);
            _bouncingMovementCurve = new DistanceMovementCurve(bouncingCurve);
            _exceedingDistanceLimit = exceedingDistanceLimit;
            _getAligningDistance = getAligningDistance;
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
            if (!_bouncingMovementCurve.IsMovementEnded())
                return;

            var state = _getFocusingStateFunc();
            var movingDirection = Mathf.Sign(distanceAdded);

            if ((state == ListFocusingState.Top && movingDirection < 0) ||
                (state == ListFocusingState.Bottom && movingDirection > 0)) {
                _bouncingMovementCurve.SetMovement(
                    movingDirection * _exceedingDistanceLimit);
                _unitMovementCurve.EndMovement();
            } else {
                distanceAdded += _unitMovementCurve.distanceRemaining;
                _unitMovementCurve.SetMovement(distanceAdded);
            }
        }

        /// <summary>
        /// Is the movement ended?
        /// </summary>
        public bool IsMovementEnded()
        {
            return _bouncingMovementCurve.IsMovementEnded() &&
                   _unitMovementCurve.IsMovementEnded();
        }

        /// <summary>
        /// Get the moving distance for the next delta time
        /// </summary>
        /// <param name="deltaTime">The next delta time</param>
        /// <returns>The moving distance in this period</returns>
        public float GetDistance(float deltaTime)
        {
            if (!_bouncingMovementCurve.IsMovementEnded()) {
                return _bouncingMovementCurve.GetDistance(deltaTime);
            }

            var state = _getFocusingStateFunc();
            var distance = _unitMovementCurve.GetDistance(deltaTime);
            var curDistance = _getAligningDistance() * -1;

            if (!MovementUtility.IsGoingToFar(
                    state, _exceedingDistanceLimit, curDistance + distance))
                return distance;

            // Make the unit movement end
            _unitMovementCurve.EndMovement();

            _bouncingMovementCurve.SetMovement(curDistance);
            // Start at the furthest point to move back
            _bouncingMovementCurve.GetDistance(0.125f);
            return _bouncingMovementCurve.GetDistance(deltaTime);
        }

        public void EndMovement()
        {
            _unitMovementCurve.EndMovement();
            _bouncingMovementCurve.EndMovement();
        }
    }
}
