using AnimationCurveExtend;
using UnityEngine;

public interface IMovementCtrl
{
	void SetMovement(float baseValue);
	bool IsMovementEnded();
	float GetDistance(float deltaTime);
}

/* Control the movement for the free movement
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

	public delegate float CalculateDistanceDelegate();
	/* The function that calculating the distance to align the list
	 */
	private CalculateDistanceDelegate _findAligningDistance;

	public delegate bool BoolValueGetterDelegate();
	/* The function that getting the value of flag _isListReachingEnd in ListPositionCtrl
	 */
	private BoolValueGetterDelegate _isListReachingEnd;

	/* Constructor
	 *
	 * @param movementCurve The curve that defines the velocity factor for the free movement
	 *        The x axis is the moving duration, and the y axis is the factor.
	 * @param aligningCurve The curve that defines the distance factor for aligning
	 *        The x axis is the aligning duration, and the y axis is the factor.
	 * @param toAlign Is it need to aligning after a movement?
	 * @param findAligningDistance The function that evaluate the distance for aligning
	 * @param isListReachingEnd The function that return the flag indicating
	 *        whether the list reaches end or not
	 */
	public FreeMovementCtrl(AnimationCurve movementCurve, AnimationCurve aligningCurve,
		bool toAlign, CalculateDistanceDelegate findAligningDistance,
		BoolValueGetterDelegate isListReachingEnd)
	{
		_freeMovement = new VelocityMovement(movementCurve);
		_aligningMovement = new DistanceMovement(aligningCurve);
		_toAlign = toAlign;
		_findAligningDistance = findAligningDistance;
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
	 *
	 * If `_alignMiddle` is true, the movement will switch to the aligning movement
	 * when the velocity is too slow.
	 */
	public float GetDistance(float deltaTime)
	{
		float distance;

		if (!_isAligning) {
			distance = _freeMovement.GetDistance(deltaTime);

			if ((_isListReachingEnd() &&
			    (_overGoingTime += deltaTime) > _overGoingTimeThreshold) ||
			    (_toAlign &&
			     Mathf.Abs(_freeMovement.lastVelocity) < _stopVelocityThreshold)) {
				// Make the free movement end
				_freeMovement.GetDistance(100.0f);
				_aligningMovement.SetMovement(_findAligningDistance());
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
}

/* Control the movement for the unit movement
 */
public class UnitMovementCtrl : IMovementCtrl
{
	/* The movement for the unit movement
	 */
	private readonly DistanceMovement _unitMovement;

	/* Constructor
	 *
	 * @param movementCurve The curve that defines the distance factor
	 *        The x axis is the moving duration, and y axis is the factor value.
	 */
	public UnitMovementCtrl(AnimationCurve movementCurve)
	{
		_unitMovement = new DistanceMovement(movementCurve);
	}

	/* Set the moving distance for this new movement
	 * It there has the distance left in the last movement,
	 * the moving distance will be accumulated.
	 */
	public void SetMovement(float distanceAdded)
	{
		distanceAdded += _unitMovement.distanceRemaining;
		_unitMovement.SetMovement(distanceAdded);
	}

	public bool IsMovementEnded()
	{
		return _unitMovement.IsMovementEnded();
	}

	/* Get the moving distance in the given delta time
	 */
	public float GetDistance(float deltaTime)
	{
		return _unitMovement.GetDistance(deltaTime);
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
	private float _lastVelocity;
	public float lastVelocity => _lastVelocity;

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
		_lastVelocity = _velocityFactorCurve.CurrentEvaluate() * _baseVelocity;
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
		_lastVelocity = _velocityFactorCurve.Evaluate(deltaTime) * _baseVelocity;
		return _lastVelocity * deltaTime;
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
	/* The passed moving distance in a movement
	 */
	private float _distancePassed;
	/* The remaining moving distance in a movement
	 */
	public float distanceRemaining => _distanceTotal - _distancePassed;

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
		_distancePassed = 0.0f;
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
		var deltaDistance = nextDistance - _distancePassed;

		_distancePassed = nextDistance;
		return deltaDistance;
	}
}
