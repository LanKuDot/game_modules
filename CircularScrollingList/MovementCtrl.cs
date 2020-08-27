using AnimationCurveExtend;
using System;
using UnityEngine;
using PositionState = ListPositionCtrl.PositionState;

public interface IMovementCtrl
{
	void SetMovement(float baseValue, bool flag);
	bool IsMovementEnded();
	float GetDistance(float deltaTime);
}

/* Control the movement for the free movement
 *
 * There are three status of the movement:
 * - Dragging: The moving distance is the same as the dragging distance
 * - Released: When the list is released after being dragged, the moving distance
 *   is decided by the releasing velocity and a velocity factor curve
 * - Aligning: If the aligning option is set or the list reaches the end
 *   in the linear mode, the movement will switch to this status to make the list
 *   move to the desired position.
 */
public class FreeMovementCtrl : IMovementCtrl
{
	/* The movement for the free movement
	 */
	private readonly VelocityMovement _releasingMovement;
	/* The movement for aligning the list
	 */
	private readonly DistanceMovement _aligningMovement;
	/* Is the list being dragged?
	 */
	private bool _isDragging;
	/* The dragging distance
	 */
	private float _draggingDistance;
	/* Does it need to align the list after a movement?
	 */
	private readonly bool _toAlign;
	/* How far does the list exceed the end
	 */
	private float _overGoingDistance;
	/* How far could the 1ist exceed the end?
	 */
	private readonly float _overGoingDistanceThreshold;
	/* The velocity threshold that stop the list to align it
	 * It is used when `_alignMiddle` is true.
	 */
	private const float _stopVelocityThreshold = 200.0f;
	/* The function that calculating the distance to align the list
	 */
	private readonly Func<float> _getAligningDistance;
	/* The function that getting the state of the list position
	 */
	private readonly Func<PositionState> _getPositionState;

	/* Constructor
	 *
	 * @param releasingCurve The curve that defines the velocity factor for the releasing
	 *        movement. The x axis is the moving duration, and the y axis is the factor.
	 * @param toAlign Is it need to aligning after a movement?
	 * @param overGoingDistanceThreshold How far could the list exceed the end?
	 * @param getAligningDistance The function that evaluates the distance for aligning
	 * @param getPositionState The function that returns the state of the list position
	 */
	public FreeMovementCtrl(AnimationCurve releasingCurve, bool toAlign,
		float overGoingDistanceThreshold,
		Func<float> getAligningDistance, Func<PositionState> getPositionState)
	{
		_releasingMovement = new VelocityMovement(releasingCurve);
		_aligningMovement = new DistanceMovement(
			AnimationCurve.EaseInOut(0.0f, 0.0f, 0.25f, 1.0f));
		_toAlign = toAlign;
		_overGoingDistanceThreshold = overGoingDistanceThreshold;
		_getAligningDistance = getAligningDistance;
		_getPositionState = getPositionState;
	}

	/* Set the base value for this new movement
	 *
	 * @param value If `isDragging` is true, this value is the dragging distance.
	 *        Otherwise, this value is the base velocity for the releasing movement.
	 * @param isDragging Is the list being dragged?
	 */
	public void SetMovement(float value, bool isDragging)
	{
		if (isDragging) {
			_isDragging = true;
			_draggingDistance = value;

			// End the last releasing movement when start dragging
			if (!_releasingMovement.IsMovementEnded())
				_releasingMovement.EndMovement();
		} else if (_getPositionState() != PositionState.Middle) {
			_aligningMovement.SetMovement(_getAligningDistance());
		} else {
			_releasingMovement.SetMovement(value);
		}
	}

	/* Is the movement ended?
	 */
	public bool IsMovementEnded()
	{
		return !_isDragging &&
		       _aligningMovement.IsMovementEnded() &&
		       _releasingMovement.IsMovementEnded();
	}

	/* Get moving distance in the given delta time
	 */
	public float GetDistance(float deltaTime)
	{
		var distance = 0.0f;

		/* If it's dragging, return the dragging distance set from `SetMovement()` */
		if (_isDragging) {
			_isDragging = false;
			distance = _draggingDistance;

			if (IsGoingTooFar(_draggingDistance)) {
				var threshold = _overGoingDistanceThreshold * Mathf.Sign(_overGoingDistance);
				distance -= _overGoingDistance - threshold;
			}
		}
		/* Aligning */
		else if (!_aligningMovement.IsMovementEnded()) {
			distance = _aligningMovement.GetDistance(deltaTime);
		}
		/* Releasing */
		else if (!_releasingMovement.IsMovementEnded()) {
			distance = _releasingMovement.GetDistance(deltaTime);

			if (NeedToAlign(distance)) {
				// Make the releasing movement end
				_releasingMovement.EndMovement();

				// Start the aligning movement instead
				_aligningMovement.SetMovement(_getAligningDistance());
				distance = _aligningMovement.GetDistance(deltaTime);
			}
		}

		return distance;
	}

