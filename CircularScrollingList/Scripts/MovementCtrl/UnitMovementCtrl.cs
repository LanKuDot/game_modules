using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.MovementCtrl
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
        /// The delta position for the bouncing effect
        /// </summary>
        private readonly float _bouncingDeltaPos;
        /// <summary>
        /// The function that returns the distance for aligning
        /// </summary>
        private readonly Func<float> _getAligningDistance;
        /// <summary>
        /// The function that returns the state of the list position
        /// </summary>
        private readonly Func<ListPositionCtrl.PositionState> _getPositionState;

        /// <summary>
        /// Create a movement control for the unit distance moving
        /// </summary>
        /// <param name="movementCurve">
        /// The curve that defines the distance factor.
        /// The x axis is the moving duration, and y axis is the factor value.
        /// </param>
        /// <param name="bouncingDeltaPos">
        /// The delta position for bouncing effect
        /// </param>
        /// <param name="getAligningDistance">
        /// The function that evaluates the distance for aligning
        /// </param>
        /// <param name="getPositionState">
        /// The function that returns the state of the list position
        /// </param>
        public UnitMovementCtrl(
            AnimationCurve movementCurve,
            float bouncingDeltaPos,
            Func<float> getAligningDistance,
            Func<ListPositionCtrl.PositionState> getPositionState)
        {
            var bouncingCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f, 0.0f, 5.0f),
                new Keyframe(0.125f, 1.0f, 0.0f, 0.0f),
                new Keyframe(0.25f, 0.0f, -5.0f, 0.0f));

            _unitMovementCurve = new DistanceMovementCurve(movementCurve);
            _bouncingMovementCurve = new DistanceMovementCurve(bouncingCurve);
            _bouncingDeltaPos = bouncingDeltaPos;
            _getAligningDistance = getAligningDistance;
            _getPositionState = getPositionState;
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

            var state = _getPositionState();
            var movingDirection = Mathf.Sign(distanceAdded);

            if ((state == ListPositionCtrl.PositionState.Top && movingDirection < 0) ||
                (state == ListPositionCtrl.PositionState.Bottom && movingDirection > 0)) {
                _bouncingMovementCurve.SetMovement(movingDirection * _bouncingDeltaPos);
            } else {
                distanceAdded += _unitMovementCurve.distanceRemaining;
                _unitMovementCurve.SetMovement(distanceAdded);
            }
        }

        /// <summary>
        /// Set the movement for certain distance
        /// for aligning the selected box to the center
        /// </summary>
        /// <param name="distance">The specified distance</param>
        public void SetSelectionMovement(float distance)
        {
            _unitMovementCurve.SetMovement(distance);
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

            var distance = _unitMovementCurve.GetDistance(deltaTime);

            if (!NeedToAlign(distance))
                return distance;

            // Make the unit movement end
            _unitMovementCurve.EndMovement();

            _bouncingMovementCurve.SetMovement(-1 * _getAligningDistance());
            // Start at the furthest point to move back
            _bouncingMovementCurve.GetDistance(0.125f);
            return _bouncingMovementCurve.GetDistance(deltaTime);
        }

        /// <summary>
        /// Check whether it needs to switch to the aligning mode
        /// </summary>
        /// <param name="deltaDistance">The next delta distance</param>
        /// <returns>
        /// Return true if the list exceeds the end for a distance or
        /// the unit movement is ended.
        /// </returns>
        private bool NeedToAlign(float deltaDistance)
        {
            if (_getPositionState() == ListPositionCtrl.PositionState.Middle)
                return false;

            return
                Mathf.Abs(_getAligningDistance() * -1 + deltaDistance)
                > _bouncingDeltaPos
                || _unitMovementCurve.IsMovementEnded();
        }
    }
}
