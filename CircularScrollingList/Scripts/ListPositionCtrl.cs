using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using AirFishLab.ScrollingList.MovementCtrl;

namespace AirFishLab.ScrollingList
{
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

        /// <summary>
        /// The setting of the list
        /// </summary>
        private readonly CircularScrollingListSetting _listSetting;
        /// <summary>
        /// The camera for transforming the point from screen space to local space
        /// </summary>
        private readonly Camera _canvasRefCamera;
        /// <summary>
        /// The rect transform that the list belongs to
        /// </summary>
        private readonly RectTransform _rectTransform;
        /// <summary>
        /// The boxes that belongs to the list
        /// </summary>
        private readonly List<ListBox> _listBoxes;

        #endregion

        #region Private Members

        /// <summary>
        /// The box that is closet to the center
        /// </summary>
        private ListBox _centeredBox;
        /// <summary>
        /// The delegate that returns the x or y value of a vector2 value
        /// </summary>
        private Func<Vector2, float> _getFactor;
        /// <summary>
        /// The delegate for handling the input position of the pointer
        /// </summary>
        /// It is available in the mode of Drag.
        private Action<PointerEventData, TouchPhase> _inputPositionHandler;
        /// <summary>
        /// The delegate for handling the scrolling of the mouse wheel
        /// </summary>
        /// It is available in the mode of Mouse Wheel.
        private Action<Vector2> _scrollHandler;
        /// <summary>
        /// The delegate for handling the scrolling direction
        /// </summary>
        private Action<float> _scrollDirectionHandler;
        /// <summary>
        /// Whether to run LateUpdate
        /// </summary>
        /// This will be false when a movement is ended.
        /// And It becomes true again when the list starts new movement.
        private bool _toRunLateUpdate = true;

        #endregion

        #region Movement Variables

        /// <summary>
        /// The controller for handling the input action and returning the moving distance
        /// </summary>
        private IMovementCtrl _movementCtrl;
        /// <summary>
        /// The local position of the pointer in the last handler call
        /// </summary>
        private Vector2 _lastInputLocalPos;
        /// <summary>
        /// The timestamp of the last valid dragging action
        /// </summary>
        private float _lastDraggingTime;
        /// <summary>
        /// The delta distance of the pointer between last two handler calls
        /// </summary>
        private float _deltaInputDistance;
        /// <summary>
        /// The distance that makes the box which is closest to the center
        /// move to the center
        /// </summary>
        private float _deltaDistanceToCenter;
        /// <summary>
        /// The position state of the list
        /// </summary>
        /// It indicates whether the list reaches the end or not.
        private PositionState _positionState = PositionState.Middle;
        /// <summary>
        /// The allowed number of the disabled boxes for the linear list
        /// </summary>
        private readonly int _maxNumOfDisabledBoxes;
        /// <summary>
        /// The factor for reversing the selection distance
        /// </summary>
        private int _selectionDistanceFactor;
        /// <summary>
        /// The factor for reversing the scrolling direction
        /// </summary>
        private int _scrollFactor;
        /// <summary>
        /// Is the current movement the ending movement?
        /// </summary>
        /// Mainly for indicating the movement status in the dragging mode
        private bool _isEndingMovement;

        #endregion

        #region Exposed Movement Variables

        /// <summary>
        /// The distance of a unit
        /// </summary>
        public float unitPos { get; private set; }
        /// <summary>
        /// The lower bound of the position to move the box to the other end
        /// </summary>
        public float lowerBoundPos { get; private set; }
        /// <summary>
        /// The upper bound of the position to move the box to the other end
        /// </summary>
        public float upperBoundPos { get; private set; }
        /// <summary>
        /// The number of the disabled boxes at the upper side of the list
        /// </summary>
        public int numOfUpperDisabledBoxes { set; get; }
        /// <summary>
        /// The number of the disabled boxes at the lower side of the list
        /// </summary>
        public int numOfLowerDisabledBoxes { set; get; }

        #endregion

        public ListPositionCtrl(
            CircularScrollingListSetting listSetting,
            RectTransform rectTransform,
            Camera canvasRefCamera,
            List<ListBox> listBoxes)
        {
            _listSetting = listSetting;
            _rectTransform = rectTransform;
            _canvasRefCamera = canvasRefCamera;
            _listBoxes = listBoxes;
            _maxNumOfDisabledBoxes = listBoxes.Count / 2;

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

            var numOfBoxes = _listBoxes.Count;

            unitPos = rectLength / (numOfBoxes - 1) / _listSetting.boxDensity;

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
                        _listSetting.boxVelocityCurve,
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
                    _scrollFactor = _listSetting.reverseDirection ? -1 : 1;
                    break;
            }

            // It is ok to set _scrollHandler here without mode checking,
            // because it is only invoked when in mouse scrolling mode.
            switch (_listSetting.direction) {
                case CircularScrollingList.Direction.Vertical:
                    _getFactor = FactorUtility.GetVector2Y;
                    _scrollDirectionHandler = ScrollVertically;
                    break;
                case CircularScrollingList.Direction.Horizontal:
                    _getFactor = FactorUtility.GetVector2X;
                    _scrollDirectionHandler = ScrollHorizontally;
                    break;
            }

            _selectionDistanceFactor = _listSetting.reverseOrder ? -1 : 1;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Sort the image order according to the position of the boxes
        /// </summary>
        public void InitialImageSorting()
        {
            var index = _listBoxes.FindIndex(box => box == _centeredBox);

            for (var i = index - 1; i >= 0; --i)
                _listBoxes[i].PushToBack();
            for (var i = index + 1; i < _listBoxes.Count; ++i)
                _listBoxes[i].PushToBack();
        }

