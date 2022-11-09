using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;

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
            for (var i = 0; i < _numOfBoxes; ++i) {
                var box = _boxes[i];
                box.Initialize(setupData, i);
                var boxId = box.ListBoxID;

                _transformController.SetInitialLocalTransform(box.Transform, boxId);

                var contentID = _contentProvider.GetInitialContentID(boxId);
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
