using System;
using AirFishLab.ScrollingList.MovementCtrl;
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
        private ListBoxManager _listBoxManager;

        #endregion

        #region IListStateProcessor

        public void Initialize(ListSetupData setupData)
        {
            InitializePositionVars(
                setupData.RectTransform.rect,
                setupData.Setting.direction,
                setupData.Setting.boxDensity,
                setupData.ListBoxes.Count);
            InitializeComponents(setupData.Setting);
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
        private void InitializeComponents(CircularScrollingListSetting setting)
        {
            var exceedingLimit = _unitPos * 0.3f;

            _freeMovementCtrl = new FreeMovementCtrl(
                setting.boxVelocityCurve,
                setting.alignMiddle,
                exceedingLimit,
                GetAligningDistance,
                GetListFocusingState);
            _unitMovementCtrl = new UnitMovementCtrl(
                setting.boxMovementCurve,
                exceedingLimit,
                GetAligningDistance,
                GetListFocusingState);
        }

        #endregion

        #region ListBoxManager

        /// <summary>
        /// Set the list box manager for the processor
        /// </summary>
        public void SetListBoxManager(ListBoxManager listBoxManager)
        {
            _listBoxManager = listBoxManager;
        }

        private ListFocusingState GetListFocusingState() =>
            _listBoxManager.ListFocusingState;

        private float GetAligningDistance() =>
            _listBoxManager.ShortestDistanceToCenter;

        #endregion
    }
}
