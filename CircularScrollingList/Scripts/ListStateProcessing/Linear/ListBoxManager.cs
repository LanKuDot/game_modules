using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
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

            InitializeBoxes(setupData);
        }

        public void UpdateBoxes(float movementValue)
        {
            for (var i = 0; i < _numOfBoxes; ++i)
                _transformController.SetLocalTransform(
                    _boxes[i].Transform, movementValue);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialized the boxes
        /// </summary>
        private void InitializeBoxes(ListSetupData setupData)
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
                var content =
                    _contentProvider.TryGetContent(contentID, out var contentReturned)
                        ? contentReturned
                        : null;
                box.SetContent(contentID, content);
            }
        }

        #endregion
    }
}
