using System;
using AirFishLab.ScrollingList.AnimationCurveUtils;
using UnityEngine;

namespace AirFishLab.ScrollingList.BoxTransformCtrl
{
    public class LinearBoxTransformCtrl : IBoxTransformCtrl
    {
        #region Position Controlling Variables
        // Position calculated here is in the local space of the list

        /// <summary>
        /// The distance between boxes
        /// </summary>
        private readonly float _unitPos;
        /// <summary>
        /// The left/down-most position of the box
        /// </summary>
        private readonly float _lowerBoundPos;
        /// <summary>
        /// The right/up-most position of the box
        /// </summary>
        private readonly float _upperBoundPos;
        /// <summary>
        /// The lower boundary where the box will be moved to the other end
        /// </summary>
        private readonly float _changeSideLowerBoundPos;
        /// <summary>
        /// The upper boundary where the box will be moved to the other end
        /// </summary>
        private readonly float _changeSideUpperBoundPos;
        /// <summary>
        /// The curve that mapping the major position to the passive position
        /// </summary>
        private readonly RangeMappingCurve _positionCurve;
        /// <summary>
        /// The curve that mapping the major position to the local scale of the box
        /// </summary>
        private readonly RangeMappingCurve _scaleCurve;

        #endregion

        /// <summary>
        /// Get the major factor from the Vector2
        /// </summary>
        private readonly Func<Vector2, float> _getMajorFactor;
        /// <summary>
        /// Get the final local position according to the major and passive position
        /// </summary>
        /// The signature is (major pos, passive pos, z pos) -> Vector3
        private readonly Func<float, float, float, Vector3> _getLocalPosition;

        /// <summary>
        /// Control the position of the box whose moving direction is in one direction
        /// </summary>
        /// <param name="positionCtrl">
        /// The component for controlling the list position
        /// </param>
        /// <param name="boxPositionCurve">
        /// The curve specifying the passive position according to the box major position
        /// </param>
        /// <param name="boxScaleCurve">
        /// The curve specifying the scale according to the box major position
        /// </param>
        /// <param name="direction">The major moving direction</param>
        public LinearBoxTransformCtrl(
            ListPositionCtrl positionCtrl,
            AnimationCurve boxPositionCurve,
            AnimationCurve boxScaleCurve,
            CircularScrollingList.Direction direction)
        {
            _unitPos = positionCtrl.unitPos;
            _lowerBoundPos = positionCtrl.lowerBoundPos;
            _upperBoundPos = positionCtrl.upperBoundPos;
            _changeSideLowerBoundPos = _lowerBoundPos + _unitPos * 0.5f;
            _changeSideUpperBoundPos = _upperBoundPos - _unitPos * 0.5f;
            _positionCurve =
                new RangeMappingCurve(
                    boxPositionCurve,
                    -1, 1,
                    _changeSideLowerBoundPos,
                    _changeSideUpperBoundPos);
            _scaleCurve =
                new RangeMappingCurve(
                    boxScaleCurve,
                    -1, 1,
                    _changeSideLowerBoundPos,
                    _changeSideUpperBoundPos);

            switch (direction) {
                case CircularScrollingList.Direction.Vertical:
                    _getMajorFactor = FactorUtility.GetVector2Y;
                    _getLocalPosition = GetLocalPositionYMajor;
                    break;
                case CircularScrollingList.Direction.Horizontal:
                    _getMajorFactor = FactorUtility.GetVector2X;
                    _getLocalPosition = GetLocalPositionXMajor;
                    break;
            }
        }

        public void SetInitialTransform(
            Transform boxTransform, int boxID, int numOfBoxes)
        {
            var majorPosition = _unitPos * (boxID * -1 + numOfBoxes / 2);

            // If there are even number of boxes, adjust the position one half unitPos down.
            if ((numOfBoxes & 0x1) == 0) {
                majorPosition =
                    _unitPos * (boxID * -1 + numOfBoxes / 2) - _unitPos / 2;
            }

            var passivePosition = GetPassivePosition(majorPosition);
            var localPosition = boxTransform.localPosition;

            var scaleValue = GetScaleValue(majorPosition);
            var localScale = boxTransform.localScale;

            boxTransform.localPosition =
                _getLocalPosition(majorPosition, passivePosition, localPosition.z);
            boxTransform.localScale =
                new Vector3(scaleValue, scaleValue, localScale.z);
        }

        public void SetLocalTransform(
            Transform boxTransform,
            float delta,
            out bool needToUpdateToLastContent,
            out bool needToUpdateToNextContent)
        {
            var localPosition = boxTransform.localPosition;
            var majorFactor = _getMajorFactor(localPosition);
            var majorPosition =
                GetMajorPosition(
                    majorFactor + delta,
                    out needToUpdateToLastContent,
                    out needToUpdateToNextContent);
            var passivePosition = GetPassivePosition(majorPosition);

            var localScale = boxTransform.localScale;
            var scaleValue = GetScaleValue(majorPosition);

            boxTransform.localPosition =
                _getLocalPosition(majorPosition, passivePosition, localPosition.z);
            boxTransform.localScale =
                new Vector3(scaleValue, scaleValue, localScale.z);
        }

        private Vector3 GetLocalPositionYMajor(float majorPos, float passivePos, float z)
        {
            return new Vector3(passivePos, majorPos, z);
        }

        private Vector3 GetLocalPositionXMajor(float majorPos, float passivePos, float z)
        {
            return new Vector3(majorPos, passivePos, z);
        }

        /// <summary>
        /// Get the major position according to the requested position
        /// If the box exceeds the boundary, one of the passed flags will be set
        /// to indicate that the content needs to be updated.
        /// </summary>
        /// <param name="positionValue">The requested position</param>
        /// <param name="needToUpdateToLastContent">
        /// Does it need to update to the last content?
        /// </param>
        /// <param name="needToUpdateToNextContent">
        /// Does it need to update to the next content?
        /// </param>
        /// <returns>The final major position</returns>
        private float GetMajorPosition(
            float positionValue,
            out bool needToUpdateToLastContent, out bool needToUpdateToNextContent)
        {
            needToUpdateToLastContent = false;
            needToUpdateToNextContent = false;

            var beyondPos = 0.0f;
            var majorPos = positionValue;

            if (positionValue < _changeSideLowerBoundPos) {
                beyondPos = positionValue - _lowerBoundPos;
                majorPos = _upperBoundPos - _unitPos + beyondPos;
                needToUpdateToLastContent = true;
            } else if (positionValue > _changeSideUpperBoundPos) {
                beyondPos = positionValue - _upperBoundPos;
                majorPos = _lowerBoundPos + _unitPos + beyondPos;
                needToUpdateToNextContent = true;
            }

            return majorPos;
        }

        /// <summary>
        /// Get the passive position according to the major position
        /// </summary>
        /// <param name="majorPosition">The major position</param>
        /// <returns>The passive position</returns>
        private float GetPassivePosition(float majorPosition)
        {
            var passivePosFactor = _positionCurve.Evaluate(majorPosition);
            return _upperBoundPos * passivePosFactor;
        }

        /// <summary>
        /// Get the scale value according to the major position
        /// </summary>
        private float GetScaleValue(float majorPosition)
        {
            return _scaleCurve.Evaluate(majorPosition);
        }
    }
}
