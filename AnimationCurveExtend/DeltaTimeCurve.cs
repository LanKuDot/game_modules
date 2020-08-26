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
		public float timeTotal { get; private set; }
		/* The passed time in this evaluation period
		 */
		private float _timePassed;

		public DeltaTimeCurve(AnimationCurve curve)
		{
			_curve = curve;
			timeTotal = _curve[_curve.length - 1].time;
			// Make the IsTimeOut() return true before the first Reset() call
			_timePassed = timeTotal + 1.0f;
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
			return _timePassed > timeTotal;
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

		/* Get the evaluated value at the current accumulated time.
		 */
		public float CurrentEvaluate()
		{
			return _curve.Evaluate(_timePassed);
		}
	}
}
