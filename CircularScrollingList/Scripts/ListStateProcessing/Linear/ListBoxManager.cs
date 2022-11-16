using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    using PositionStatus = BoxTransformController.PositionStatus;

    public class ListBoxManager : IListBoxManager
    {
        #region Private Components

        /// <summary>
        /// The managed boxes
        /// </summary>
        private readonly List<IListBox> _boxes = new List<IListBox>();
        /// <summary>
        /// The component fot getting the list contents
        /// </summary>
        private IListContentProvider _contentProvider;
        /// <summary>
        /// The number of boxes
        /// </summary>
        private int _numOfBoxes;
        /// <summary>
        /// The controller for setting the transform of the boxes
        /// </summary>
        private BoxTransformController _transformController;

        #endregion

        #region IListBoxManager

        public void Initialize(
            ListSetupData setupData, IListContentProvider contentProvider)
        {
            _boxes.Clear();
            _boxes.AddRange(setupData.ListBoxes);
            _numOfBoxes = _boxes.Count;
            _contentProvider = contentProvider;

            _transformController = new BoxTransformController(setupData);

            InitializeBoxes();
        }

        public void UpdateBoxes(float movementValue)
        {
            for (var i = 0; i < _numOfBoxes; ++i) {
                var box = _boxes[i];

                var positionStatus =
                    _transformController.SetLocalTransform(
                        box.Transform, movementValue);
                UpdateBoxContent(box, positionStatus);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialized the boxes
        /// </summary>
        private void InitializeBoxes()
        {
            for (var boxID = 0; boxID < _numOfBoxes; ++boxID) {
                var box = _boxes[boxID];
                var lastListBox =
                    _boxes[(int)Mathf.Repeat(boxID - 1, _numOfBoxes)];
                var nextListBox =
                    _boxes[(int)Mathf.Repeat(boxID + 1, _numOfBoxes)];
                box.Initialize(boxID, lastListBox, nextListBox);

                _transformController.SetInitialLocalTransform(box.Transform, boxID);

                var contentID = _contentProvider.GetInitialContentID(boxID);
                SetBoxContent(box, contentID);
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
                    break;
                case PositionStatus.JumpToBottom:
                    contentID =
                        _contentProvider.GetContentIDByLastBox(
                            box.LastListBox.ContentID);
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
