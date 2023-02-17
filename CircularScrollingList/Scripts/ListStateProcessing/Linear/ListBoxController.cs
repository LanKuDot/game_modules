﻿using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    public class ListBoxController : IListBoxController
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
        private ListSetting _setting;
        /// <summary>
        /// The managed boxes
        /// </summary>
        private readonly List<IListBox> _boxes = new List<IListBox>();
        /// <summary>
        /// The component for controlling the box transform
        /// </summary>
        private BoxTransformController _transformController;
        /// <summary>
        /// The component for finding the focusing box
        /// </summary>
        private FocusingBoxFinder _focusingBoxFinder;
        /// <summary>
        /// The component for getting the list contents
        /// </summary>
        private ListContentProvider _contentProvider;
        /// <summary>
        /// The function for updating the focusing box
        /// </summary>
        private Action _updateFocusingBoxFunc;

        #endregion

        #region Private Fields

        /// <summary>
        /// The box which is closest to the center position
        /// </summary>
        private IListBox _focusingBox;

        #endregion

        #region Initialize

        public void Initialize(ListSetupData setupData)
        {
            _setting = setupData.ListSetting;
            _boxes.Clear();
            _boxes.AddRange(setupData.ListBoxes);
            _transformController = new BoxTransformController(setupData);
            _contentProvider = setupData.ListContentProvider;
            _focusingBoxFinder =
                new FocusingBoxFinder(
                    _boxes, _setting,
                    _transformController.TopBaseline,
                    _transformController.BottomBaseline);

            switch (_setting.FocusingPosition) {
                case CircularScrollingList.FocusingPosition.Top:
                    _updateFocusingBoxFunc = UpdateTopFocusingBox;
                    break;
                case CircularScrollingList.FocusingPosition.Center:
                    _updateFocusingBoxFunc = UpdateCenterFocusingBox;
                    break;
                case CircularScrollingList.FocusingPosition.Bottom:
                    _updateFocusingBoxFunc = UpdateBottomFocusingBox;
                    break;
            }

            InitializeBoxes(setupData);
        }

        /// <summary>
        /// Initialize the boxes
        /// </summary>
        private void InitializeBoxes(ListSetupData setupData)
        {
            var numOfBoxes = _boxes.Count;
            for (var boxID = 0; boxID < numOfBoxes; ++boxID) {
                var box = _boxes[boxID];
                var lastListBox =
                    _boxes[(int)Mathf.Repeat(boxID - 1, numOfBoxes)];
                var nextListBox =
                    _boxes[(int)Mathf.Repeat(boxID + 1, numOfBoxes)];
                box.Initialize(
                    setupData.ScrollingList,
                    boxID, lastListBox, nextListBox);
                box.OnBoxClick.AddListener(_setting.OnBoxClick.Invoke);

                _transformController.SetInitialLocalTransform(box.GetTransform(), boxID);

                var contentID =
                    _contentProvider.GetInitialContentID(
                        boxID + setupData.ListSetting.InitFocusedContentID);
                UpdateBoxContent(box, contentID);
            }

            _updateFocusingBoxFunc();
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
                if (_boxes[centeredBoxIndex] == _focusingBox)
                    break;

            for (var i = centeredBoxIndex - 1; i >= 0; --i)
                _boxes[i].PushToBack();
            for (var i = centeredBoxIndex + 1; i < numOfBoxes; ++i)
                _boxes[i].PushToBack();
        }

        #endregion

        #region Box Updating

        public void UpdateBoxes(float movementValue)
        {
            if (Mathf.Approximately(movementValue, 0f))
                return;

            foreach (var box in _boxes) {
                var positionStatus =
                    _transformController.UpdateLocalTransform(
                        box.GetTransform(), movementValue);
                var contentID = box.ContentID;
                switch (positionStatus) {
                    case BoxPositionState.Nothing:
                        continue;
                    case BoxPositionState.JumpToTop:
                        box.PushToBack();
                        contentID =
                            _contentProvider.GetContentIDByNextBox(
                                box.NextListBox.ContentID);
                        break;
                    case BoxPositionState.JumpToBottom:
                        box.PushToBack();
                        contentID =
                            _contentProvider.GetContentIDByLastBox(
                                box.LastListBox.ContentID);
                        break;
                }

                UpdateBoxContent(box, contentID);
            }

            _updateFocusingBoxFunc();
        }

        public void RefreshBoxes(int centeredContentID = -1)
        {
            var curCenteredContentID = _focusingBox.ContentID;
            var numOfContents = _contentProvider.GetContentCount();

            if (centeredContentID >= numOfContents)
                throw new IndexOutOfRangeException(
                    $"{nameof(centeredContentID)} is larger than the number of contents");

            if (centeredContentID < 0)
                centeredContentID =
                    curCenteredContentID == ListContentProvider.NO_CONTENT_ID
                        ? 0
                        : Mathf.Min(curCenteredContentID, numOfContents - 1);

            RecalculateAllBoxContent(centeredContentID);
        }

        public IListBox GetFocusingBox() => _focusingBox;

        #endregion

        #region List State

        /// <summary>
        /// Update the top focusing box
        /// </summary>
        private void UpdateTopFocusingBox()
        {
            var result =
                _focusingBoxFinder.FindForBothEnds(_contentProvider.GetContentCount());
            var (focusingBox, aligningDistance) = result.TopFocusing;
            UpdateFocusingBox(
                focusingBox, aligningDistance, result.ListFocusingState);
        }

        /// <summary>
        /// Update the center focusing box
        /// </summary>
        private void UpdateCenterFocusingBox()
        {
            var result =
                _focusingBoxFinder.FindForMiddle(_contentProvider.GetContentCount());
            var (focusingBox, aligningDistance) = result.MiddleFocusing;
            UpdateFocusingBox(
                focusingBox, aligningDistance, result.ListFocusingState);
        }

        /// <summary>
        /// Update the bottom focusing box
        /// </summary>
        private void UpdateBottomFocusingBox()
        {
            var result =
                _focusingBoxFinder.FindForBothEnds(_contentProvider.GetContentCount());
            var (focusingBox, aligningDistance) = result.BottomFocusing;
            UpdateFocusingBox(
                focusingBox, aligningDistance, result.ListFocusingState);
        }

        /// <summary>
        /// Update the focusing box
        /// </summary>
        private void UpdateFocusingBox(
            IListBox focusingBox, float aligningDistance,
            ListFocusingState listFocusingState)
        {
            ListFocusingState = listFocusingState;
            ShortestDistanceToCenter = aligningDistance;

            if (focusingBox == _focusingBox)
                return;

            focusingBox.PopToFront();
            _setting.OnFocusingBoxChanged.Invoke(
                (ListBox)_focusingBox, (ListBox)focusingBox);
            _focusingBox = focusingBox;
        }

        #endregion

        #region Content Management

        /// <summary>
        /// Recalculate all the box contents
        /// </summary>
        /// <param name="newCenteredContentID">The new centered content ID</param>
        private void RecalculateAllBoxContent(int newCenteredContentID)
        {
            var numOfBoxes = _boxes.Count;
            var centeredBoxID = _focusingBox.ListBoxID;
            var reverseFactor = _setting.ReverseContentOrder ? -1 : 1;
            // TODO Store the pos factor in the boxes
            var factorFunc =
                _setting.Direction == CircularScrollingList.Direction.Horizontal
                    ? (Func<Vector2, float>)FactorUtility.GetVector2X
                    : FactorUtility.GetVector2Y;

            foreach (var box in _boxes) {
                var posFactor = factorFunc(box.GetPosition());
                var tempBoxID = box.ListBoxID;

                if (tempBoxID > centeredBoxID && posFactor > 0)
                    tempBoxID -= numOfBoxes;
                else if (tempBoxID < centeredBoxID && posFactor < 0)
                    tempBoxID += numOfBoxes;

                var contentID =
                    newCenteredContentID + (tempBoxID - centeredBoxID) * reverseFactor;
                var newContentID = _contentProvider.GetRefreshedContentID(contentID);
                UpdateBoxContent(box, newContentID);
            }

            _updateFocusingBoxFunc();
        }

        /// <summary>
        /// Update the box content according to the position status
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="contentID">The target content ID</param>
        private void UpdateBoxContent(IListBox box, int contentID)
        {
            var idState = _contentProvider.GetIDState(contentID);
            ToggleBoxActivation(box, idState);

            box.SetContentID(contentID);
            if (_contentProvider.TryGetContent(contentID, out var content))
                box.SetContent(content);
        }

        /// <summary>
        /// Check if it needs to activate/inactivate the box
        /// </summary>
        /// <param name="box">The target box</param>
        /// <param name="idState">The content id state of the box</param>
        private void ToggleBoxActivation(IListBox box, ContentIDState idState)
        {
            var contentID = box.ContentID;

            // If there has no content in the content provider,
            // just inactivate the box.
            if (contentID == ListContentProvider.NO_CONTENT_ID) {
                box.IsActivated = false;
                return;
            }

            if (_setting.ListType != CircularScrollingList.ListType.Linear)
                return;

            var isPreviouslyActivated = box.IsActivated;
            var isIdValid = idState == ContentIDState.Valid;

            if (!isIdValid && isPreviouslyActivated)
                box.IsActivated = false;
            else if (isIdValid && !isPreviouslyActivated)
                box.IsActivated = true;
        }

        #endregion
    }
}
