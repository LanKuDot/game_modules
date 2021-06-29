using UnityEngine;

/// <summary>
/// The class for mapping the custom x range to the x axis of an animation curve
/// </summary>
public class CurveResolver
{
    /// <summary>
    /// The animation curve to be mapped to
    /// </summary>
    private readonly AnimationCurve _curve;
    /// <summary>
    /// The maximum value of the custom x range
    /// </summary>
    private readonly float _maxValue;
    /// <summary>
    /// The minimum value of the custom x range
    /// </summary>
    private readonly float _minValue;

    /// <summary>
    /// The class for mapping the custom x range to the x axis of an animation curve
    /// </summary>
    /// <param name="curve">The target animation curve to be mapped to</param>
    /// <param name="minValue">The minimum value of the custom x range</param>
    /// <param name="maxValue">The maximum value of the custom x range</param>
    public CurveResolver(AnimationCurve curve, float minValue, float maxValue)
    {
        _curve = curve;
        _minValue = minValue;
        _maxValue = maxValue;
    }

    /// <summary>
    /// Evaluate the y value of the animation curve according to the value
    /// in the custom range
    /// </summary>
    /// <param name="value">The value within [minValue, maxValue]</param>
    /// <returns>The corresponding y value on the animation curve</returns>
    public float Evaluate(float value)
    {
        var lerpValue = Mathf.InverseLerp(_minValue, _maxValue, value);
        return _curve.Evaluate(lerpValue);
    }
}
