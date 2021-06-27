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
    public class ListPositionCtrl
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

        #region Referenced Components

        private readonly CircularScrollingListSetting _listSetting;
        /// <summary>
        /// The camera for transforming the point from screen space to local space
        /// </summary>
        private readonly Camera _canvasRefCamera;
        private readonly RectTransform _rectTransform;

        #endregion

        #region Handler Delegate

        private Action<PointerEventData, TouchPhase> _inputPositionHandler;
        private Action<Vector2> _scrollHandler;

        #endregion


        #region Movement Variables

        private IMovementCtrl _movementCtrl;
        private Vector2 _lastInputLocalPos;
        private float _deltaInputLocalPos;
        private float _deltaDistanceToCenter;
        private PositionState _positionState = PositionState.Middle;
        private readonly int _maxNumOfDisabledBoxes;

        #endregion

        #region Exposed Movement Variables

        public float unitPos { get; private set; }
        public float lowerBoundPos { get; private set; }
        public float upperBoundPos { get; private set; }
        public int numOfUpperDisabledBoxes { set; get; }
        public int numOfLowerDisabledBoxes { set; get; }

        #endregion

        public ListPositionCtrl(
            CircularScrollingListSetting listSetting,
            RectTransform rectTransform, Camera canvasRefCamera)
        {
            _listSetting = listSetting;
            _rectTransform = rectTransform;
            _canvasRefCamera = canvasRefCamera;
            _maxNumOfDisabledBoxes = _listSetting.listBoxes.Count / 2;

            InitializePositionVars();
            InitializeInputFunction();
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

            switch (_listSetting.direction) {
                case CircularScrollingList.Direction.Vertical:
                    rectLength = rectRange.height;
                    break;
                case CircularScrollingList.Direction.Horizontal:
                    rectLength = rectRange.width;
                    break;
            }

            var numOfBoxes = _listSetting.listBoxes.Count;

            unitPos = rectLength / (numOfBoxes - 1);

            // If there are even number of ListBoxes, narrow the boundary for 1 unitPos.
            var boundPosAdjust =
                ((numOfBoxes & 0x1) == 0) ? unitPos / 2 : 0;

            lowerBoundPos = unitPos * (-1 * numOfBoxes / 2 - 1) + boundPosAdjust;
            upperBoundPos = unitPos * (numOfBoxes / 2 + 1) - boundPosAdjust;
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

            switch (_listSetting.controlMode) {
                case CircularScrollingList.ControlMode.Drag:
                    _movementCtrl = new FreeMovementCtrl(
                        _listSetting.boxMovementCurve,
                        _listSetting.alignMiddle,
                        overGoingThreshold,
                        GetAligningDistance, GetPositionState);
                    _inputPositionHandler = DragPositionHandler;
                    _scrollHandler = v => { };
                    break;

                case CircularScrollingList.ControlMode.Function:
                    _movementCtrl = new UnitMovementCtrl(
                        _listSetting.boxMovementCurve,
                        overGoingThreshold,
                        GetAligningDistance, GetPositionState);
                    _inputPositionHandler = (pointer, phase) => { };
                    _scrollHandler = v => { };
                    break;

                case CircularScrollingList.ControlMode.MouseWheel:
                    _movementCtrl = new UnitMovementCtrl(
                        _listSetting.boxMovementCurve,
                        overGoingThreshold,
                        GetAligningDistance, GetPositionState);
                    _inputPositionHandler = (pointer, phase) => { };
                    _scrollHandler = ScrollDeltaHandler;
                    break;
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Handle the input position event
        /// </summary>
        /// <param name="eventData">The data of the event</param>
        /// <param name="phase">The phase of the input action</param>
        public void InputPositionHandler(PointerEventData eventData, TouchPhase phase)
        {
            _inputPositionHandler(eventData, phase);
        }

        /// <summary>
        /// Handle the scrolling event
        /// </summary>
        /// <param name="eventData">The data of the event</param>
        public void ScrollHandler(PointerEventData eventData)
        {
            _scrollHandler(eventData.scrollDelta);
        }

        #endregion

        #region Input Value Handlers

        /// <summary>
        /// Move the list according to the dragging position and the dragging state
        /// </summary>
        /// <param name="pointer">The information of the pointer</param>
        /// <param name="state">The dragging state</param>
        private void DragPositionHandler(PointerEventData pointer, TouchPhase state)
        {
            switch (state) {
                case TouchPhase.Began:
                    _lastInputLocalPos = ScreenToLocalPos(pointer.position);
                    break;

                case TouchPhase.Moved:
                    _deltaInputLocalPos = GetDeltaInputPos(
                        ScreenToLocalPos(pointer.position));
                    // Slide the list as long as the moving distance of the pointer
                    _movementCtrl.SetMovement(_deltaInputLocalPos, true);
                    break;

                case TouchPhase.Ended:
                    _movementCtrl.SetMovement(_deltaInputLocalPos / Time.deltaTime, false);
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

            switch (_listSetting.direction) {
                case CircularScrollingList.Direction.Vertical:
                    deltaLocalPos = pointerLocalPos.y - _lastInputLocalPos.y;
                    break;
                case CircularScrollingList.Direction.Horizontal:
                    deltaLocalPos = pointerLocalPos.x - _lastInputLocalPos.x;
                    break;
            }

            _lastInputLocalPos = pointerLocalPos;

            return deltaLocalPos;
        }

        /// <summary>
        /// Scroll the list according to the delta of the mouse scrolling
        /// </summary>
        /// <param name="mouseScrollDelta">The delta scrolling distance</param>
        private void ScrollDeltaHandler(Vector2 mouseScrollDelta)
        {
            if (Mathf.Approximately(mouseScrollDelta.y, 0))
                return;

            switch (_listSetting.direction) {
                case CircularScrollingList.Direction.Vertical:
                    SetUnitMove(mouseScrollDelta.y > 0 ? 1 : -1);
                    break;

                case CircularScrollingList.Direction.Horizontal:
                    SetUnitMove(mouseScrollDelta.y < 0 ? 1 : -1);
                    break;
            }
        }

        #endregion

        #region Update Functions

        /// <summary>
        /// Update the position of the boxes
        /// </summary>
        public void Update()
        {
            // Update the position of boxes
            if (_movementCtrl.IsMovementEnded())
                return;

            var distance = _movementCtrl.GetDistance(Time.deltaTime);
            foreach (var listBox in _listSetting.listBoxes)
                listBox.UpdatePosition(distance);
        }

        /// <summary>
        /// Update the position state and find the distance for aligning a box
        /// to the center
        /// </summary>
        public void LateUpdate()
        {
            // Update the state of the boxes
            FindDeltaDistanceToCenter();
            if (_listSetting.listType == CircularScrollingList.ListType.Linear)
                UpdatePositionState();
        }

        #endregion

        #region Movement Control

        /// <summary>
        /// Find the listBox which is the closest to the center position,
        /// and calculate the delta x or y position between it and the center position.
        /// </summary>
        private void FindDeltaDistanceToCenter()
        {
            var minDeltaPos = Mathf.Infinity;
            var deltaPos = 0.0f;

            switch (_listSetting.direction) {
                case CircularScrollingList.Direction.Vertical:
                    foreach (var listBox in _listSetting.listBoxes) {
                        // Skip the disabled box in linear mode
                        if (!listBox.isActiveAndEnabled)
                            continue;

                        deltaPos = -listBox.transform.localPosition.y;
                        if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
                            minDeltaPos = deltaPos;
                    }

                    break;

                case CircularScrollingList.Direction.Horizontal:
                    foreach (var listBox in _listSetting.listBoxes) {
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

        /// <summary>
        /// Move the list for the distance of times of unit position
        /// </summary>
        /// <param name="unit">The number of units</param>
        public void SetUnitMove(int unit)
        {
            _movementCtrl.SetMovement(unit * unitPos, false);
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
                    Mathf.Abs(
                        _listSetting.direction
                        == CircularScrollingList.Direction.Horizontal
                            ? localPos.x
                            : localPos.y);

                if (value < minPosition) {
                    minPosition = value;
                    return true;
                }

                return false;
            }

            foreach (var listBox in _listSetting.listBoxes) {
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
