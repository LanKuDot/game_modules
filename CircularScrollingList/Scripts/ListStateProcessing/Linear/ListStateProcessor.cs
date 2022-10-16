using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.MovementCtrl;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    /// <summary>
    /// The processor for moving the list along a line
    /// </summary>
    public class ListStateProcessor : IListStateProcessor
    {
        #region Enums

        #endregion

        #region Private Components

        private CircularScrollingListSetting _listSetting;
        private RectTransform _rectTransform;
        private const int _numOfBoxes = 5;
        private readonly List<ListBoxState> _boxStates = new List<ListBoxState>();
        private FreeMovementCtrl _freeMovementCtrl;
        private UnitMovementCtrl _unitMovementCtrl;

        #endregion

        #region Position Variables

        private Func<Vector2, float> _getFactorFunc;
        private float _unitPos;
        private float _minPos;
        private float _maxPos;

        #endregion

        #region IListStateProcessor

        public void Initialize(ListSetupData setupData)
        {
            _listSetting = setupData.Setting;
            _rectTransform = setupData.RectTransform;
            for (var i = 0; i < _numOfBoxes; ++i)
                _boxStates.Add(new ListBoxState());

            InitializePositionVars();
            InitializeComponents();
        }

        public void SetMovement(InputInfo inputInfo)
        {
            switch (inputInfo.Phase) {
                case InputPhase.Began:
                    if (!_freeMovementCtrl.IsMovementEnded())
                        _freeMovementCtrl.EndMovement();
                    if (!_unitMovementCtrl.IsMovementEnded())
                        _unitMovementCtrl.EndMovement();
                    break;

                case InputPhase.Moved:
                    var deltaDistance = _getFactorFunc(inputInfo.DeltaLocalPos);
                    _freeMovementCtrl.SetMovement(deltaDistance, true);
                    break;

                case InputPhase.Ended:
                    deltaDistance = _getFactorFunc(inputInfo.DeltaLocalPos);
                    var finalVelocity = deltaDistance / inputInfo.DeltaTime;
                    _freeMovementCtrl.SetMovement(finalVelocity, false);
                    break;

                case InputPhase.Scrolled:
                    if (!_freeMovementCtrl.IsMovementEnded())
                        _freeMovementCtrl.EndMovement();

                    deltaDistance = inputInfo.DeltaLocalPos.y * _unitPos;
                    _unitMovementCtrl.SetMovement(deltaDistance, false);
                    break;
            }
        }

        public float GetMovement(float detailTime)
        {
            if (!_freeMovementCtrl.IsMovementEnded())
                return _freeMovementCtrl.GetDistance(detailTime);
            if (!_unitMovementCtrl.IsMovementEnded())
                return _unitMovementCtrl.GetDistance(detailTime);

            return 0;
        }

        public bool IsMovementEnded() =>
            _freeMovementCtrl.IsMovementEnded()
            && _unitMovementCtrl.IsMovementEnded();

        public void EndMovement()
        {
            _freeMovementCtrl.EndMovement();
            _unitMovementCtrl.EndMovement();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the position related controlling variables
        /// </summary>
        private void InitializePositionVars()
        {
            var rect = _rectTransform.rect;
            var rectDistance =
                _listSetting.direction == CircularScrollingList.Direction.Vertical
                    ? rect.height
                    : rect.width;

            _unitPos = rectDistance / (_numOfBoxes - 1) / _listSetting.boxDensity;

            // If there are event number of boxes, narrow the boundary for 1 unit pos.
            var boundPosAdjust =
                (_numOfBoxes & 0x1) == 0 ? _unitPos / 2 : 0;

            _minPos = _unitPos * (-1 * _numOfBoxes / 2 - 1) + boundPosAdjust;
            _maxPos = _unitPos * (_numOfBoxes / 2 + 1) - boundPosAdjust;
        }

        /// <summary>
        /// Initialize the movement controllers
        /// </summary>
        private void InitializeComponents()
        {
            if (_listSetting.direction == CircularScrollingList.Direction.Vertical)
                _getFactorFunc = FactorUtility.GetVector2Y;
            else
                _getFactorFunc = FactorUtility.GetVector2X;

            var exceedingLimit = _unitPos * 0.3f;

            _freeMovementCtrl = new FreeMovementCtrl(
                _listSetting.boxVelocityCurve,
                _listSetting.alignMiddle,
                exceedingLimit,
                () => 0.0f,
                () => ListPositionCtrl.PositionState.Middle);
            _unitMovementCtrl = new UnitMovementCtrl(
                _listSetting.boxPositionCurve,
                exceedingLimit,
                () => 0.0f,
                () => ListPositionCtrl.PositionState.Middle);
        }

        #endregion
    }
}
