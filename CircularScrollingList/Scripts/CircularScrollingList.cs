using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.ListStateProcessing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        /// The focusing position of the list
        /// </summary>
        public enum FocusingPosition
        {
            Top,
            Center,
            Bottom,
        }

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
        [Tooltip("The object that stores the contents for the list to display. " +
                 "It should be derived from the class BaseListBank.")]
        private BaseListBank _listBank;
        [SerializeField]
        private ListBoxSetting _boxSetting;
        [SerializeField]
        [Tooltip("The objects that are used for displaying the content. " +
                 "They should be derived from the class ListBox")]
        private List<ListBox> _listBoxes;
        [SerializeField]
        [FormerlySerializedAs("_setting")]
        [Tooltip("The setting of this list")]
        private ListSetting _listSetting;

        #endregion

        #region Exposed Properties

        public BaseListBank ListBank => _listBank;
        public ListBox[] ListBoxes => _listBoxes.ToArray();
        public ListBoxSetting BoxSetting => _boxSetting;
        public ListSetting ListSetting => _listSetting;
        /// <summary>
        /// Is the list interactable?
        /// </summary>
        public bool IsInteractable => _isInteractable;

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
        private IListBoxController _listBoxController;
        /// <summary>
        /// The component for providing the content for the boxes
        /// </summary>
        private ListContentProvider _listContentProvider;
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

        private void Reset()
        {
            if (_boxSetting == null)
                _boxSetting = new ListBoxSetting();
            _boxSetting.BoxRootTransform = transform;
        }

        private void Start()
        {
            if (_listSetting.InitializeOnStart)
                Initialize();
        }

        #region Initialization

        /// <summary>
        /// Set the list bank of the list
        /// </summary>
        public void SetListBank(BaseListBank listBank)
        {
            if (CheckIsInitialized())
                return;

            _listBank = listBank;
        }

        /// <summary>
        /// Initialize the list
        /// </summary>
        public void Initialize()
        {
            if (CheckIsInitialized())
                return;

            Validate();
            _boxSetting.Initialize(gameObject);
            _listSetting.Initialize(_listBank, name);

            GetComponentReference();
            SetListBoxes();
            InitializeMembers();

            _isInitialized = true;
        }

        /// <summary>
        /// Check if the list is initialized
        /// </summary>
        private bool CheckIsInitialized()
        {
            if (_isInitialized)
                Debug.LogWarning($"The list '{name}' is initialized. Skip.");

            return _isInitialized;
        }

        /// <summary>
        /// Validate the setting
        /// </summary>
        private void Validate()
        {
            if (!_listBank)
                throw new UnassignedReferenceException(
                    $"The 'ListBank' is not assigned in the list '{name}'");
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
        private void InitializeMembers()
        {
            if (_listSetting.FocusSelectedBox)
                _listSetting.OnBoxSelected.AddListener(
                    box => SelectContentID(box.ContentID, false));
            _controlMode = _listSetting.ControlMode;

            _inputProcessor =
                new InputProcessor(_rectTransform, _canvasRefCamera);
            _listContentProvider =
                new ListContentProvider(_listSetting, _listBank, _listBoxes.Count);
            _hasNoContent = _listContentProvider.GetContentCount() == 0;

            var setupData =
                new ListSetupData(
                    this, _listSetting, _rectTransform, _canvasRefCamera,
                    new List<IListBox>(_listBoxes), _listContentProvider);
            ListStateProcessorManager.GetProcessors(
                setupData, out _listMovementProcessor, out _listBoxController);
        }

        #endregion

        #region Box Setup Functions

        /// <summary>
        /// Set the list boxes to be used by the list
        /// </summary>
        private void SetListBoxes()
        {
            var prefab = _boxSetting.BoxPrefab;
            var rootTransform = _boxSetting.BoxRootTransform;
            var numOfBoxes = _boxSetting.NumOfBoxes;
#if UNITY_EDITOR
            var undoGroupID = Undo.GetCurrentGroup();
#endif

            var curNumOfBoxes = ReassignListBoxes(_listBoxes, rootTransform, numOfBoxes);
            for (var i = curNumOfBoxes; i < numOfBoxes; ++i) {
                var box = GenerateListBox(prefab, rootTransform, i);
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(
                    box.gameObject, "Generate Boxes and Arrange");
                Undo.CollapseUndoOperations(undoGroupID);
                // TODO Record the change of the list boxes
#endif
                _listBoxes.Add(box);
            }
        }

        /// <summary>
        /// Reassign the list boxes from the box root transform
        /// </summary>
        /// <param name="listBoxes">The container for holding the boxes</param>
        /// <param name="rootTransform">The root transform for finding boxes</param>
        /// <param name="desiredNumOfBoxes">The number of desired boxes</param>
        /// <returns>The number of boxes added</returns>
        private int ReassignListBoxes(
            List<ListBox> listBoxes, Transform rootTransform, int desiredNumOfBoxes)
        {
            var existingBoxes = new List<ListBox>();

            foreach (Transform child in rootTransform) {
                if (!child.TryGetComponent<ListBox>(out var box))
                    continue;
                existingBoxes.Add(box);
            }

            var numOfExistingBoxes = existingBoxes.Count;
            if (numOfExistingBoxes > desiredNumOfBoxes)
                Debug.LogWarning("The number of existing boxes are more than "
                                 + $"the number of desired boxes in the list '{name}'");

            var numOfBoxes = Mathf.Min(numOfExistingBoxes, desiredNumOfBoxes);
            listBoxes.Clear();
            for (var i = 0; i < numOfBoxes; ++i) {
                listBoxes.Add(existingBoxes[i]);
            }

            return numOfBoxes;
        }

        /// <summary>
        /// Generate a list box under the box root transform
        /// </summary>
        /// <param name="prefab">The prefab of the box</param>
        /// <param name="rootTransform">
        /// The root transform for the box to be generated at
        /// </param>
        /// <param name="index">The index of the box</param>
        /// <returns>The generated box</returns>
        private static ListBox GenerateListBox(
            ListBox prefab, Transform rootTransform, int index)
        {
            ListBox box;

#if UNITY_EDITOR
            if (!Application.isPlaying && PrefabUtility.IsPartOfAnyPrefab(prefab)) {
                // If it is the prefab instance, get the source prefab asset
                if (PrefabUtility.IsPartOfPrefabInstance(prefab))
                    prefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
                box = PrefabUtility.InstantiatePrefab(prefab, rootTransform) as ListBox;
            }
            else
#endif
                box = Instantiate(prefab, rootTransform);

            box.name = $"{prefab.name} ({index})";
            return box;
        }

        #endregion

        #region Public Movement Operations

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

            var toAlign = _listMovementProcessor.NeedToAlign();
            _listMovementProcessor.EndMovement(toAlign);

            if (toAlign)
                return;

            _listSetting.OnMovementEnd.Invoke();
            _isMoving = false;
        }

        #endregion

        #region Public Box And Content Operations

        /// <summary>
        /// Get the focusing box
        /// </summary>
        public ListBox GetFocusingBox() =>
            _listBoxController.GetFocusingBox() as ListBox;

        /// <summary>
        /// Get the content ID of the focusing box
        /// </summary>
        public int GetFocusingContentID() =>
            _listBoxController.GetFocusingBox().ContentID;

        /// <summary>
        /// Make the boxes recalculate their content ID and reacquire the contents
        /// </summary>
        /// <param name="focusingContentID">
        /// The focusing content ID after the list is refreshed
        /// If it is negative, it will take current focusing content ID. <para />
        /// If current focusing content ID is larger than the number of contents,
        /// it will be the ID of the last content.
        /// </param>
        public void Refresh(int focusingContentID = -1)
        {
            _hasNoContent = _listContentProvider.GetContentCount() == 0;
            _listBoxController.RefreshBoxes(focusingContentID);
        }

        /// <summary>
        /// Select the specified content ID and make it be aligned at the center
        /// </summary>
        /// <param name="contentID">The target content ID</param>
        /// <param name="notToIgnore">
        /// Not to ignore the selection movement when the list is not interactable
        /// </param>
        public void SelectContentID(int contentID, bool notToIgnore = true)
        {
            if (_hasNoContent)
                return;

            if (!_listContentProvider.IsIDValid(contentID))
                throw new IndexOutOfRangeException(
                    $"'{nameof(contentID)}' is invalid");

            var focusingContentID = GetFocusingContentID();
            var idDiff =
                _listContentProvider.GetShortestIDDiff(focusingContentID, contentID);

            SetSelectionMovement(idDiff, notToIgnore);
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
            _listBoxController.UpdateBoxes(movementValue);

            if (!_listMovementProcessor.IsMovementEnded())
                return;

            _listSetting.OnMovementEnd.Invoke();
            _isMoving = false;
        }

        #region Movement Setup Functions

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
        /// <param name="notToIgnore">Not to ignore this movement</param>
        private void SetSelectionMovement(int shortestIDDiff, bool notToIgnore)
        {
            if (!notToIgnore && ToIgnoreMovement())
                return;

            _listMovementProcessor.SetSelectionMovement(shortestIDDiff);
            _isMoving = true;
        }

        #endregion

        #region Editor Utility
#if UNITY_EDITOR

        /// <summary>
        /// Generate the boxes and arrange them
        /// </summary>
        public void GenerateBoxesAndArrange()
        {
            if (Application.isPlaying)
                return;

            GetComponentReference();
            SetListBoxes();
            // It's ok that the content provider is not created
            var setupData = new ListSetupData(
                this, _listSetting, _rectTransform, _canvasRefCamera,
                new List<IListBox>(_listBoxes), null);
            ListStateProcessorManager.PreviewBoxLayout(setupData);
        }

#endif
        #endregion
    }
}
