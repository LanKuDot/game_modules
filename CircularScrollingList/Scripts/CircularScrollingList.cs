using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <summary>
        /// Is the list initialized?
        /// </summary>
        private bool _isInitialized;
        /// <summary>
        /// Does the list bank has no content?
        /// </summary>
        /// It is used for blocking any input if the list has nothing to display.
        private bool _hasNoContent;

        #endregion

        private void Awake()
        {
            GetComponentReference();
        }

        private void Start()
        {
            if (_setting.initializeOnStart)
                Initialize();
        }

        #region Initialization

        /// <summary>
        /// Initialize the list
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            InitializeListComponents();
            // Make the list position ctrl initialize its position state
            _listPositionCtrl.LateUpdate();
            _listPositionCtrl.InitialImageSorting();
            _isInitialized = true;
        }

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

            _hasNoContent = _listBank.GetListLength() == 0;
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
            if (_hasNoContent)
                return;

            _listPositionCtrl.SetUnitMove(1);
        }

        /// <summary>
        /// Move the list one unit down or left
        /// </summary>
        public void MoveOneUnitDown()
        {
            if (_hasNoContent)
                return;

            _listPositionCtrl.SetUnitMove(-1);
        }

        /// <summary>
        /// Make the boxes recalculate their content ID and
        /// reacquire the contents from the bank
        /// </summary>
        /// If the specified <c cref="centeredContentID">centeredContentID</c> is negative,
        /// it will take current centered content ID. <para />
        /// If current centered content ID is int.MinValue, it will be 0. <para />
        /// If current centered content ID is larger than the number of contents,
        /// it will be the ID of the last content.
        /// <param name="centeredContentID">
        /// The centered content ID after the list is refreshed
        /// </param>
        public void Refresh(int centeredContentID = -1)
        {
            var centeredBox = _listPositionCtrl.GetCenteredBox();
            var numOfContents = _listBank.GetListLength();

            if (centeredContentID < 0)
                centeredContentID =
                    centeredBox.contentID == int.MinValue
                        ? 0
                        : Mathf.Min(centeredBox.contentID, numOfContents - 1);
            else if (centeredContentID >= numOfContents)
                throw new IndexOutOfRangeException(
                    $"{nameof(centeredContentID)} is larger than the number of contents");

            _listPositionCtrl.numOfLowerDisabledBoxes = 0;
            _listPositionCtrl.numOfUpperDisabledBoxes = 0;

            foreach (var listBox in _listBoxes)
                listBox.Refresh(centeredBox.listBoxID, centeredContentID);

            _hasNoContent = numOfContents == 0;
        }

        /// <summary>
        /// Select the specified content ID and make it be aligned at the center
        /// </summary>
        /// <param name="contentID">The target content ID</param>
        public void SelectContentID(int contentID)
        {
            if (_hasNoContent)
                return;

            if (!_listContentManager.IsIDValid(contentID))
                throw new IndexOutOfRangeException(
                    $"{nameof(contentID)} is larger than the number of contents");

            var centeredBox = _listPositionCtrl.GetCenteredBox();
            var centeredContentID = centeredBox.contentID;
            _listPositionCtrl.SetSelectionMovement(
                _listContentManager.GetShortestDiff(centeredContentID, contentID));
        }

        #endregion

        #region Event System Callback

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_hasNoContent)
                return;

            _listPositionCtrl.InputPositionHandler(eventData, TouchPhase.Began);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_hasNoContent)
                return;

            _listPositionCtrl.InputPositionHandler(eventData, TouchPhase.Moved);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_hasNoContent)
                return;

            _listPositionCtrl.InputPositionHandler(eventData, TouchPhase.Ended);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_hasNoContent)
                return;

            _listPositionCtrl.ScrollHandler(eventData);
        }

        #endregion

        private void Update()
        {
            if (!_isInitialized)
                return;

            _listPositionCtrl.Update();
        }

        private void LateUpdate()
        {
            if (!_isInitialized)
                return;

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
