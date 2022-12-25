using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.ListStateProcessing;
using UnityEngine;
using UnityEngine.EventSystems;

using Linear = AirFishLab.ScrollingList.ListStateProcessing.Linear;

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
        [Flags]
        public enum ControlMode
        {
            /// <summary>
            /// Control the list by the mouse pointer or finger
            /// </summary>
            Pointer = 1 << 0,
            /// <summary>
            /// Control the list by the mouse wheel
            /// </summary>
            MouseWheel = 1 << 1,
            /// <summary>
            /// All the control modes
            /// </summary>
            Everything = ~(~0 << 2),
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

        public BaseListBank ListBank => _listBank;
        public ListBox[] ListBoxes => _listBoxes.ToArray();
        public CircularScrollingListSetting Setting => _setting;

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
        /// The controlling mode of the list
        /// </summary>
        private ControlMode _controlMode;
        /// <summary>
        /// The component for handing the user input
        /// </summary>
        private InputProcessor _inputProcessor;
        /// <summary>
        /// The component for processing the state of the list
        /// </summary>
        private IListMovementProcessor _listMovementProcessor;
        /// <summary>
        /// The component for managing the list boxes
        /// </summary>
        private IListBoxManager _listBoxManager;
        /// <summary>
        /// The component for providing the content for the boxes
        /// </summary>
        private IListContentProvider _listContentProvider;
        /// <summary>
        /// Is the list initialized?
        /// </summary>
        private bool _isInitialized;
        /// <summary>
        /// Is the list interactable?
        /// </summary>
        private bool _isInteractable = true;
        /// <summary>
        /// Does the list bank has no content?
        /// </summary>
        /// It is used for blocking any input if the list has nothing to display.
        private bool _hasNoContent;
        /// <summary>
        /// Is the list moving?
        /// </summary>
        private bool _isMoving;

        #endregion

        private void Start()
        {
            if (_setting.InitializeOnStart)
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

            GetComponentReference();

            var setupData =
                new ListSetupData(
                    _setting, _rectTransform, _canvasRefCamera,
                    new List<IListBox>(_listBoxes), ListBank);

            InitializeMembers(setupData);
            InitializeComponentsForLinearList(setupData);

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
        /// Initialize the related list members
        /// </summary>
        private void InitializeMembers(ListSetupData setupData)
        {
            var setting = setupData.Setting;
            if (setting.CenterSelectedBox)
                setting.OnBoxClick.AddListener(SelectContentID);
            _controlMode = setting.ControlMode;

            _inputProcessor =
                new InputProcessor(_rectTransform, _canvasRefCamera);
            _listContentProvider = new ListContentProvider();
            _listContentProvider.Initialize(setupData);
            _hasNoContent = _listContentProvider.GetContentCount() == 0;
        }

        /// <summary>
        /// Initialize the components for controlling the linear list
        /// </summary>
        // TODO Move the function to list controller builder
        private void InitializeComponentsForLinearList(ListSetupData setupData)
        {
            var movementProcessor = new Linear.ListMovementProcessor();
            movementProcessor.Initialize(setupData);
            var listBoxManager = new Linear.ListBoxManager();
            listBoxManager.Initialize(setupData, _listContentProvider);

            movementProcessor.SetListBoxManager(listBoxManager);

            _listMovementProcessor = movementProcessor;
            _listBoxManager = listBoxManager;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Whether the list is interactable or not
        /// </summary>
        /// <param name="interactable">Is the list interactable?</param>
        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
        }

        /// <summary>
        /// Move the list one unit up or right
        /// </summary>
        public void MoveOneUnitUp()
        {
            if (_hasNoContent)
                return;

            SetUnitMovement(1);
        }

        /// <summary>
        /// Move the list one unit down or left
        /// </summary>
        public void MoveOneUnitDown()
        {
            if (_hasNoContent)
                return;

            SetUnitMovement(-1);
        }

        /// <summary>
        /// Stop the list immediately
        /// </summary>
        public void EndMovement()
        {
            if (_listMovementProcessor.IsMovementEnded())
                return;

            _listMovementProcessor.EndMovement();
            _setting.OnMovementEnd.Invoke();
            _isMoving = false;
        }

        /// <summary>
        /// Get the box that is closest to the center
        /// </summary>
        public IListBox GetCenteredBox() =>
            _listBoxManager.GetCenteredBox();

        /// <summary>
        /// Get the content ID of the box that is closest to the center
        /// </summary>
        public int GetCenteredContentID() =>
            _listBoxManager.GetCenteredBox().ContentID;

        /// <summary>
        /// Make the boxes recalculate their content ID and reacquire the contents
        /// </summary>
        /// <param name="centeredContentID">
        /// The centered content ID after the list is refreshed
        /// </param>
        public void Refresh(int centeredContentID = -1)
        {
            _listBoxManager.RefreshBoxes(centeredContentID);
            _hasNoContent = _listContentProvider.GetContentCount() == 0;
        }

        /// <summary>
        /// Select the specified content ID and make it be aligned at the center
        /// </summary>
        /// <param name="contentID">The target content ID</param>
        public void SelectContentID(int contentID)
        {
            if (_hasNoContent)
                return;

            if (!_listContentProvider.IsIDValid(contentID))
                throw new IndexOutOfRangeException(
                    $"{nameof(contentID)} is invalid");

            var centeredContentID = GetCenteredContentID();
            SetSelectionMovement(
                _listContentProvider.GetShortestIDDiff(
                    centeredContentID, contentID));
        }

        #endregion

        #region Event System Callback

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_controlMode.HasFlag(ControlMode.Pointer))
                return;

            SetMovement(eventData, InputPhase.Began);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_controlMode.HasFlag(ControlMode.Pointer))
                return;

            SetMovement(eventData, InputPhase.Moved);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_controlMode.HasFlag(ControlMode.Pointer))
                return;

            SetMovement(eventData, InputPhase.Ended);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!_controlMode.HasFlag(ControlMode.MouseWheel))
                return;

            SetMovement(eventData, InputPhase.Scrolled);
        }

        #endregion

        private void Update()
        {
            if (!_isInitialized || !_isMoving)
                return;

            var movementValue = _listMovementProcessor.GetMovement(Time.deltaTime);
            _listBoxManager.UpdateBoxes(movementValue);

            if (!_listMovementProcessor.IsMovementEnded())
                return;

            _setting.OnMovementEnd.Invoke();
            _isMoving = false;
        }

        #region Operation Functions

        /// <summary>
        /// Whether to ignore the movement request or not
        /// </summary>
        private bool ToIgnoreMovement()
        {
            return _hasNoContent || !_isInteractable;
        }

        /// <summary>
        /// Set the movement to the list movement processor
        /// </summary>
        private void SetMovement(PointerEventData eventData, InputPhase phase)
        {
            if (ToIgnoreMovement())
                return;

            var inputInfo = _inputProcessor.GetInputInfo(eventData, phase);
            _listMovementProcessor.SetMovement(inputInfo);
            _isMoving = true;
        }

        /// <summary>
        /// Set the unit movement to the list movement processor
        /// </summary>
        /// <param name="unit">The units to be moved</param>
        private void SetUnitMovement(int unit)
        {
            if (ToIgnoreMovement())
                return;

            _listMovementProcessor.SetUnitMovement(unit);
            _isMoving = true;
        }

        /// <summary>
        /// Set the selection movement to the list movement processor
        /// </summary>
        /// <param name="shortestIDDiff">
        /// The shortest id difference between centered content and the selected content
        /// </param>
        private void SetSelectionMovement(int shortestIDDiff)
        {
            if (ToIgnoreMovement())
                return;

            _listMovementProcessor.SetSelectionMovement(shortestIDDiff);
            _isMoving = true;
        }

        #endregion

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
