using UnityEngine;

namespace AnimationCurveExtend
{
/* The curve for evaluating the value by providing the delta time.
 * It has a counter to accumulate the time passed since a new evaluation period.
 */
	public class DeltaTimeCurve
	{
		/* The curve for evaluating the value
		 * The x axis is the evaluation time period, which starts from 0.
		 * The y axis is the value to be evaluated.
		 */
		private AnimationCurve _curve;
		/* The total time period of the curve
		 * It is the time of the last KeyFrame of the `_curve`.
		 */
		private float _timeTotal;
		/* The passed time in this evaluation period
		 */
		private float _timePassed;

		public DeltaTimeCurve(AnimationCurve curve)
		{
			_curve = curve;
			_timeTotal = _curve[_curve.length - 1].time;
		}

		/* Reset the time counter to start a new evaluation period
		 */
		public void Reset()
		{
			_timePassed = 0.0f;
		}

		/* Does the time counter exceed the time interval defined by the curve?
		 */
		public bool IsTimeOut()
		{
			return _timePassed > _timeTotal;
		}

		/* Evaluate the value by providing the delta time in this evaluation period
		 * The time counter will be accumulated first, and then evaluate the final
		 * value from the curve.
		 */
		public float Evaluate(float deltaTime)
		{
			_timePassed += deltaTime;
			return _curve.Evaluate(_timePassed);
		}
	}

	public interface IMovement
	{
		void SetMovement(float baseValue);
		bool IsMovementEnded();
		float GetDistance(float deltaTime);
	}

	/* Get the moving distance per frame in a movement,
	 * which is calculated by the base velocity and the velocity factor at this frame.
	 */
	public class VelocityMovement : IMovement
	{
		/* The curve that defines the velocity factor in a movement
		 */
		private DeltaTimeCurve _velocityFactorCurve;
		/* The base velocity to be multiplied by the velocity factor in a movement
		 */
		private float _baseVelocity;

		public VelocityMovement(AnimationCurve velocityFactorCurve)
		{
			_velocityFactorCurve = new DeltaTimeCurve(velocityFactorCurve);
		}

		/* Set the base velocity for this new movement
		 * The accumulated time will be reset.
		 */
		public void SetMovement(float baseVelocity)
		{
			_velocityFactorCurve.Reset();
			_baseVelocity = baseVelocity;
		}

		public bool IsMovementEnded()
		{
			return _velocityFactorCurve.IsTimeOut();
		}

		/* Get the distance in this time period. The given delta time will be accumulated first.
		 * The base velocity is multiplied by the velocity factor at the time accumulated
		 * and then it is multiplied by the delta time to get the distance result.
		 */
		public float GetDistance(float deltaTime)
		{
			return _velocityFactorCurve.Evaluate(deltaTime) * _baseVelocity * deltaTime;
		}
	}

	/* Get the moving distance per frame in a movement,
	 * which makes the result distance since this new movement to be the total distance
	 * in this movement multiplied by the distance factor at this frame.
	 */
	public class DistanceMovement : IMovement
	{
		/* The curve that defines the distance factor in a movement
		 */
		private DeltaTimeCurve _distanceFactorCurve;
		/* The total moving distance in a movement
		 */
		private float _distanceTotal;
		/* The distance passed in a movement
		 */
		private float _distancePassed;

		public DistanceMovement(AnimationCurve distanceFactorCurve)
		{
			_distanceFactorCurve = new DeltaTimeCurve(distanceFactorCurve);
		}

		/* Set the total moving distance for this new movement
		 * The accumulated distance will be reset.
		 */
		public void SetMovement(float distanceTotal)
		{
			_distanceFactorCurve.Reset();
			_distanceTotal = distanceTotal;
			_distancePassed = 0.0f;
		}

		public bool IsMovementEnded()
		{
			return _distanceFactorCurve.IsTimeOut();
		}

		/* Get the moving distance for the given delta time. The time will be accumulated first.
		 * It will get the final distance at the time accumulated, and subtract it
		 * from the passed distance to get the moving distance in the given delta time.
		 */
		public float GetDistance(float deltaTime)
		{
			float nextDistance = _distanceFactorCurve.Evaluate(deltaTime) * _distanceTotal;
			float deltaDistance = nextDistance - _distancePassed;

			_distancePassed = nextDistance;
			return deltaDistance;
		}
	}
}
