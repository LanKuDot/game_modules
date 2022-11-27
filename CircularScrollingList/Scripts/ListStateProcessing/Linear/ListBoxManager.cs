using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.Util;
using UnityEditor;
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
        public ListState ListState { get; private set; }

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
        /// The shortest distance to make a box at the center position of the list
        /// </summary>
        private float _shortestDistanceToCenter;
        /// <summary>
        /// The function for getting the major factor from the vector2
        /// </summary>
        private Func<Vector2, float> _getMajorFactorFunc;
        /// <summary>
        /// The number of the inactivated boxes
        /// </summary>
        private readonly NumOfInactivatedBoxes _inactivatedBoxes =
            new NumOfInactivatedBoxes();

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

            InitializeFactorFunc(_setting.direction);
            InitializeBoxes();
        }

        public void UpdateBoxes(float movementValue)
        {
            foreach (var box in _boxes) {
                var positionStatus =
                    _transformController.SetLocalTransform(
                        box.Transform, movementValue);
                UpdateBoxContent(box, positionStatus);
            }

            UpdateListState();
        }

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
        /// Initialized the boxes
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

                _transformController.SetInitialLocalTransform(box.Transform, boxID);

                var contentID = _contentProvider.GetInitialContentID(boxID);
                SetBoxContent(box, contentID);
                ToggleBoxActivation(box, PositionState.Nothing);
            }

            FindShortestDistanceToCenter(out _centeredBox);
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

        #region List Status

        /// <summary>
        /// Find the shortest distance to make a box at the center of the list
        /// </summary>
        /// <param name="candidateBox">
        /// The candidate box which is closest to the center
        /// </param>
        /// <returns>
        /// The distance to make the candidate box at the center of the list
        /// </returns>
        private float FindShortestDistanceToCenter(out IListBox candidateBox)
        {
            var shortestDistance = Mathf.Infinity;
            candidateBox = null;

            foreach (var listBox in _boxes) {
                // Skip the inactivated box
                if (!listBox.IsActivated)
                    continue;

                var localPos = listBox.Transform.localPosition;
                var deltaDistance = -_getMajorFactorFunc(localPos);

                if (Mathf.Abs(deltaDistance) >= Mathf.Abs(shortestDistance))
                    continue;

                shortestDistance = deltaDistance;
                candidateBox = listBox;
            }

            return shortestDistance;
        }

        /// <summary>
        /// Update the state of the list
        /// </summary>
        private void UpdateListState()
        {
            _shortestDistanceToCenter =
                FindShortestDistanceToCenter(out var candidateBox);

            if (candidateBox != _centeredBox) {
                candidateBox.PopToFront();
                _setting.onCenteredContentChanged.Invoke(candidateBox.ContentID);
                // TODO _setting.onCenteredBoxChanged.Invoke(_centeredBox, candidateBox);
                _centeredBox = candidateBox;
            }
        }

        #endregion

        #region Content Management

        /// <summary>
        /// Update the box content according to the position status
        /// </summary>
        private void UpdateBoxContent(IListBox box, PositionState positionState)
        {
            if (positionState == PositionState.Nothing)
                return;

            var contentID = 0;
            switch (positionState) {
                case PositionState.JumpToTop:
                    contentID =
                        _contentProvider.GetContentIDByNextBox(
                            box.NextListBox.ContentID);
                    box.PushToBack();
                    break;
                case PositionState.JumpToBottom:
                    contentID =
                        _contentProvider.GetContentIDByLastBox(
                            box.LastListBox.ContentID);
                    box.PushToBack();
                    break;
            }

            SetBoxContent(box, contentID);
            ToggleBoxActivation(box, positionState);
        }

        /// <summary>
        /// Set the content of the box
        /// </summary>
        private void SetBoxContent(IListBox box, int contentID)
        {
            var content =
                _contentProvider.TryGetContent(contentID, out var contentReturned)
                    ? contentReturned
                    : null;
            box.SetContent(contentID, content);
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

        #endregion

        #region Sub Data

        /// <summary>
        /// The data class for storing the number of inactivated boxes
        /// </summary>
        private class NumOfInactivatedBoxes
        {
            /// <summary>
            /// The number of inactivated boxes at the top
            /// </summary>
            public int AtTop;
            /// <summary>
            /// The number of inactivated boxes at the bottom
            /// </summary>
            public int AtBottom;
        }

        #endregion
    }
}
