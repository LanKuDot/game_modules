using System;
using System.Collections.Generic;
using UnityEditor;
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
        [Tooltip("The game object that stores the contents for the list to display. " +
                 "It should be derived from the class BaseListBank.")]
        private BaseListBank _listBank;
        [SerializeField]
        [Tooltip("The game objects that used for displaying the content. " +
                 "They should be derived from the class ListBox")]
        private List<ListBox> _listBoxes;
        [SerializeField]
        [Tooltip("The setting of this list")]
        private CircularScrollingListSetting _setting;

        #endregion

        #region Exposed Properties

        public BaseListBank listBank => _listBank;
        public CircularScrollingListSetting setting => _setting;

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
        /// The component that controlling the position of each box
        /// </summary>
        private ListPositionCtrl _listPositionCtrl;
        /// <summary>
        /// The component that controlling the content for each box
        /// </summary>
        private ListContentManager _listContentManager;

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
                    _setting, _rectTransform, _canvasRefCamera, _listBoxes);
            _listContentManager =
                new ListContentManager(
                    _setting, _listBank, _listBoxes.Count);

            if (_setting.centerSelectedBox)
                _setting.onBoxClick.AddListener(SelectContentID);

            for (var i = 0; i < _listBoxes.Count; ++i)
                _listBoxes[i].Initialize(
                    _setting, _listPositionCtrl, _listContentManager,
                    _listBoxes, i);
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

        /// <summary>
        /// Make the boxes recalculate their content ID and
        /// reacquire the contents from bank
        /// </summary>
        public void Refresh()
        {
            _listPositionCtrl.numOfLowerDisabledBoxes = 0;
            _listPositionCtrl.numOfUpperDisabledBoxes = 0;

            var centeredBox = _listPositionCtrl.GetCenteredBox();
            // Make sure that the content ID will not exceed the number of content
            var centeredContentID =
                Mathf.Min(centeredBox.contentID, _listBank.GetListLength() - 1);
            foreach (var listBox in _listBoxes)
                listBox.Refresh(centeredBox.listBoxID, centeredContentID);
        }

        /// <summary>
        /// Select the specified content ID and make it be aligned to the center
        /// </summary>
        /// <param name="contentID">The target content ID</param>
        public void SelectContentID(int contentID)
        {
            Debug.Log(contentID);
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

#if UNITY_EDITOR

        #region Editor Utility

        [ContextMenu("Assign References of Bank and Boxes")]
        private void AssignReferences()
        {
            _listBank = GetComponent<BaseListBank>();
            if (_listBoxes == null)
                _listBoxes = new List<ListBox>();
            else
                _listBoxes.Clear();
            foreach (Transform child in transform) {
                var listBox = child.GetComponent<ListBox>();
                if (listBox)
                    _listBoxes.Add(listBox);
            }
        }

        #endregion

#endif
    }
}