	/* Check whether it needs to switch to the aligning movement or not
	 *
	 * Return true if the list reaches the end and it exceeds the end for a distance, or
	 * if the aligning mode is on and the list moves too slow.
	 */
	private bool NeedToAlign(float distance)
	{
		return IsGoingTooFar(distance) ||
		       (_toAlign &&
		        Mathf.Abs(_releasingMovement.lastVelocity) < _stopVelocityThreshold);
	}

	private bool IsGoingTooFar(float distance)
	{
		if (_getPositionState() == PositionState.Middle)
			return false;

		_overGoingDistance = -1 * _getAligningDistance();
		return Mathf.Abs(_overGoingDistance += distance) > _overGoingDistanceThreshold;
	}
}

/* Control the movement for the unit movement
 *
 * It is controlled by the distance movement which moves for the given distance.
 * If the list reaches the end in the linear mode, it will controlled by the
 * bouncing movement which performs a back and forth movement.
 */
public class UnitMovementCtrl : IMovementCtrl
{
	/* The movement for the unit movement
	 */
	private readonly DistanceMovement _unitMovement;
	/* The movement for the bouncing movement when the list reaches the end
	 */
	private readonly DistanceMovement _bouncingMovement;
	/* The delta position for the bouncing effect
	 */
	private readonly float _bouncingDeltaPos;
	/* The function that returns the distance for aligning
	 */
	private readonly Func<float> _getAligningDistance;
	/* The function that returns the state of the list position
	 */
	private readonly Func<PositionState> _getPositionState;

	/* Constructor
	 *
	 * @param movementCurve The curve that defines the distance factor
	 *        The x axis is the moving duration, and y axis is the factor value.
	 * @param bouncingDeltaPos The delta position for bouncing effect
	 * @param getAligningDistance The function that evaluates the distance
	 *        for aligning
	 * @param getPositionState The function that returns the state of the list position
	 */
	public UnitMovementCtrl(AnimationCurve movementCurve, float bouncingDeltaPos,
		Func<float> getAligningDistance, Func<PositionState> getPositionState)
	 {
		var bouncingCurve = new AnimationCurve(
			new Keyframe(0.0f, 0.0f, 0.0f, 5.0f),
			new Keyframe(0.125f, 1.0f, 0.0f, 0.0f),
			new Keyframe(0.25f, 0.0f, -5.0f, 0.0f));

		_unitMovement = new DistanceMovement(movementCurve);
		_bouncingMovement = new DistanceMovement(bouncingCurve);
		_bouncingDeltaPos = bouncingDeltaPos;
		_getAligningDistance = getAligningDistance;
		_getPositionState = getPositionState;
	 }

	/* Set the moving distance for this new movement
	 * If there has the distance left in the last movement, the moving distance
	 * will be accumulated.
	 * If the list reaches the end in the linear mode, the moving distance
	 * will be ignored and use `_bouncingDeltaPos` for the bouncing movement.
	 */
	public void SetMovement(float distanceAdded, bool flag)
	{
		// Ignore any movement when the list is aligning
		if (!_bouncingMovement.IsMovementEnded())
			return;

		var state = _getPositionState();
		var movingDirection = Mathf.Sign(distanceAdded);

		if ((state == PositionState.Top && movingDirection < 0) ||
		    (state == PositionState.Bottom && movingDirection > 0)) {
			_bouncingMovement.SetMovement(movingDirection * _bouncingDeltaPos);
		} else {
			distanceAdded += _unitMovement.distanceRemaining;
			_unitMovement.SetMovement(distanceAdded);
		}
	}

	/* Is the movement ended?
	 */
	public bool IsMovementEnded()
	{
		return _bouncingMovement.IsMovementEnded() &&
		       _unitMovement.IsMovementEnded();
	}

	/* Get the moving distance in the given delta time
	 */
	public float GetDistance(float deltaTime)
	{
		var distance = 0.0f;

		if (!_bouncingMovement.IsMovementEnded()) {
			distance = _bouncingMovement.GetDistance(deltaTime);
		} else {
			distance = _unitMovement.GetDistance(deltaTime);

			if (NeedToAlign(distance)) {
				// Make the unit movement end
				_unitMovement.EndMovement();

				_bouncingMovement.SetMovement(-1 * _getAligningDistance());
				// Start at the furthest point to move back
				_bouncingMovement.GetDistance(0.125f);
				distance = _bouncingMovement.GetDistance(deltaTime);
			}
		}

		return distance;
	}

