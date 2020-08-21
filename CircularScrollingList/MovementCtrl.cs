using AnimationCurveExtend;
using System;
using UnityEngine;

public interface IMovementCtrl
{
	void SetMovement(float baseValue);
	bool IsMovementEnded();
	float GetDistance(float deltaTime);
}

/* Control the movement for the free movement
 *
 * It is controlled by a velocity movement which moving velocity is related to
 * the releasing velocity. If the aligning mode is on or the list reaches the end
 * in the linear mode, it will switch to the aligning movement to move
 * to the desired position.
 */
public class FreeMovementCtrl : IMovementCtrl
{
	/* The movement for the free movement
	 */
	private readonly VelocityMovement _freeMovement;
	/* The movement for aligning the list
	 */
	private readonly DistanceMovement _aligningMovement;
	/* Does it need to align the list after a movement?
	 */
	private readonly bool _toAlign;
	/* Is the list aligning?
	 */
	private bool _isAligning;
	/* How long does the list exceed the end
	 */
	private float _overGoingTime;
	/* How long could the 1ist exceed the end?
	 */
	private const float _overGoingTimeThreshold = 0.02f;
	/* The velocity threshold that stop the list to align it
	 * It is used when `_alignMiddle` is true.
	 */
	private const float _stopVelocityThreshold = 200.0f;
	/* The function that calculating the distance to align the list
	 */
	private readonly Func<float> _getAligningDistance;
	/* The function that getting the flag indicating whether the list reaches end
	 * or not
	 */
	private readonly Func<bool> _isListReachingEnd;

	/* Constructor
	 *
	 * @param movementCurve The curve that defines the velocity factor for the free
	 *        movement. The x axis is the moving duration, and the y axis is the factor.
	 * @param aligningCurve The curve that defines the distance factor for aligning
	 *        movement. The x axis is the aligning duration, and the y axis is the factor.
	 * @param toAlign Is it need to aligning after a movement?
	 * @param getAligningDistance The function that evaluate the distance for aligning
	 * @param isListReachingEnd The function that return the flag indicating
	 *        whether the list reaches end or not
	 */
	public FreeMovementCtrl(AnimationCurve movementCurve, AnimationCurve aligningCurve,
		bool toAlign, Func<float> getAligningDistance, Func<bool> isListReachingEnd)
	{
		_freeMovement = new VelocityMovement(movementCurve);
		_aligningMovement = new DistanceMovement(aligningCurve);
		_toAlign = toAlign;
		_getAligningDistance = getAligningDistance;
		_isListReachingEnd = isListReachingEnd;
	}

	/* Set the release velocity for this new movement
	 */
	public void SetMovement(float releaseVelocity)
	{
		_freeMovement.SetMovement(releaseVelocity);
		_overGoingTime = _isListReachingEnd() ? _overGoingTimeThreshold : 0.0f;
	}

	/* Is the movement ended?
	 * If `_alignMiddle` is true, the aligning movement is also counted in the movement.
	 */
	public bool IsMovementEnded()
	{
		if (!_isAligning)
			return _freeMovement.IsMovementEnded();

		return _aligningMovement.IsMovementEnded();
	}

	/* Get moving distance in the given delta time
	 */
	public float GetDistance(float deltaTime)
	{
		float distance;

		if (!_isAligning) {
			distance = _freeMovement.GetDistance(deltaTime);

			if (NeedToAlign(deltaTime)) {
				// Make the free movement end
				_freeMovement.GetDistance(100.0f);
				_aligningMovement.SetMovement(_getAligningDistance());
				_isAligning = true;

				// Start the aligning movement instead
				distance = _aligningMovement.GetDistance(deltaTime);
			}
		} else {
			distance = _aligningMovement.GetDistance(deltaTime);

			if (_aligningMovement.IsMovementEnded())
				_isAligning = false;
		}

		return distance;
	}

