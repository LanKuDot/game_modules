using UnityEngine;

namespace AirFishLab.ScrollingList.Util
{
    /// <summary>
    /// The curve for evaluating the value by providing the delta time
    /// </summary>
    /// It has a counter to accumulate the time passed since a new evaluation period.
    public class DeltaTimeCurve
    {
        /// <summary>
        /// The curve for evaluating the value
        /// </summary>
        /// The x axis is the evaluation time period, which starts from 0.
        /// The y axis is the value to be evaluated.
        private readonly AnimationCurve _curve;
        /// <summary>
        /// The passed time in this evaluation period
        /// </summary>
        private float _timePassed;

        /// <summary>
        /// The total time period of the curve
        /// </summary>
        /// It is the time of the last KeyFrame of the `_curve`
        public readonly float TotalTime;

        public DeltaTimeCurve(AnimationCurve curve)
        {
            _curve = curve;
            TotalTime = _curve[_curve.length - 1].time;
            // Make the IsTimeOut() return true before the first Reset() call
            _timePassed = TotalTime + 1.0f;
        }

        /// <summary>
        /// Reset the time counter to start a new evaluation period
        /// </summary>
        public void Reset()
        {
            _timePassed = 0.0f;
        }

        /// <summary>
        /// Does the time counter exceed the time interval defined by the curve?
        /// </summary>
        public bool IsTimeOut()
        {
            return _timePassed > TotalTime;
        }

        /// <summary>
        /// Evaluate the value by providing the delta time in this evaluation period
        /// </summary>
        /// The time counter will be accumulated first,
        /// and then evaluate the value from the curve.
        /// <param name="deltaTime">The time passed</param>
        /// <returns>The value evaluated from the curve after the time added</returns>
        public float Evaluate(float deltaTime)
        {
            _timePassed += deltaTime;
            return _curve.Evaluate(_timePassed);
        }

        /// <summary>
        /// Get the evaluated value at the current accumulated time
        /// </summary>
        public float CurrentEvaluate()
        {
            return _curve.Evaluate(_timePassed);
        }
    }
}