        /// <summary>
        /// Handle the input position event
        /// </summary>
        /// <param name="eventData">The data of the event</param>
        /// <param name="phase">The phase of the input action</param>
        public void InputPositionHandler(PointerEventData eventData, TouchPhase phase)
        {
            _inputPositionHandler(eventData, phase);
            _toRunLateUpdate = true;
        }

        /// <summary>
        /// Handle the scrolling event
        /// </summary>
        /// <param name="eventData">The data of the event</param>
        public void ScrollHandler(PointerEventData eventData)
        {
            _scrollHandler(eventData.scrollDelta);
            _toRunLateUpdate = true;
            _isEndingMovement = true;
        }

        /// <summary>
        /// Set the movement that makes the list align the selected box at the center
        /// </summary>
        /// <param name="idDiff">The difference between two ids</param>
        public void SetSelectionMovement(int idDiff)
        {
            _movementCtrl.SetSelectionMovement(
                _selectionDistanceFactor * idDiff * unitPos
                + _deltaDistanceToCenter);
            _toRunLateUpdate = true;
            _isEndingMovement = true;
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
                    _deltaInputDistance =
                        GetDeltaInputDistance(ScreenToLocalPos(pointer.position));
                    // Slide the list as long as the moving distance of the pointer
                    _movementCtrl.SetMovement(_deltaInputDistance, true);
                    break;

                case TouchPhase.Ended:
                    var deltaTime = Time.realtimeSinceStartup - _lastDraggingTime;
                    _movementCtrl.SetMovement(_deltaInputDistance / deltaTime, false);
                    _isEndingMovement = true;
                    break;
            }

            _lastDraggingTime = Time.realtimeSinceStartup;
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
        private float GetDeltaInputDistance(Vector2 pointerLocalPos)
        {
            var deltaInputDistance =
                _getFactor(pointerLocalPos) - _getFactor(_lastInputLocalPos);

            _lastInputLocalPos = pointerLocalPos;

            return deltaInputDistance;
        }

        /// <summary>
        /// Scroll the list according to the delta of the mouse scrolling
        /// </summary>
        /// <param name="mouseScrollDelta">The delta scrolling distance</param>
        private void ScrollDeltaHandler(Vector2 mouseScrollDelta)
        {
            if (Mathf.Approximately(mouseScrollDelta.y, 0))
                return;

            _scrollDirectionHandler(mouseScrollDelta.y);
        }

        /// <summary>
        /// Scroll the list in vertical direction
        /// </summary>
        private void ScrollVertically(float scrollDelta)
        {
            SetUnitMove(scrollDelta > 0 ? _scrollFactor : -_scrollFactor);
        }

        /// <summary>
        /// Scroll the list in horizontal direction
        /// </summary>
        private void ScrollHorizontally(float scrollDelta)
        {
            SetUnitMove(scrollDelta < 0 ? _scrollFactor : -_scrollFactor);
        }

        #endregion

        #region Update Functions

        /// <summary>
        /// Update the position of the boxes
        /// </summary>
        public void Update()
        {
            if (_movementCtrl.IsMovementEnded())
                return;

            var distance = _movementCtrl.GetDistance(Time.deltaTime);
            foreach (var listBox in _listBoxes)
                listBox.UpdatePosition(distance);
        }

        /// <summary>
        /// Update the position state and find the distance for aligning a box
        /// to the center
        /// </summary>
        public void LateUpdate()
        {
            if (!_toRunLateUpdate)
                return;

            // Update the state of the boxes
            FindDeltaDistanceToCenter();
            if (_listSetting.listType == CircularScrollingList.ListType.Linear)
                UpdatePositionState();

            if (!_movementCtrl.IsMovementEnded())
                return;

            // Not to update the state of box after the last frame of movement
            _toRunLateUpdate = false;

            if (!_isEndingMovement)
                return;

            _isEndingMovement = false;
            _listSetting.onMovementEnd?.Invoke();
        }

        #endregion

        #region Movement Control

        /// <summary>
        /// Find the listBox which is the closest to the center position,
        /// and calculate the delta x or y position between it and the center position.
        /// </summary>
        private void FindDeltaDistanceToCenter()
        {
            var minDeltaDistance = Mathf.Infinity;
            ListBox candidateBox = null;

            foreach (var listBox in _listBoxes) {
                // Skip the disabled box in linear mode
                if (!listBox.isActiveAndEnabled)
                    continue;

                var localPos = listBox.transform.localPosition;
                var deltaDistance = -_getFactor(localPos);

                if (Mathf.Abs(deltaDistance) >= Mathf.Abs(minDeltaDistance))
                    continue;

                minDeltaDistance = deltaDistance;
                candidateBox = listBox;
            }

            _deltaDistanceToCenter = minDeltaDistance;

            if (_centeredBox != candidateBox) {
                _listSetting.onCenteredContentChanged?.Invoke(candidateBox.contentID);
                candidateBox.PopToFront();
            }

            _centeredBox = candidateBox;
        }

        /// <summary>
        /// Move the list for the distance of times of unit position
        /// </summary>
        /// <param name="unit">The number of units</param>
        public void SetUnitMove(int unit)
        {
            _movementCtrl.SetMovement(unit * unitPos, false);
            _toRunLateUpdate = true;
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
        /// Get the box that is closet to the center
        /// </summary>
        public ListBox GetCenteredBox()
        {
            return _centeredBox;
        }

        /// <summary>
        /// Get the content ID of the centered box
        /// </summary>
        /// <returns>The content ID of the centered box</returns>
        public int GetCenteredContentID()
        {
            return GetCenteredBox().contentID;
        }

        #endregion
    }
}