	/* Check whether it needs to switch to the aligning movement or not
	 *
	 * Return true if the list reaches the end and it exceeds the end for a while, or
	 * if the aligning mode is on and the list moves too slow.
	 */
	private bool NeedToAlign(float deltaTime)
	{
		return (_isListReachingEnd() &&
		        (_overGoingTime += deltaTime) > _overGoingTimeThreshold) ||
		       (_toAlign &&
		        Mathf.Abs(_freeMovement.lastVelocity) < _stopVelocityThreshold);
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
	/* Is the list bouncing?
	 */
	private bool _isBouncing = false;
	/* How far does the list exceed the end?
	 */
	private float _overGoingDistance = 0.0f;
	/* The function that returns the distance for aligning
	 */
	private readonly Func<float> _getAligningDistance;
	/* The function that returns the flag indicating whether the list reaches
	 * the end or not
	 */
	private readonly Func<bool> _isListReachingEnd;

	/* Constructor
	 *
	 * @param movementCurve The curve that defines the distance factor
	 *        The x axis is the moving duration, and y axis is the factor value.
	 * @param bouncingCurve The curve that defines the distance factor
	 *        for the bouncing effect
	 * @param bouncingDeltaPos The delta position for bouncing effect
	 * @param getAligningDistance The function that evaluates the distance
	 *        for aligning
	 * @param IsListReachingEnd The function that returns the flag
	 *        indicating whether the list reaches the end or not
	 */
	public UnitMovementCtrl(AnimationCurve movementCurve,
		AnimationCurve bouncingCurve, float bouncingDeltaPos,
		Func<float> getAligningDistance, Func<bool> isListReachingEnd)
	 {
		_unitMovement = new DistanceMovement(movementCurve);
		_bouncingMovement = new DistanceMovement(bouncingCurve);
		_bouncingDeltaPos = bouncingDeltaPos;
		_getAligningDistance = getAligningDistance;
		_isListReachingEnd = isListReachingEnd;
	}

	/* Set the moving distance for this new movement
	 * If there has the distance left in the last movement, the moving distance
	 * will be accumulated.
	 * If the list reaches the end in the linear mode, the moving distance
	 * will be ignored and use `_bouncingDeltaPos` for the bouncing movement.
	 */
	public void SetMovement(float distanceAdded)
	{
		// Ignore any movement when the list is aligning
		if (_isBouncing)
			return;

		if (_isListReachingEnd()) {
			var sign = Mathf.Sign(distanceAdded);
			_bouncingMovement.SetMovement(sign * _bouncingDeltaPos);
			_isBouncing = true;
		} else {
			distanceAdded += _unitMovement.distanceRemaining;
			_unitMovement.SetMovement(distanceAdded);
		}
	}

	public bool IsMovementEnded()
	{
		if (_isBouncing)
			return _bouncingMovement.IsMovementEnded();

		return _unitMovement.IsMovementEnded();
	}

	/* Get the moving distance in the given delta time
	 */
	public float GetDistance(float deltaTime)
	{
		var distance = 0.0f;

		if (!_isBouncing) {
			distance = _unitMovement.GetDistance(deltaTime);

			if (NeedToAlign(distance)) {
				// Make the unit movement end
				_unitMovement.GetDistance(100.0f);

				_isBouncing = true;
				_bouncingMovement.SetMovement(-1 * _getAligningDistance());
				// Start at the furthest point to move back
				_bouncingMovement.GetDistance(0.125f);
				distance = _bouncingMovement.GetDistance(deltaTime);
			}
		} else {
			distance = _bouncingMovement.GetDistance(deltaTime);

			if (_bouncingMovement.IsMovementEnded())
				_isBouncing = false;
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
		if (!_isListReachingEnd()) {
			_overGoingDistance = 0.0f;
			return false;
		}

		return (_overGoingDistance += Mathf.Abs(deltaDistance)) > _bouncingDeltaPos ||
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