	/* Check whether it needs to switch to the aligning mode
	 *
	 * Return true if the list exceeds the end for a distance or the unit movement
	 * is ended.
	 */
	private bool NeedToAlign(float deltaDistance)
	{
		if (_getPositionState() == PositionState.Middle)
			return false;

		return Mathf.Abs(_getAligningDistance() * -1 + deltaDistance) > _bouncingDeltaPos ||
		        _unitMovement.IsMovementEnded();
	}
}

/* Evaluate the moving distance within the given delta time according to the velocity
 * factor curve
 */
internal class VelocityMovement
{
	/* The curve that evaluating the velocity factor at the accumulated delta time
	 * The evaluated value will be multiplied by the `_baseVelocity` to get the
	 * final velocity.
	 */
	private readonly DeltaTimeCurve _velocityFactorCurve;
	/* The referencing velocity for a movement
	 */
	private float _baseVelocity;
	/* The velocity at the last `GetDistance()` call or the last `SetMovement()` call
	 */
	public float lastVelocity { get; private set; }

	/* Constructor
	 *
	 * @param factorCurve The curve that defines the velocity factor
	 *        The x axis is the moving duration, and the y axis is the factor.
	 */
	public VelocityMovement(AnimationCurve factorCurve)
	{
		_velocityFactorCurve = new DeltaTimeCurve(factorCurve);
	}

	/* Set the base velocity for this new movement
	 */
	public void SetMovement(float baseVelocity)
	{
		_velocityFactorCurve.Reset();
		_baseVelocity = baseVelocity;
		lastVelocity = _velocityFactorCurve.CurrentEvaluate() * _baseVelocity;
	}

	/* Is the movement ended?
	 */
	public bool IsMovementEnded()
	{
		return _velocityFactorCurve.IsTimeOut();
	}

	/* Forcibly end the movement by making it time out
	 */
	public void EndMovement()
	{
		_velocityFactorCurve.Evaluate(_velocityFactorCurve.timeTotal);
	}

	/* Get moving distance in the given delta time
	 *
	 * The given delta time will be accumulated first, and then get the velocity
	 * at the accumulated time. The velocity will be multiplied by the given delta time
	 * to get the moving distance.
	 */
	public float GetDistance(float deltaTime)
	{
		lastVelocity = _velocityFactorCurve.Evaluate(deltaTime) * _baseVelocity;
		return lastVelocity * deltaTime;
	}
}

/* Evaluate the moving distance within the given delta time according to the total
 * moving distance
 */
internal class DistanceMovement
{
	/* The curve that evaluating the distance factor at the accumulated delta time
	 * The evaluated value will be multiplied by `_distanceTotal` to get the final
	 * moving distance.
	 */
	private readonly DeltaTimeCurve _distanceFactorCurve;
	/* The total moving distance in a movement
	 */
	private float _distanceTotal;
	/* The last target distance in a movement
	 */
	private float _lastDistance;
	/* The remaining moving distance in a movement
	 */
	public float distanceRemaining
	{
		get { return _distanceTotal - _lastDistance; }
	}

	/* Constructor
	 *
	 * @param factorCurve The curve that defines the distance factor
	 *        The x axis is the moving duration, and y axis is the factor value.
	 */
	public DistanceMovement(AnimationCurve factorCurve)
	{
		_distanceFactorCurve = new DeltaTimeCurve(factorCurve);
	}

	/* Set the moving distance for this new movement
	 */
	public void SetMovement(float totalDistance)
	{
		_distanceFactorCurve.Reset();
		_distanceTotal = totalDistance;
		_lastDistance = 0.0f;
	}

	public bool IsMovementEnded()
	{
		return _distanceFactorCurve.IsTimeOut();
	}

	/* Forcibly end the movement by making it time out
	 */
	public void EndMovement()
	{
		_distanceFactorCurve.Evaluate(_distanceFactorCurve.timeTotal);
		_lastDistance = _distanceTotal;
	}

	/* Get the moving distance in the given delta time
	 *
	 * The time will be accumulated first, and then get the final distance
	 * at the time accumulated, and subtract it from the passed distance
	 * to get the moving distance in the given delta time.
	 */
	public float GetDistance(float deltaTime)
	{
		var nextDistance = _distanceTotal * _distanceFactorCurve.Evaluate(deltaTime);
		var deltaDistance = nextDistance - _lastDistance;

		_lastDistance = nextDistance;
		return deltaDistance;
	}
}
