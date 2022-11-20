﻿using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    using PositionStatus = BoxTransformController.PositionStatus;

    public class ListBoxManager : IListBoxManager
    {
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
        /// The box which is closest to the center position
        /// </summary>
        private IListBox _centeredBox;
        /// <summary>
        /// The shortest distance to make a box at the center position of the list
        /// </summary>
        private float _shortestDistanceToCenter;
        /// <summary>
        /// The component fot getting the list contents
        /// </summary>
        private IListContentProvider _contentProvider;
        /// <summary>
        /// The controller for setting the transform of the boxes
        /// </summary>
        private BoxTransformController _transformController;
        /// <summary>
        /// The function for getting the major factor from the vector2
        /// </summary>
        private Func<Vector2, float> _getMajorFactorFunc;

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
        private void UpdateBoxContent(IListBox box, PositionStatus positionStatus)
        {
            if (positionStatus == PositionStatus.Nothing)
                return;

            var contentID = 0;
            switch (positionStatus) {
                case PositionStatus.JumpToTop:
                    contentID =
                        _contentProvider.GetContentIDByNextBox(
                            box.NextListBox.ContentID);
                    box.PushToBack();
                    break;
                case PositionStatus.JumpToBottom:
                    contentID =
                        _contentProvider.GetContentIDByLastBox(
                            box.LastListBox.ContentID);
                    box.PushToBack();
                    break;
            }

            SetBoxContent(box, contentID);
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
    }
}
