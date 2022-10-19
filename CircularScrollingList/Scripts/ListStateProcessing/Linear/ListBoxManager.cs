using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    public class ListBoxManager : IListBoxManager
    {
        #region Private Components

        /// <summary>
        /// The managed boxes
        /// </summary>
        private readonly List<ListBox> _boxes = new List<ListBox>();
        /// <summary>
        /// The number of boxes
        /// </summary>
        private int _numOfBoxes;

        #endregion

        #region Position Variables

        private Func<Vector2, float> _getFactorFunc;
        private float _unitPos;
        private float _minPos;
        private float _maxPos;

        #endregion

        #region IListBoxManager

        public void Initialize(ListSetupData setupData)
        {
            _boxes.Clear();
            _boxes.AddRange(setupData.ListBoxes);
            _numOfBoxes = _boxes.Count;

            InitializePositionVars(
                setupData.RectTransform, setupData.Setting, _numOfBoxes);
        }

        public void UpdateBoxes(float movementValue)
        {
            Debug.Log(movementValue);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the position related controlling variables
        /// </summary>
        private void InitializePositionVars(
            RectTransform rectTransform, CircularScrollingListSetting listSetting,
            int numOfBoxes)
        {
            var rect = rectTransform.rect;
            var rectDistance =
                listSetting.direction == CircularScrollingList.Direction.Vertical
                    ? rect.height
                    : rect.width;

            _unitPos = rectDistance / (numOfBoxes - 1) / listSetting.boxDensity;

            // If there are event number of boxes, narrow the boundary for 1 unit pos.
            var boundPosAdjust =
                (numOfBoxes & 0x1) == 0 ? _unitPos / 2 : 0;

            _minPos = _unitPos * (-1 * numOfBoxes / 2 - 1) + boundPosAdjust;
            _maxPos = _unitPos * (numOfBoxes / 2 + 1) - boundPosAdjust;

            if (listSetting.direction == CircularScrollingList.Direction.Vertical)
                _getFactorFunc = FactorUtility.GetVector2Y;
            else
                _getFactorFunc = FactorUtility.GetVector2X;
        }

        #endregion
    }
}
