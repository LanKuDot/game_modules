using UnityEngine;

namespace AirFishLab.ScrollingList.Util
{
    /// <summary>
    /// The class for mapping the custom x range to the x axis of an animation curve
    /// </summary>
    public class RangeMappingCurve
    {
        /// <summary>
        /// The animation curve to be mapped to
        /// </summary>
        private readonly AnimationCurve _curve;
        private readonly float _curveXMin;
        private readonly float _curveXMax;
        private readonly float _customXMin;
        private readonly float _customXMax;

        /// <summary>
        /// The class for mapping the custom x range to the x axis of an animation curve
        /// </summary>
        /// <param name="curve">The target animation curve to be mapped to</param>
        /// <param name="curveXMin">The minimum x value of the curve</param>
        /// <param name="curveXMax">The maximum x value of the curve</param>
        /// <param name="customXMin">The minimum custom x value</param>
        /// <param name="customXMax">The maximum custom x value</param>
        public RangeMappingCurve(
            AnimationCurve curve,
            float curveXMin, float curveXMax,
            float customXMin, float customXMax)
        {
            _curve = curve;
            _curveXMin = curveXMin;
            _curveXMax = curveXMax;
            _customXMin = customXMin;
            _customXMax = customXMax;
        }

        /// <summary>
        /// Evaluate the y value of the animation curve according to the value
        /// in the custom range
        /// </summary>
        /// <param name="value">The value within the custom x range</param>
        /// <returns>The corresponding y value on the animation curve</returns>
        public float Evaluate(float value)
        {
            var lerpValue = Mathf.InverseLerp(_customXMin, _customXMax, value);
            return _curve.Evaluate(Mathf.Lerp(_curveXMin, _curveXMax, lerpValue));
        }
    }
}
