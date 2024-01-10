using System;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    public class BoxTransformController : IBoxTransformController
    {
        #region Position Variables

        /// <summary>
        /// The number of the boxes
        /// </summary>
        private int _numOfBoxes;
        /// <summary>
        /// The distance between boxes
        /// </summary>
        private float _unitPos;
        /// <summary>
        /// The left/down-most position of the box
        /// </summary>
        private float _minPos;
        /// <summary>
        /// The right/up-most position of the box
        /// </summary>
        private float _maxPos;
        /// <summary>
        /// The lower boundary where the box will be moved to the other end
        /// </summary>
        private float _sideChangingMinPos;
        /// <summary>
        /// The upper boundary where the box will be moved to the other end
        /// </summary>
        private float _sideChangingMaxPos;
        /// <summary>
        /// The curve that mapping the major position to the minor position
        /// </summary>
        private RangeMappingCurve _positionCurve;
        /// <summary>
        /// The curve that mapping the major position to the local scale of the box
        /// </summary>
        private RangeMappingCurve _scaleCurve;

        #endregion

        #region Public Properties

        /// <summary>
        /// The baseline position at the top of the list
        /// </summary>
        public float TopBaseline { get; private set; }
        /// <summary>
        /// The baseline position at the middle of the list
        /// </summary>
        public float MiddleBaseline { get; private set; }
        /// <summary>
        /// The baseline position at the bottom of the list
        /// </summary>
        public float BottomBaseline { get; private set; }

        #endregion

        #region Variable Getter

        /// <summary>
        /// The function for getting the major factor of the Vector2
        /// </summary>
        private Func<Vector2, float> _getMajorFactorFunc;
        /// <summary>
        /// Get the final local position according to the major and minor position
        /// </summary>
        /// The signature is (major pos, minor pos, z pos) -> Vector3
        private Func<float, float, float, Vector3> _getLocalPositionFunc;

        #endregion

        public BoxTransformController(ListSetupData setupData)
        {
            _numOfBoxes = setupData.ListBoxes.Count;

            InitializePositionVars(
                setupData.RectTransform, setupData.ListSetting, _numOfBoxes);
            InitializeFactorGetter(setupData.ListSetting);
            InitializeCurves(setupData.ListSetting);
        }

        #region Initialization

        /// <summary>
        /// Initialize the position related controlling variables
        /// </summary>
        private void InitializePositionVars(
            RectTransform rectTransform, ListSetting listSetting,
            int numOfBoxes)
        {
            var rect = rectTransform.rect;
            var rectDistance =
                listSetting.Direction == CircularScrollingList.Direction.Vertical
                    ? rect.height
                    : rect.width;

            _unitPos = rectDistance / (numOfBoxes - 1) / listSetting.BoxDensity;

            // If there are event number of boxes, narrow the boundary for 1 unit pos.
            var boundAdjust = (numOfBoxes & 0x1) == 0 ? 0.5f : 0;
            TopBaseline = _unitPos * (numOfBoxes / 2 - boundAdjust);
            BottomBaseline = -TopBaseline;
            MiddleBaseline = _unitPos * boundAdjust;

            _maxPos = TopBaseline + _unitPos;
            _minPos = -_maxPos;
            _sideChangingMinPos = _minPos + _unitPos * 0.5f;
            _sideChangingMaxPos = _maxPos - _unitPos * 0.5f;
        }

        /// <summary>
        /// Initialize the functions for getting the factor
        /// </summary>
        /// <param name="setting">The setting of the list</param>
        private void InitializeFactorGetter(ListSetting setting)
        {
            if (setting.Direction == CircularScrollingList.Direction.Vertical) {
                _getMajorFactorFunc = FactorUtility.GetVector2Y;
                _getLocalPositionFunc = GetPositionYMajor;
            } else {
                _getMajorFactorFunc = FactorUtility.GetVector2X;
                _getLocalPositionFunc = GetPositionXMajor;
            }
        }

        /// <summary>
        /// Initialize the curves
        /// </summary>
        /// <param name="setting">The setting of the list</param>
        private void InitializeCurves(ListSetting setting)
        {
            _positionCurve =
                new RangeMappingCurve(
                    setting.BoxPositionCurve,
                    -1, 1,
                    _sideChangingMinPos, _sideChangingMaxPos);
            _scaleCurve =
                new RangeMappingCurve(
                    setting.BoxScaleCurve,
                    -1, 1,
                    _sideChangingMinPos, _sideChangingMaxPos);
        }

        #endregion

        #region IBoxTransformController

        /// <summary>
        /// Set the initial local transform
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="boxID">The ID of the box</param>
        public void SetInitialLocalTransform(IListBox box, int boxID)
        {
            var majorPos = _unitPos * (boxID * -1 + _numOfBoxes / 2);

            // If there are even number of boxes,
            // adjust the position one half unitPos down.
            if ((_numOfBoxes & 0x1) == 0) {
                majorPos = _unitPos * (boxID * -1 + _numOfBoxes / 2) - _unitPos / 2;
            }

            var boxTransform = box.GetTransform();

            var minorPos = GetMinorPosition(majorPos);
            var localPosZ = boxTransform.localPosition.z;

            var scaleValue = GetScaleValue(majorPos);
            var localScaleZ = boxTransform.localScale.z;

            boxTransform.localPosition =
                _getLocalPositionFunc(majorPos, minorPos, localPosZ);
            boxTransform.localScale =
                new Vector3(scaleValue, scaleValue, localScaleZ);

            if (Application.isPlaying)
                box.OnBoxMoved(GetPositionRatio(majorPos));
        }

        /// <summary>
        /// Update the local transform of the box according to the moving distance
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="deltaPos">The moving distance</param>
        /// <returns>The final status of the transform position</returns>
        public BoxPositionState UpdateLocalTransform(
            IListBox box, float deltaPos)
        {
            var boxTransform = box.GetTransform();
            var localPosition = boxTransform.localPosition;
            var majorFactor = _getMajorFactorFunc(localPosition);
            var majorPosition =
                GetMajorPosition(
                    majorFactor + deltaPos,
                    out var isJumpingToTop,
                    out var isJumpingToBottom);
            var minorPosition = GetMinorPosition(majorPosition);

            var localScale = boxTransform.localScale;
            var scaleValue = GetScaleValue(majorPosition);

            boxTransform.localPosition =
                _getLocalPositionFunc(majorPosition, minorPosition, localPosition.z);
            boxTransform.localScale =
                new Vector3(scaleValue, scaleValue, localScale.z);

            box.OnBoxMoved(GetPositionRatio(majorFactor));

            return
                isJumpingToTop ? BoxPositionState.JumpToTop :
                isJumpingToBottom ? BoxPositionState.JumpToBottom :
                BoxPositionState.Nothing;
        }

        #endregion

        #region Position Handling

        /// <summary>
        /// Get the position ratio of the major position in the list
        /// </summary>
        /// <returns>The position ratio from -1 to 1</returns>
        private float GetPositionRatio(float majorPosition)
        {
            return Mathf.InverseLerp(
                _sideChangingMinPos, _sideChangingMaxPos, majorPosition) * 2 - 1;
        }

        /// <summary>
        /// Get the major position according to the requested position
        /// If the box exceeds the boundary, one of the passed flags will be set
        /// to indicate that the content needs to be updated.
        /// </summary>
        /// <param name="positionValue">The requested position</param>
        /// <param name="isJumpingToTop">
        /// Will the final position make the box jump to the top of the list?
        /// </param>
        /// <param name="isJumpingToBottom">
        /// Will the final position make the box jump to the bottom of the list?
        /// </param>
        /// <returns>The final major position</returns>
        private float GetMajorPosition(
            float positionValue,
            out bool isJumpingToTop, out bool isJumpingToBottom)
        {
            isJumpingToTop = false;
            isJumpingToBottom = false;

            var beyondPos = 0.0f;
            var majorPos = positionValue;

            if (positionValue < _sideChangingMinPos) {
                beyondPos = positionValue - _minPos;
                majorPos = _maxPos - _unitPos + beyondPos;
                isJumpingToTop = true;
            } else if (positionValue > _sideChangingMaxPos) {
                beyondPos = positionValue - _maxPos;
                majorPos = _minPos + _unitPos + beyondPos;
                isJumpingToBottom = true;
            }

            return majorPos;
        }

        /// <summary>
        /// Get the minor position according to the major position
        /// </summary>
        /// <param name="majorPosition">The major position</param>
        /// <returns>The minor position</returns>
        private float GetMinorPosition(float majorPosition)
        {
            var minorPosFactor = _positionCurve.Evaluate(majorPosition);
            return _sideChangingMaxPos * minorPosFactor;
        }

        /// <summary>
        /// Return Vector3(majorPos, minorPos, z)
        /// </summary>
        private Vector3 GetPositionXMajor(float majorPos, float minorPos, float z)
        {
            return new Vector3(majorPos, minorPos, z);
        }

        /// <summary>
        /// Return Vector3(minorPos, majorPos, z)
        /// </summary>
        private Vector3 GetPositionYMajor(float majorPos, float minorPos, float z)
        {
            return new Vector3(minorPos, majorPos, z);
        }

        #endregion

        #region Scale Handling

        /// <summary>
        /// Get the scale value according to the major position
        /// </summary>
        private float GetScaleValue(float majorPosition)
        {
            return _scaleCurve.Evaluate(majorPosition);
        }

        #endregion
    }
}
