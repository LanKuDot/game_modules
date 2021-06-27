﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// Manage and control the circular scrolling list
    /// </summary>
    public class CircularScrollingList : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        #region Enum Definitions

        /// <summary>
        /// The type of the list
        /// </summary>
        public enum ListType
        {
            Circular,
            Linear
        };

        /// <summary>
        /// The controlling mode of the list
        /// </summary>
        public enum ControlMode
        {
            /// <summary>
            /// Control the list by the mouse pointer or finger
            /// </summary>
            Drag,
            /// <summary>
            /// Control the list by invoking functions
            /// </summary>
            Function,
            /// <summary>
            /// Control the list by the mouse wheel
            /// </summary>
            MouseWheel
        };

        /// <summary>
        /// The major moving direction of the list
        /// </summary>
        public enum Direction
        {
            Vertical,
            Horizontal
        };

        #endregion

        #region Settings

        [SerializeField]
        private CircularScrollingListSetting _setting;

        #endregion

        #region Private Members

        /// <summary>
        /// The rect transform that this list belongs to
        /// </summary>
        private RectTransform _rectTransform;
        /// <summary>
        /// The camera that the parent canvas is referenced
        /// </summary>
        private Camera _canvasRefCamera;
        /// <summary>
        /// The component that controlling the position of each list box
        /// </summary>
        private ListPositionCtrl _listPositionCtrl;

        #endregion

        #region Exposed Properties

        public CircularScrollingListSetting setting => _setting;

        #endregion

        private void Start()
        {
            GetComponentReference();
            InitializeListComponents();
        }

        #region Initialization

        /// <summary>
        /// Get the reference of the used component
        /// </summary>
        private void GetComponentReference()
        {
            _rectTransform = GetComponent<RectTransform>();
            var parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _canvasRefCamera = parentCanvas.worldCamera;
        }

        /// <summary>
        /// Initialize the related list components
        /// </summary>
        private void InitializeListComponents()
        {
            _listPositionCtrl =
                new ListPositionCtrl(
                    _setting, _rectTransform, _canvasRefCamera);

            InitializeBoxDependency();

            foreach (var box in _setting.listBoxes)
                box.Initialize(_setting, _listPositionCtrl);
        }

        /// <summary>
        /// Initialize the dependency between the registered boxes
        /// </summary>
        private void InitializeBoxDependency()
        {
            var listBoxes = _setting.listBoxes;
            var numOfBoxes = listBoxes.Count;

            // Set the box ID according to the order in the container `listBoxes`
            for (var i = 0; i < numOfBoxes; ++i)
                listBoxes[i].listBoxID = i;

            // Set the neighbor boxes
            for (var i = 0; i < numOfBoxes; ++i) {
                listBoxes[i].lastListBox =
                    listBoxes[(i - 1 >= 0) ? i - 1 : numOfBoxes - 1];
                listBoxes[i].nextListBox =
                    listBoxes[(i + 1 < numOfBoxes) ? i + 1 : 0];
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Get the box that is closest to the center
        /// </summary>
        public ListBox GetCenteredBox()
        {
            return _listPositionCtrl.GetCenteredBox();
        }

        /// <summary>
        /// Get the content ID of the box that is closest to the center
        /// </summary>
        public int GetCenteredContentID()
        {
            return _listPositionCtrl.GetCenteredContentID();
        }

        /// <summary>
        /// Move the list one unit up or right
        /// </summary>
        public void MoveOneUnitUp()
        {
            _listPositionCtrl.SetUnitMove(1);
        }

        /// <summary>
        /// Move the list one unit down or left
        /// </summary>
        public void MoveOneUnitDown()
        {
            _listPositionCtrl.SetUnitMove(-1);
        }

        #endregion

        #region Event System Callback

        public void OnBeginDrag(PointerEventData eventData)
        {
            _listPositionCtrl.InputPositionHandler(eventData, TouchPhase.Began);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _listPositionCtrl.InputPositionHandler(eventData, TouchPhase.Moved);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _listPositionCtrl.InputPositionHandler(eventData, TouchPhase.Ended);
        }

        public void OnScroll(PointerEventData eventData)
        {
            _listPositionCtrl.ScrollHandler(eventData);
        }

        #endregion

        private void Update()
        {
            _listPositionCtrl.Update();
        }

        private void LateUpdate()
        {
            _listPositionCtrl.LateUpdate();
        }
    }
}
