using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AirFishLab.ScrollingList
{
    public interface IControlEventHandler :
        IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
    }

    /// <summary>
    /// Control the position of boxes
    /// </summary>
    public class ListPositionCtrl : MonoBehaviour, IControlEventHandler
    {
        #region Enums

        /// <summary>
        /// The state of the position of the list
        /// </summary>
        public enum PositionState
        {
            /// <summary>
            /// The list reaches the top
            /// </summary>
            Top,
            /// <summary>
            /// The list doesn't reach either end
            /// </summary>
            Middle,
            /// <summary>
            /// The list reaches the bottom
            /// </summary>
            Bottom
        };

        #endregion

        #region Settings

        /* List mode */
        [Tooltip("The type of the list.")]
        public CircularScrollingList.ListType listType = CircularScrollingList.ListType.Circular;
        [Tooltip("The controlling mode of the list.")]
        public CircularScrollingList.ControlMode controlMode = CircularScrollingList.ControlMode.Drag;
        [Tooltip("Should a box align in the middle of the list after sliding?")]
        public bool alignMiddle = false;
        [Tooltip("The major moving direction of the list.")]
        public CircularScrollingList.Direction direction = CircularScrollingList.Direction.Vertical;

        /* Containers */
        [Tooltip("The game object which holds the content bank for the list. " +
                 "It will be the derived class of the BaseListBank.")]
        public BaseListBank listBank;
        [Tooltip("Specify the initial content ID for the centered box.")]
        public int centeredContentID = 0;
        [Tooltip("The boxes which belong to this list.")]
        public ListBox[] listBoxes;

        /* Appearance */
        [Tooltip("The distance between each box. The larger, the closer.")]
        public float boxDensity = 2.0f;
        [Tooltip("The curve specifying the box position. " +
                 "The x axis is the major position of the box, which is mapped to [0, 1]. " +
                 "The y axis defines the factor of the passive position of the box. " +
                 "Point (0.5, 0) is the center of the list layout.")]
        public AnimationCurve boxPositionCurve =
            AnimationCurve.Constant(0.0f, 1.0f, 0.0f);
        [Tooltip("The curve specifying the box scale. " +
                 "The x axis is the major position of the box, which is mapped to [0, 1]. " +
                 "The y axis specifies the value of 'localScale' of the box at the " +
                 "corresponding position.")]
        public AnimationCurve boxScaleCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
        [Tooltip("The curve specifying the movement of the box. " +
                 "The x axis is the moving duration in seconds, which starts from 0. " +
                 "The y axis is the factor of the releasing velocity in Drag mode, or " +
                 "the factor of the target position in Function and Mouse Wheel modes.")]
        public AnimationCurve boxMovementCurve = new AnimationCurve(
            new Keyframe(0.0f, 1.0f, 0.0f, -2.5f),
            new Keyframe(1.0f, 0.0f, 0.0f, 0.0f));

        #endregion

        #region Events

        [Tooltip("The callbacks for the event of the clicking on boxes." +
                 "The registered callbacks will be added to the 'onClick' event of boxes, " +
                 "therefore, boxes should be 'Button's.")]
        public ListBoxClickEvent onBoxClick;

        #endregion

        /// <summary>
        /// The camera for transforming the point from screen space to local space
        /// </summary>
        private Camera _canvasRefCamera;
        /// <summary>
        /// The rect transform that this list belongs to
        /// </summary>
        private RectTransform _rectTransform;
        public float unitPos { get; private set; }
        public float lowerBoundPos { get; private set; }
        public float upperBoundPos { get; private set; }

        // Delegate functions
        private Action<PointerEventData, TouchPhase> _inputPositionHandler;
        private Action<Vector2> _scrollHandler;

        // Variables for moving listBoxes
        private IMovementCtrl _movementCtrl;
        // Input mouse/finger position in the local space of the list.
        private Vector2 _lastInputPos;
        private float _deltaInputPos;
        private float _deltaDistanceToCenter = 0.0f;

        // Variables for linear mode
        private PositionState _positionState = PositionState.Middle;
        public int numOfUpperDisabledBoxes { set; get; }
        public int numOfLowerDisabledBoxes { set; get; }
        private int _maxNumOfDisabledBoxes = 0;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            InitializePositionVars();
            InitializeInputFunction();
            InitializeBoxDependency();
            _maxNumOfDisabledBoxes = listBoxes.Length / 2;
            foreach (var listBox in listBoxes)
                listBox.Initialize(this);
        }

        #region Initialization

        /// <summary>
        /// Initialize the position related controlling variables
        /// </summary>
        private void InitializePositionVars()
        {
            // Get the range of the rect transform that this list belongs to
            var rectRange = _rectTransform.rect;
            var rectLength = 0f;

            switch (direction) {
                case CircularScrollingList.Direction.Vertical:
                    rectLength = rectRange.height;
                    break;
                case CircularScrollingList.Direction.Horizontal:
                    rectLength = rectRange.width;
                    break;
            }

            unitPos = rectLength / (listBoxes.Length - 1);

            // If there are even number of ListBoxes, narrow the boundary for 1 unitPos.
            var boundPosAdjust =
                ((listBoxes.Length & 0x1) == 0) ? unitPos / 2 : 0;

            lowerBoundPos = unitPos * (-1 * listBoxes.Length / 2 - 1) + boundPosAdjust;
            upperBoundPos = unitPos * (listBoxes.Length / 2 + 1) - boundPosAdjust;
        }

        /// <summary>
        /// Initialize the dependency between the registered boxes
        /// </summary>
        private void InitializeBoxDependency()
        {
            // Set the box ID according to the order in the container `listBoxes`
            for (var i = 0; i < listBoxes.Length; ++i)
                listBoxes[i].listBoxID = i;

            // Set the neighbor boxes
            for (var i = 0; i < listBoxes.Length; ++i) {
                listBoxes[i].lastListBox =
                    listBoxes[(i - 1 >= 0) ? i - 1 : listBoxes.Length - 1];
                listBoxes[i].nextListBox =
                    listBoxes[(i + 1 < listBoxes.Length) ? i + 1 : 0];
            }
        }

        /// <summary>
        /// Initialize the corresponding handlers for the selected controlling mode
        /// </summary>
        /// The unused handler will be assigned a dummy function to
        /// prevent the handling of the event.
        private void InitializeInputFunction()
        {
            float GetAligningDistance() => _deltaDistanceToCenter;
            PositionState GetPositionState() => _positionState;

            var overGoingThreshold = unitPos * 0.3f;

            switch (controlMode) {
                case CircularScrollingList.ControlMode.Drag:
                    _movementCtrl = new FreeMovementCtrl(
                        boxMovementCurve, alignMiddle, overGoingThreshold,
                        GetAligningDistance, GetPositionState);
                    _inputPositionHandler = DragPositionHandler;
                    _scrollHandler = v => { };
                    break;

                case CircularScrollingList.ControlMode.Function:
                    _movementCtrl = new UnitMovementCtrl(
                        boxMovementCurve, overGoingThreshold,
                        GetAligningDistance, GetPositionState);
                    _inputPositionHandler = (pointer, phase) => { };
                    _scrollHandler = v => { };
                    break;

                case CircularScrollingList.ControlMode.MouseWheel:
                    _movementCtrl = new UnitMovementCtrl(
                        boxMovementCurve, overGoingThreshold,
                        GetAligningDistance, GetPositionState);
                    _inputPositionHandler = (pointer, phase) => { };
                    _scrollHandler = ScrollDeltaHandler;
                    break;
            }

            var parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _canvasRefCamera = parentCanvas.worldCamera;
        }

        #endregion

        #region Event System Callback

        public void OnBeginDrag(PointerEventData pointer)
        {
            _inputPositionHandler(pointer, TouchPhase.Began);
        }

        public void OnDrag(PointerEventData pointer)
        {
            _inputPositionHandler(pointer, TouchPhase.Moved);
        }

        public void OnEndDrag(PointerEventData pointer)
        {
            _inputPositionHandler(pointer, TouchPhase.Ended);
        }

        public void OnScroll(PointerEventData pointer)
        {
            _scrollHandler(pointer.scrollDelta);
        }

        #endregion

        #region Input Value Handler

        /// <summary>
        /// Move the list according to the dragging position and the dragging state
        /// </summary>
        /// <param name="pointer">The information of the pointer</param>
        /// <param name="state">The dragging state</param>
        private void DragPositionHandler(PointerEventData pointer, TouchPhase state)
        {
            switch (state) {
                case TouchPhase.Began:
                    _lastInputPos = ScreenToLocalPos(pointer.position);
                    break;

                case TouchPhase.Moved:
                    _deltaInputPos = GetDeltaInputPos(
                        ScreenToLocalPos(pointer.position));
                    // Slide the list as long as the moving distance of the pointer
                    _movementCtrl.SetMovement(_deltaInputPos, true);
                    break;

                case TouchPhase.Ended:
                    _movementCtrl.SetMovement(_deltaInputPos / Time.deltaTime, false);
                    break;
            }
        }

        /// <summary>
        /// Scroll the list according to the delta of the mouse scrolling
        /// </summary>
        /// <param name="mouseScrollDelta">The delta scrolling distance</param>
        private void ScrollDeltaHandler(Vector2 mouseScrollDelta)
        {
            switch (direction) {
                case CircularScrollingList.Direction.Vertical:
                    if (mouseScrollDelta.y > 0)
                        MoveOneUnitUp();
                    else if (mouseScrollDelta.y < 0)
                        MoveOneUnitDown();
                    break;

                case CircularScrollingList.Direction.Horizontal:
                    if (mouseScrollDelta.y > 0)
                        MoveOneUnitDown();
                    else if (mouseScrollDelta.y < 0)
                        MoveOneUnitUp();
                    break;
            }
        }

        /// <summary>
        /// Transform the point in the screen space to the point in the
        /// space of the local rect transform
        /// </summary>
        private Vector2 ScreenToLocalPos(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, screenPos, _canvasRefCamera, out var localPos);

            return localPos;
        }

        /// <summary>
        /// Get the delta position of the pointer in the local space
        /// </summary>
        /// <param name="pointerLocalPos">
        /// The position of the pointer in local space
        /// </param>
        private float GetDeltaInputPos(Vector2 pointerLocalPos)
        {
            var deltaLocalPos = 0f;

            switch (direction) {
                case CircularScrollingList.Direction.Vertical:
                    deltaLocalPos = pointerLocalPos.y - _lastInputPos.y;
                    break;
                case CircularScrollingList.Direction.Horizontal:
                    deltaLocalPos = pointerLocalPos.x - _lastInputPos.x;
                    break;
            }

            _lastInputPos = pointerLocalPos;

            return deltaLocalPos;
        }

        #endregion

        private void Update()
        {
            // Update the position of boxes
            if (_movementCtrl.IsMovementEnded())
                return;

            var distance = _movementCtrl.GetDistance(Time.deltaTime);
            foreach (var listBox in listBoxes)
                listBox.UpdatePosition(distance);
        }

        private void LateUpdate()
        {
            // Update the state of the boxes
            FindDeltaDistanceToCenter();
            if (listType == CircularScrollingList.ListType.Linear)
                UpdatePositionState();
        }

        #region Movement Control

        /* Find the listBox which is the closest to the center position,
         * and calculate the delta x or y position between it and the center position.
         */
        private void FindDeltaDistanceToCenter()
        {
            var minDeltaPos = Mathf.Infinity;
            var deltaPos = 0.0f;

            switch (direction) {
                case CircularScrollingList.Direction.Vertical:
                    foreach (var listBox in listBoxes) {
                        // Skip the disabled box in linear mode
                        if (!listBox.isActiveAndEnabled)
                            continue;

                        deltaPos = -listBox.transform.localPosition.y;
                        if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
                            minDeltaPos = deltaPos;
                    }

                    break;

                case CircularScrollingList.Direction.Horizontal:
                    foreach (var listBox in listBoxes) {
                        // Skip the disabled box in linear mode
                        if (!listBox.isActiveAndEnabled)
                            continue;

                        deltaPos = -listBox.transform.localPosition.x;
                        if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
                            minDeltaPos = deltaPos;
                    }

                    break;
            }

            _deltaDistanceToCenter = minDeltaPos;
        }

        /* Move the list for the distance of times of unit position
         */
        private void SetUnitMove(int unit)
        {
            _movementCtrl.SetMovement(unit * unitPos, false);
        }

        /// <summary>
        /// Move all boxes 1 unit up
        /// </summary>
        public void MoveOneUnitUp()
        {
            SetUnitMove(1);
        }

        /// <summary>
        /// Move all boxes 1 unit down
        /// </summary>
        public void MoveOneUnitDown()
        {
            SetUnitMove(-1);
        }

        /// <summary>
        /// Check if the list reaches the end and update the position state
        /// </summary>
        private void UpdatePositionState()
        {
            if (numOfUpperDisabledBoxes >= _maxNumOfDisabledBoxes &&
                _deltaDistanceToCenter > -1e-4)
                _positionState = PositionState.Top;
            else if (numOfLowerDisabledBoxes >= _maxNumOfDisabledBoxes &&
                     _deltaDistanceToCenter < 1e-4)
                _positionState = PositionState.Bottom;
            else
                _positionState = PositionState.Middle;
        }

        #endregion

        #region Center Box Searching

        /// <summary>
        /// Get the centered ListBox<para />
        /// The centered ListBox is found by comparing which one is the closest
        /// to the center.
        /// </summary>
        /// <returns>The ListBox</returns>
        public ListBox GetCenteredBox()
        {
            var minPosition = Mathf.Infinity;
            ListBox candidateBox = null;

            bool IsCloser(Vector3 localPos)
            {
                var value =
                    Mathf.Abs(direction == CircularScrollingList.Direction.Horizontal
                        ? localPos.x
                        : localPos.y);

                if (value < minPosition) {
                    minPosition = value;
                    return true;
                }

                return false;
            }

            foreach (var listBox in listBoxes) {
                if (IsCloser(listBox.transform.localPosition))
                    candidateBox = listBox;
            }

            return candidateBox;
        }

        /// <summary>
        /// Get the content ID of the centered box
        /// </summary>
        /// <returns>The content ID of the centered box</returns>
        public int GetCenteredContentID()
        {
            return GetCenteredBox().GetContentID();
        }

        #endregion
    }
}
