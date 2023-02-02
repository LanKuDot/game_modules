using System;
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
        /// The component fot getting the list contents
        /// </summary>
        private ListContentProvider _contentProvider;

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

        #endregion

        #region IListBoxManager

        public void Initialize(ListSetupData setupData)
        {
            _setting = setupData.ListSetting;
            _boxes.Clear();
            _boxes.AddRange(setupData.ListBoxes);
            _contentProvider = setupData.ListContentProvider;

            InitializeFactorFunc(_setting.Direction);
            InitializeBoxes(setupData);
        }

        public void UpdateBoxes(float movementValue)
        {
            if (Mathf.Approximately(movementValue, 0f))
                return;

            foreach (var box in _boxes) {
                var positionStatus = box.UpdateTransform(movementValue);

                var contentID = box.ContentID;
                switch (positionStatus) {
                    case BoxPositionState.Nothing:
                        continue;
                    case BoxPositionState.JumpToTop:
                        contentID =
                            _contentProvider.GetContentIDByNextBox(
                                box.NextListBox.ContentID);
                        break;
                    case BoxPositionState.JumpToBottom:
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
        private void InitializeBoxes(ListSetupData setupData)
        {
            var transformController = new BoxTransformController(setupData);
            var numOfBoxes = _boxes.Count;
            for (var boxID = 0; boxID < numOfBoxes; ++boxID) {
                var box = _boxes[boxID];
                var lastListBox =
                    _boxes[(int)Mathf.Repeat(boxID - 1, numOfBoxes)];
                var nextListBox =
                    _boxes[(int)Mathf.Repeat(boxID + 1, numOfBoxes)];
                box.Initialize(
                    setupData.ScrollingList, transformController,
                    boxID, lastListBox, nextListBox);
                box.OnBoxClick.AddListener(_setting.OnBoxClick.Invoke);

                var contentID = _contentProvider.GetInitialContentID(boxID);
                UpdateBoxContent(box, contentID, BoxPositionState.Nothing);
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
            if (_setting.ListType != CircularScrollingList.ListType.Linear)
                return;

            const float tolerance = 1e-4f;
            var centeredContentID = GetCenteredBox().ContentID;
            var contentCount = _contentProvider.GetContentCount();
            var isReversed = _setting.ReverseContentOrder;
            var isFirstContent = centeredContentID == 0;
            var isLastContent = centeredContentID == contentCount - 1;

            if (!(isFirstContent || isLastContent))
                ListFocusingState = ListFocusingState.Middle;
            else if (isReversed ^ isFirstContent
                     && ShortestDistanceToCenter > -tolerance)
                ListFocusingState = ListFocusingState.Top;
            else if (isReversed ^ isLastContent
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

            foreach (var box in _boxes) {
                // Skip the inactivated box
                // But if there has no content to display,
                // it is still need to find the centered box,
                // cause all the boxes are inactivated.
                if (!box.IsActivated
                    && box.ContentID != ListContentProvider.NO_CONTENT_ID)
                    continue;

                var localPos = box.GetPosition();
                var deltaDistance = -_getMajorFactorFunc(localPos);

                if (Mathf.Abs(deltaDistance) >= Mathf.Abs(distance))
                    continue;

                distance = deltaDistance;
                candidateBox = box;
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
            _setting.OnCenteredContentChanged.Invoke(candidateBox.ContentID);
            _setting.OnCenteredBoxChanged.Invoke(
                (ListBox)_centeredBox, (ListBox)candidateBox);
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
            var numOfBoxes = _boxes.Count;
            var centeredBoxID = _centeredBox.ListBoxID;
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
                UpdateBoxContent(box, newContentID, BoxPositionState.Nothing);
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
            IListBox box, int contentID, BoxPositionState positionState)
        {
            if (positionState != BoxPositionState.Nothing)
                box.PushToBack();

            box.SetContentID(contentID);
            if (_contentProvider.TryGetContent(contentID, out var content))
                box.SetContent(content);
            var idState = _contentProvider.GetIDState(contentID);
            ToggleBoxActivation(box, idState);
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
