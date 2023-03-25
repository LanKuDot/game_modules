using System;
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
        /// <summary>
        /// Whether to align a box after releasing
        /// </summary>
        private bool _alignAtFocusingPosition;

        #endregion

        #region IListStateProcessor

        public void Initialize(ListSetupData setupData)
        {
            var setting = setupData.ListSetting;

            InitializePositionVars(
                setupData.RectTransform.rect,
                setting.Direction,
                setting.BoxDensity,
                setupData.ListBoxes.Count);
            InitializeComponents(setting);

            _alignAtFocusingPosition = setting.AlignAtFocusingPosition;
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

            _unitMovementCtrl.SetMovement(deltaDistance, false);
        }

        public void SetSelectionMovement(int units)
        {
            EndMovement(false);

            var deltaDistance =
                units * _unitPos * _selectionDistanceFactor;
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

        public bool NeedToAlign()
        {
            return (_alignAtFocusingPosition && !_freeMovementCtrl.IsMovementEnded())
                   || !_unitMovementCtrl.IsMovementEnded();
        }

        public void EndMovement(bool toAlign)
        {
            _freeMovementCtrl.EndMovement();
            _unitMovementCtrl.EndMovement();

            // Use unit movement control to align the box
            if (toAlign)
                _unitMovementCtrl.SetMovement(0, false);
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
                _unitPos * 1.2f,
                exceedingLimit,
                GetFocusingDistanceOffset,
                GetListFocusingState);
            _unitMovementCtrl = new UnitMovementCtrl(
                setting.BoxMovementCurve,
                exceedingLimit,
                GetFocusingDistanceOffset,
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

        private float GetFocusingDistanceOffset() =>
            _listBoxController.FocusingDistanceOffset;

        #endregion
    }
}
