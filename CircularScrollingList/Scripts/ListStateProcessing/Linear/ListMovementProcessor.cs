using System;
using AirFishLab.ScrollingList.ListStateProcessing.Linear;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    /// <summary>
    /// The processor for moving the list along a line
    /// </summary>
    public class ListMovementProcessor : IListMovementProcessor
    {
        #region Enums

        #endregion

        #region Private Members

        private Func<Vector2, float> _getFactorFunc;
        private float _unitPos;
        private FreeMovementCtrl _freeMovementCtrl;
        private UnitMovementCtrl _unitMovementCtrl;
        private ListBoxController _listBoxController;
        /// <summary>
        /// The factor for reversing the scrolling direction or not
        /// </summary>
        private int _scrollingFactor;
        /// <summary>
        /// The factor for reversing the moving direction of selection movement
        /// </summary>
        private int _selectionDistanceFactor;

        #endregion

        #region IListStateProcessor

        public void Initialize(ListSetupData setupData)
        {
            InitializePositionVars(
                setupData.RectTransform.rect,
                setupData.ListSetting.Direction,
                setupData.ListSetting.BoxDensity,
                setupData.ListBoxes.Count);
            InitializeComponents(setupData.ListSetting);
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
                    SetUnitMovement((int)inputInfo.DeltaLocalPos.y * _scrollingFactor);
                    break;
            }
        }

        public void SetUnitMovement(int unit)
        {
            if (!_freeMovementCtrl.IsMovementEnded())
                _freeMovementCtrl.EndMovement();

            var deltaDistance = unit * _unitPos;

            // If the unit movement is not started yet,
            // countervail the position difference
            if (_unitMovementCtrl.IsMovementEnded())
                deltaDistance += _listBoxController.ShortestDistanceToCenter;

            _unitMovementCtrl.SetMovement(deltaDistance, false);
        }

        public void SetSelectionMovement(int units)
        {
            EndMovement();

            var deltaDistance =
                units * _unitPos * _selectionDistanceFactor
                + _listBoxController.ShortestDistanceToCenter;
            _unitMovementCtrl.SetMovement(deltaDistance, false);
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
        private void InitializePositionVars(
            Rect parentRect, CircularScrollingList.Direction direction,
            float boxDensity, int numOfBoxes)
        {
            var rectDistance =
                direction == CircularScrollingList.Direction.Vertical
                    ? parentRect.height
                    : parentRect.width;

            _unitPos = rectDistance / (numOfBoxes - 1) / boxDensity;

            if (direction == CircularScrollingList.Direction.Vertical)
                _getFactorFunc = FactorUtility.GetVector2Y;
            else
                _getFactorFunc = FactorUtility.GetVector2X;
        }

        /// <summary>
        /// Initialize the movement controllers
        /// </summary>
        private void InitializeComponents(ListSetting setting)
        {
            var exceedingLimit = _unitPos * 0.3f;

            _freeMovementCtrl = new FreeMovementCtrl(
                setting.BoxVelocityCurve,
                setting.AlignAtFocusingPosition,
                exceedingLimit,
                GetAligningDistance,
                GetListFocusingState);
            _unitMovementCtrl = new UnitMovementCtrl(
                setting.BoxMovementCurve,
                exceedingLimit,
                GetAligningDistance,
                GetListFocusingState);
            _scrollingFactor = setting.ReverseScrollingDirection ? -1 : 1;
            _selectionDistanceFactor = setting.ReverseContentOrder ? -1 : 1;
        }

        #endregion

        #region ListBoxManager

        /// <summary>
        /// Set the list box controller for the processor
        /// </summary>
        public void SetListBoxController(ListBoxController listBoxController)
        {
            _listBoxController = listBoxController;
        }

        private ListFocusingState GetListFocusingState() =>
            _listBoxController.ListFocusingState;

        private float GetAligningDistance() =>
            _listBoxController.ShortestDistanceToCenter;

        #endregion
    }
}
