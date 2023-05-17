using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
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
        /// The distance how long is the box away from the focusing position
        /// </summary>
        public float FocusingDistanceOffset { get; private set; }

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
        /// The container for storing the boxes to be updated
        /// </summary>
        private readonly List<IListBox> _boxesToBeUpdated = new List<IListBox>();
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
        /// <summary>
        /// The function for recalculating the all box content
        /// </summary>
        private Action<int> _recalculateAllBoxContentFunc;

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
                    _transformController.MiddleBaseline,
                    _transformController.BottomBaseline);

            switch (_setting.FocusingPosition) {
                case CircularScrollingList.FocusingPosition.Top:
                    _updateFocusingBoxFunc = UpdateTopFocusingBox;
                    _recalculateAllBoxContentFunc = RecalculateAllBoxContentForBothEnds;
                    break;
                case CircularScrollingList.FocusingPosition.Center:
                    _updateFocusingBoxFunc = UpdateCenterFocusingBox;
                    _recalculateAllBoxContentFunc = RecalculateAllBoxContentFofMiddle;
                    break;
                case CircularScrollingList.FocusingPosition.Bottom:
                    _updateFocusingBoxFunc = UpdateBottomFocusingBox;
                    _recalculateAllBoxContentFunc = RecalculateAllBoxContentForBothEnds;
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
                box.OnBoxSelected.AddListener(_setting.OnBoxSelected.Invoke);

                _transformController.SetInitialLocalTransform(box, boxID);

                var contentID =
                    _contentProvider.GetInitialContentID(boxID);
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

            var allPositionStatuses = BoxPositionState.Nothing;
            foreach (var box in _boxes) {
                var positionStatus =
                    _transformController.UpdateLocalTransform(box, movementValue);

                if (positionStatus == BoxPositionState.Nothing)
                    continue;

                _boxesToBeUpdated.Add(box);
                // The position statuses of all boxes updated in a single call
                // are the same
                allPositionStatuses = positionStatus;
            }

            UpdateBoxesInOrder(_boxesToBeUpdated, allPositionStatuses);
            _boxesToBeUpdated.Clear();

            _updateFocusingBoxFunc();
        }

        public void RefreshBoxes(int focusingContentID = -1)
        {
            var curFocusingContentID = _focusingBox.ContentID;
            var numOfContents = _contentProvider.GetContentCount();

            if (focusingContentID >= numOfContents)
                throw new IndexOutOfRangeException(
                    $"{nameof(focusingContentID)} is larger than the number of contents");

            if (focusingContentID < 0)
                focusingContentID =
                    curFocusingContentID == ListContentProvider.NO_CONTENT_ID
                        ? 0
                        : Mathf.Min(curFocusingContentID, numOfContents - 1);

            _recalculateAllBoxContentFunc(focusingContentID);
        }

        public IListBox GetFocusingBox() => _focusingBox;

        /// <summary>
        /// Update the multiple boxes in their position order
        /// </summary>
        /// <param name="boxes">The boxes to be updated</param>
        /// <param name="positionState">The position state of these boxes</param>
        private void UpdateBoxesInOrder(
            List<IListBox> boxes, BoxPositionState positionState)
        {
            if (positionState == BoxPositionState.Nothing)
                return;

            switch (positionState) {
                case BoxPositionState.JumpToTop:
                    // The lower one is updated first
                    boxes.Sort((a, b) =>
                        a.GetPositionFactor().CompareTo(b.GetPositionFactor()));

                    foreach (var box in boxes) {
                        var contentID =
                            _contentProvider.GetContentIDByNextBox(
                                box.NextListBox.ContentID);
                        box.PushToBack();
                        UpdateBoxContent(box, contentID);
                    }

                    break;

                case BoxPositionState.JumpToBottom:
                    // The higher one is updated first
                    boxes.Sort((a, b) =>
                        b.GetPositionFactor().CompareTo(a.GetPositionFactor()));

                    foreach (var box in boxes) {
                        var contentID =
                            _contentProvider.GetContentIDByLastBox(
                                box.LastListBox.ContentID);
                        box.PushToBack();
                        UpdateBoxContent(box, contentID);
                    }

                    break;
            }
        }

        #endregion

        #region List State

        /// <summary>
        /// Update the top focusing box
        /// </summary>
        private void UpdateTopFocusingBox()
        {
            var result =
                _focusingBoxFinder.FindForBothEnds(_contentProvider.GetContentCount());
            var topFocusing = result.TopFocusing;
            var topBoxIDState =
                _contentProvider.GetIDState(topFocusing.Box.ContentID);
            var bottomFocusing = result.BottomFocusing;
            var bottomBoxIDState =
                _contentProvider.GetIDState(bottomFocusing.Box.ContentID);
            var distanceOffset =
                bottomBoxIDState.HasFlag(ContentIDState.Last)
                && !topBoxIDState.HasFlag(ContentIDState.First)
                    ? bottomFocusing.DistanceOffset
                    : topFocusing.DistanceOffset;

            UpdateFocusingBox(
                topFocusing.Box, distanceOffset, result.ListFocusingState);
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
            var topFocusing = result.TopFocusing;
            var topBoxIDState =
                _contentProvider.GetIDState(topFocusing.Box.ContentID);
            var bottomFocusing = result.BottomFocusing;
            var bottomBoxIDState =
                _contentProvider.GetIDState(bottomFocusing.Box.ContentID);
            var distanceOffset =
                topBoxIDState.HasFlag(ContentIDState.Last)
                && !bottomBoxIDState.HasFlag(ContentIDState.First)
                    ? topFocusing.DistanceOffset
                    : bottomFocusing.DistanceOffset;

            UpdateFocusingBox(
                bottomFocusing.Box, distanceOffset, result.ListFocusingState);
        }

        /// <summary>
        /// Update the focusing box
        /// </summary>
        private void UpdateFocusingBox(
            IListBox focusingBox, float distanceOffset,
            ListFocusingState listFocusingState)
        {
            ListFocusingState = listFocusingState;
            FocusingDistanceOffset = distanceOffset;

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
        /// Recalculate all the box contents for the middle focusing position
        /// </summary>
        /// <param name="newFocusingContentID">The new focusing content ID</param>
        private void RecalculateAllBoxContentFofMiddle(int newFocusingContentID)
        {
            var numOfBoxes = _boxes.Count;
            var focusingBoxID = _focusingBox.ListBoxID;
            var reverseFactor = _setting.ReverseContentOrder ? -1 : 1;

            foreach (var box in _boxes) {
                var posFactor = box.GetPositionFactor();
                var tempBoxID = box.ListBoxID;

                if (tempBoxID > focusingBoxID && posFactor > 0)
                    tempBoxID -= numOfBoxes;
                else if (tempBoxID < focusingBoxID && posFactor < 0)
                    tempBoxID += numOfBoxes;

                var contentID =
                    newFocusingContentID + (tempBoxID - focusingBoxID) * reverseFactor;
                var newContentID = _contentProvider.GetRefreshedContentID(contentID);
                UpdateBoxContent(box, newContentID);
            }

            _updateFocusingBoxFunc();
        }

        /// <summary>
        /// Recalculate all the box contents for both ends
        /// </summary>
        /// <param name="newFocusingContentID">The new focusing content ID</param>
        private void RecalculateAllBoxContentForBothEnds(int newFocusingContentID)
        {
            // TODO Combine the algorithm with the GetInitialContentID
            // in the ContentProvider
            var tempBoxList = new List<IListBox>(_boxes);
            if (!_setting.ReverseContentOrder)
                tempBoxList.Sort((a, b) =>
                    b.GetPositionFactor().CompareTo(a.GetPositionFactor()));
            else
                tempBoxList.Sort((a, b) =>
                    a.GetPositionFactor().CompareTo(b.GetPositionFactor()));

            var numOfBoxes = tempBoxList.Count;
            var numOfContents = _contentProvider.GetContentCount();

            if (_setting.ListType == CircularScrollingList.ListType.Circular)
                // No need to do content content id adjusting
                newFocusingContentID = newFocusingContentID;
            else if (numOfContents <= numOfBoxes)
                newFocusingContentID = 0;
            else {
                var numOfLackingContents =
                    numOfContents - newFocusingContentID - numOfBoxes;
                if (numOfLackingContents < 0)
                    newFocusingContentID += numOfLackingContents;
            }

            foreach (var box in tempBoxList) {
                var newContentID =
                    _contentProvider.GetRefreshedContentID(newFocusingContentID);
                UpdateBoxContent(box, newContentID);
                ++newFocusingContentID;
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
            // If there has no content in the content provider,
            // just inactivate the box.
            if (idState == ContentIDState.NoContent) {
                box.IsActivated = false;
                return;
            }

            var isPreviouslyActivated = box.IsActivated;
            var isIdValid = idState.HasFlag(ContentIDState.Valid);

            if (!isIdValid && isPreviouslyActivated)
                box.IsActivated = false;
            else if (isIdValid && !isPreviouslyActivated)
                box.IsActivated = true;
        }

        #endregion
    }
}
