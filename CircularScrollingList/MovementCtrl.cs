using AnimationCurveExtend;
using UnityEngine;

public interface IMovementCtrl
{
	void SetMovement(float baseValue);
	bool IsMovementEnded();
	float GetDistance(float deltaTime);
}

/* Control the movement that is decided by the velocity factor curve
 */
public class VelocityMovement : IMovementCtrl
{
	/* The curve that evaluating the velocity factor at the accumulated delta time
	 * The evaluated value will be multiplied by the `_baseVelocity` to get the
	 * final velocity.
	 */
	private DeltaTimeCurve _velocityFactorCurve;
	/* The movement control for aligning the list
	 */
	private DistanceMovementCtrl _aligningMovement;
	/* Does it need to align the list after a movement?
	 */
	private bool _toAlign;
	/* Is the list aligning?
	 */
	private bool _isAligning;
	/* The referencing velocity for a movement
	 */
	private float _baseVelocity;
	/* The velocity threshold that stop the list to align it
	 * It is used when `_alignMiddle` is true.
	 */
	private const float _stopVelocityThreshold = 200.0f;

	public delegate float CalculateDistanceDelegate();
	/* The function that calculating the distance to align the list
	 */
	private CalculateDistanceDelegate _findAligningDistance;

	/* Constructor
	 *
	 * @param movementCurve The curve that defines the velocity factor
	 *        The x axis is the moving duration, and the y axis is the factor.
	 * @param aligningCurve The curve that defines the distance factor for aligning
	 *        The x axis is the aligning duration, and the y axis is the factor.
	 * @param toAlign Is it need to aligning after a movement?
	 * @param findAligningDistance The function that evaluate the distance for aligning
	 */
	public VelocityMovement(AnimationCurve movementCurve, AnimationCurve aligningCurve,
		bool toAlign, CalculateDistanceDelegate findAligningDistance)
	{
		_velocityFactorCurve = new DeltaTimeCurve(movementCurve);
		_aligningMovement = new DistanceMovementCtrl(aligningCurve);
		_toAlign = toAlign;
		_findAligningDistance = findAligningDistance;
	}

	/* Set the base velocity for this new movement
	 */
	public void SetMovement(float baseVelocity)
	{
		_velocityFactorCurve.Reset();
		_baseVelocity = baseVelocity;
	}

	/* Is the movement ended?
	 * If `_alignMiddle` is true, the aligning movement is also counted in the movement.
	 */
	public bool IsMovementEnded()
	{
		if (!_isAligning)
			return _velocityFactorCurve.IsTimeOut();

		return _aligningMovement.IsMovementEnded();
	}

	/* Get moving distance in the given delta time
	 *
	 * The given delta time will be accumulated first, and then get the velocity
	 * at the accumulated time. The velocity will be multiplied by the given delta time
	 * to get the moving distance.
	 * If `_alignMiddle` is true, the movement will switch to the aligning movement
	 * when the velocity is too slow.
	 */
	public float GetDistance(float deltaTime)
	{
		float distance;

		if (!_isAligning) {
			float velocity = _baseVelocity * _velocityFactorCurve.Evaluate(deltaTime);
			distance = velocity * deltaTime;

			if (_toAlign && Mathf.Abs(velocity) < _stopVelocityThreshold) {
				// Make the curve to be time out
				_velocityFactorCurve.Evaluate(100.0f);
				_aligningMovement.SetMovement(_findAligningDistance());
				_isAligning = true;
			}
		} else {
			distance = _aligningMovement.GetDistance(deltaTime);

			if (_aligningMovement.IsMovementEnded())
				_isAligning = false;
		}

		return distance;
	}
}

/* Control the movement that is decided by the total moving distance
 */
public class DistanceMovementCtrl : IMovementCtrl
{
	/* The curve that evaluating the distance factor at the accumulated delta time
	 * The evaluated value will be multiplied by `_distanceTotal` to get the final
	 * moving distance.
	 */
	private DeltaTimeCurve _distanceFactorCurve;
	/* The total moving distance in a movement
	 */
	private float _distanceTotal;
	/* The passed moving distance in a movement
	 */
	private float _distancePassed;

	/* Constructor
	 *
	 * @param movementCurve The curve that defines the distance factor
	 *        The x axis is the moving duration, and y axis is the factor value.
	 */
	public DistanceMovementCtrl(AnimationCurve movementCurve)
	{
		_distanceFactorCurve = new DeltaTimeCurve(movementCurve);
	}

	/* Set the moving distance for this new movement
	 * It there has the distance left in the last movement,
	 * the moving distance will be accumulated.
	 */
	public void SetMovement(float distanceAdded)
	{
		_distanceFactorCurve.Reset();
		_distanceTotal = distanceAdded + (_distanceTotal - _distancePassed);
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
		float nextDistance = _distanceTotal * _distanceFactorCurve.Evaluate(deltaTime);
		float deltaDistance = nextDistance - _distancePassed;

		_distancePassed = nextDistance;
		return deltaDistance;
	}
}
