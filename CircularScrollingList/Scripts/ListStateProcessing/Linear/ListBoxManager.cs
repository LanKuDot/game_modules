﻿using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    using PositionState = BoxTransformController.PositionState;

    public class ListBoxManager : IListBoxManager
    {
        #region Public Properties

        /// <summary>
        /// The state of the list
        /// </summary>
        public ListFocusingState ListFocusingState { get; private set; } =
            ListFocusingState.Middle;
        /// <summary>
        /// The shortest distance to make a box at the center position of the list
        /// </summary>
        public float ShortestDistanceToCenter { get; private set; }

        #endregion

        #region Private Components

        /// <summary>
        /// The setting of the list
        /// </summary>
        private CircularScrollingListSetting _setting;
        /// <summary>
        /// The managed boxes
        /// </summary>
        private readonly List<IListBox> _boxes = new List<IListBox>();
        /// <summary>
        /// The component fot getting the list contents
        /// </summary>
        private IListContentProvider _contentProvider;
        /// <summary>
        /// The controller for setting the transform of the boxes
        /// </summary>
        private BoxTransformController _transformController;

        #endregion

        #region Private Fields

        /// <summary>
        /// The box which is closest to the center position
        /// </summary>
        private IListBox _centeredBox;
        /// <summary>
        /// The function for getting the major factor from the vector2
        /// </summary>
        private Func<Vector2, float> _getMajorFactorFunc;
        /// <summary>
        /// The number of the inactivated boxes
        /// </summary>
        private NumOfInactivatedBoxes _inactivatedBoxes;

        #endregion

        #region IListBoxManager

        public void Initialize(
            ListSetupData setupData, IListContentProvider contentProvider)
        {
            _setting = setupData.Setting;
            _boxes.Clear();
            _boxes.AddRange(setupData.ListBoxes);
            _contentProvider = contentProvider;
            _transformController = new BoxTransformController(setupData);
            _inactivatedBoxes =
                new NumOfInactivatedBoxes(_boxes.Count / 2);

            InitializeFactorFunc(_setting.direction);
            InitializeBoxes();
        }

        public void UpdateBoxes(float movementValue)
        {
            if (Mathf.Approximately(movementValue, 0f))
                return;

            foreach (var box in _boxes) {
                var positionStatus =
                    _transformController.SetLocalTransform(
                        box.Transform, movementValue);

                var contentID = box.ContentID;
                switch (positionStatus) {
                    case PositionState.Nothing:
                        continue;
                    case PositionState.JumpToTop:
                        contentID =
                            _contentProvider.GetContentIDByNextBox(
                                box.NextListBox.ContentID);
                        break;
                    case PositionState.JumpToBottom:
                        contentID =
                            _contentProvider.GetContentIDByLastBox(
                                box.LastListBox.ContentID);
                        break;
                }

                UpdateBoxContent(box, contentID, positionStatus);
            }

            UpdateListState();
        }

        public void RefreshBoxes(int centeredContentID = -1)
        {
            var curCenteredContentID = _centeredBox.ContentID;
            var numOfContents = _contentProvider.GetContentCount();

            if (centeredContentID < 0)
                centeredContentID =
                    curCenteredContentID == int.MinValue
                        ? 0
                        : Mathf.Min(curCenteredContentID, numOfContents - 1);
            else if (centeredContentID >= numOfContents)
                throw new IndexOutOfRangeException(
                    $"{nameof(centeredContentID)} is larger than the number of contents");

            RecalculateAllBoxContent(centeredContentID);
        }

        public IListBox GetCenteredBox() => _centeredBox;

        #endregion

        #region Initialization

        private void InitializeFactorFunc(CircularScrollingList.Direction direction)
        {
            if (direction == CircularScrollingList.Direction.Horizontal)
                _getMajorFactorFunc = FactorUtility.GetVector2X;
            else
                _getMajorFactorFunc = FactorUtility.GetVector2Y;
        }

        /// <summary>
        /// Initialize the boxes
        /// </summary>
        private void InitializeBoxes()
        {
            var numOfBoxes = _boxes.Count;
            for (var boxID = 0; boxID < numOfBoxes; ++boxID) {
                var box = _boxes[boxID];
                var lastListBox =
                    _boxes[(int)Mathf.Repeat(boxID - 1, numOfBoxes)];
                var nextListBox =
                    _boxes[(int)Mathf.Repeat(boxID + 1, numOfBoxes)];
                box.Initialize(boxID, lastListBox, nextListBox);
                box.OnBoxClick.AddListener(_setting.onBoxClick.Invoke);

                _transformController.SetInitialLocalTransform(box.Transform, boxID);

                var contentID = _contentProvider.GetInitialContentID(boxID);
                UpdateBoxContent(box, contentID, PositionState.Nothing);
            }

            UpdateListState();
            InitializeBoxLayerSorting();
        }

        /// <summary>
        /// Initialize the sorting of the image of the boxes
        /// </summary>
        private void InitializeBoxLayerSorting()
        {
            var centeredBoxIndex = 0;
            var numOfBoxes = _boxes.Count;
            for (; centeredBoxIndex < numOfBoxes; ++centeredBoxIndex)
                if (_boxes[centeredBoxIndex] == _centeredBox)
                    break;

            for (var i = centeredBoxIndex - 1; i >= 0; --i)
                _boxes[i].PushToBack();
            for (var i = centeredBoxIndex + 1; i < numOfBoxes; ++i)
                _boxes[i].PushToBack();
        }

        #endregion

        #region List State

        /// <summary>
        /// Update the list state
        /// </summary>
        private void UpdateListState()
        {
            UpdateCenteredBox();
            UpdateListFocusingState();
        }

        /// <summary>
        /// Update the focusing state of the list
        /// </summary>
        private void UpdateListFocusingState()
        {
            if (_setting.listType != CircularScrollingList.ListType.Linear)
                return;

            const float tolerance = 1e-4f;

            if (_inactivatedBoxes.AtTop >= _inactivatedBoxes.MaxNum
                && ShortestDistanceToCenter > -tolerance)
                ListFocusingState = ListFocusingState.Top;
            else if (_inactivatedBoxes.AtBottom >= _inactivatedBoxes.MaxNum
                     && ShortestDistanceToCenter < tolerance)
                ListFocusingState = ListFocusingState.Bottom;
            else
                ListFocusingState = ListFocusingState.Middle;
        }

        #endregion

        #region Centered Box

        /// <summary>
        /// Find the box which is closet to the center
        /// </summary>
        /// <param name="distance">
        /// The distance for moving this box to the center
        /// </param>
        /// <returns>The box</returns>
        private IListBox FindBoxClosestToCenter(out float distance)
        {
            distance = Mathf.Infinity;
            IListBox candidateBox = null;

            foreach (var listBox in _boxes) {
                // Skip the inactivated box
                if (!listBox.IsActivated)
                    continue;

                var localPos = listBox.Transform.localPosition;
                var deltaDistance = -_getMajorFactorFunc(localPos);

                if (Mathf.Abs(deltaDistance) >= Mathf.Abs(distance))
                    continue;

                distance = deltaDistance;
                candidateBox = listBox;
            }

            return candidateBox;
        }

        /// <summary>
        /// Update the centered box
        /// </summary>
        private void UpdateCenteredBox()
        {
            var candidateBox = FindBoxClosestToCenter(out var distance);
            ShortestDistanceToCenter = distance;

            if (candidateBox == _centeredBox)
                return;

            candidateBox.PopToFront();
            _setting.onCenteredContentChanged.Invoke(candidateBox.ContentID);
            // TODO _setting.onCenteredBoxChanged.Invoke(_centeredBox, candidateBox);
            _centeredBox = candidateBox;
        }

        #endregion

        #region Content Management

        /// <summary>
        /// Recalculate all the box contents
        /// </summary>
        /// <param name="newCenteredContentID">The new centered content ID</param>
        private void RecalculateAllBoxContent(int newCenteredContentID)
        {
            ResetNumOfInactivatedBoxes();

            var numOfBoxes = _boxes.Count;
            var centeredBoxID = _centeredBox.ListBoxID;
            var reverseFactor = _setting.reverseOrder ? -1 : 1;
            // TODO Store the pos factor in the boxes
            var factorFunc =
                _setting.direction == CircularScrollingList.Direction.Horizontal
                    ? (Func<Vector2, float>)FactorUtility.GetVector2X
                    : FactorUtility.GetVector2Y;

            foreach (var box in _boxes) {
                var posFactor = factorFunc(box.Transform.localPosition);
                var tempBoxID = box.ListBoxID;

                if (tempBoxID > centeredBoxID && posFactor > 0)
                    tempBoxID -= numOfBoxes;
                else if (tempBoxID < centeredBoxID && posFactor < 0)
                    tempBoxID += numOfBoxes;

                var contentID =
                    newCenteredContentID + (tempBoxID - centeredBoxID) * reverseFactor;
                contentID = _contentProvider.GetContentID(contentID);
                UpdateBoxContent(box, contentID, PositionState.Nothing);
            }

            UpdateListState();
        }

        /// <summary>
        /// Update the box content according to the position status
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="contentID">The target content ID</param>
        /// <param name="positionState">The current position state of the box</param>
        private void UpdateBoxContent(
            IListBox box, int contentID, PositionState positionState)
        {
            if (positionState != PositionState.Nothing)
                box.PushToBack();

            SetBoxContent(box, contentID);
            ToggleBoxActivation(box, positionState);
        }

        /// <summary>
        /// Set the content of the box
        /// </summary>
        private void SetBoxContent(IListBox box, int contentID)
        {
            _contentProvider.TryGetContent(contentID, out var contentReturned);
            box.SetContent(contentID, contentReturned);
        }

        #endregion

        #region Box Activation

        /// <summary>
        /// Check if it needs to activate/inactivate the box
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="positionState">The position status of the box</param>
        private void ToggleBoxActivation(IListBox box, PositionState positionState)
        {
            var contentID = box.ContentID;

            if (contentID == int.MinValue) {
                box.IsActivated = false;
                return;
            }

            if (_setting.listType != CircularScrollingList.ListType.Linear)
                return;

            var isPreviouslyActivated = box.IsActivated;
            var idState = _contentProvider.GetIDState(contentID);
            var isIdValid = idState == ContentIDState.Valid;

            if (!isIdValid && isPreviouslyActivated)
                box.IsActivated = false;
            else if (isIdValid && !isPreviouslyActivated)
                box.IsActivated = true;

            UpdateNumOfInactivatedBoxes(positionState, idState, !isPreviouslyActivated);
        }

        /// <summary>
        /// Update the number of the inactivated boxes
        /// </summary>
        /// <param name="positionState">The position state of the box</param>
        /// <param name="idState">The id state of the box</param>
        /// <param name="isPreviouslyInactivated">
        /// Is the box previously inactivated?
        /// </param>
        private void UpdateNumOfInactivatedBoxes(
            PositionState positionState, ContentIDState idState,
            bool isPreviouslyInactivated)
        {
            var isReverseOrder = _setting.reverseOrder;
            var isIdValid = idState == ContentIDState.Valid;

            switch (positionState) {
                case PositionState.Nothing:
                    if (isIdValid)
                        break;

                    if ((!isReverseOrder && idState == ContentIDState.Underflow)
                        || (isReverseOrder && idState == ContentIDState.Overflow)) {
                        ++_inactivatedBoxes.AtTop;
                        break;
                    }

                    if ((!isReverseOrder && idState == ContentIDState.Overflow)
                        || (isReverseOrder && idState == ContentIDState.Underflow)) {
                        ++_inactivatedBoxes.AtBottom;
                    }
                    break;

                case PositionState.JumpToTop:
                    if (!isIdValid)
                        ++_inactivatedBoxes.AtTop;
                    if (isPreviouslyInactivated)
                        --_inactivatedBoxes.AtBottom;
                    break;

                case PositionState.JumpToBottom:
                    if (!isIdValid)
                        ++_inactivatedBoxes.AtBottom;
                    if (isPreviouslyInactivated)
                        --_inactivatedBoxes.AtTop;
                    break;
            }
        }

        /// <summary>
        /// Reset the number of the inactivated boxes
        /// </summary>
        private void ResetNumOfInactivatedBoxes()
        {
            _inactivatedBoxes.AtBottom = 0;
            _inactivatedBoxes.AtTop = 0;
        }

        #endregion

        #region Sub Data

        /// <summary>
        /// The data class for storing the number of inactivated boxes
        /// </summary>
        private class NumOfInactivatedBoxes
        {
            /// <summary>
            /// The max number of inactivated boxes
            /// </summary>
            public readonly int MaxNum;
            /// <summary>
            /// The number of inactivated boxes at the top
            /// </summary>
            public int AtTop;
            /// <summary>
            /// The number of inactivated boxes at the bottom
            /// </summary>
            public int AtBottom;

            public NumOfInactivatedBoxes(int maxNum)
            {
                MaxNum = maxNum;
            }
        }

        #endregion
    }
}