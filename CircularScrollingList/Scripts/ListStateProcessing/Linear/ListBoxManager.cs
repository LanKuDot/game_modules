using System.Collections.Generic;
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
        /// <summary>
        /// The controller for setting the transform of the boxes
        /// </summary>
        private BoxTransformController _transformController;

        #endregion

        #region IListBoxManager

        public void Initialize(ListSetupData setupData)
        {
            _boxes.Clear();
            _boxes.AddRange(setupData.ListBoxes);
            _numOfBoxes = _boxes.Count;

            _transformController = new BoxTransformController(setupData);
        }

        public void InitializeBoxes()
        {
            for (var i = 0; i < _numOfBoxes; ++i)
                _transformController.SetInitialLocalTransform(
                    _boxes[i].transform, i);
        }

        public void UpdateBoxes(float movementValue)
        {
            for (var i = 0; i< _numOfBoxes; ++i)
                _transformController.SetLocalTransform(
                    _boxes[i].transform, movementValue);
        }

        #endregion
    }
}
