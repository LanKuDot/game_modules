using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AirFishLab.ScrollingList
{
    [Serializable]
    public class ListSetting
    {
        #region List Mode

        [SerializeField]
        [Tooltip("The type of the list.")]
        private CircularScrollingList.ListType _listType =
            CircularScrollingList.ListType.Circular;
        [SerializeField]
        [Tooltip("The major moving direction of the list.")]
        private CircularScrollingList.Direction _direction =
            CircularScrollingList.Direction.Vertical;
        [SerializeField]
        [Tooltip("The controlling mode of the list.")]
        private CircularScrollingList.ControlMode _controlMode =
            CircularScrollingList.ControlMode.Everything;
        [SerializeField]
        [Tooltip("The focusing position of the list")]
        private CircularScrollingList.FocusingPosition _focusingPosition =
                CircularScrollingList.FocusingPosition.Center;
        [SerializeField]
        [Tooltip("To show the list contents in the reversed order. "
                 + "Available when the 'FocusingPosition' is 'center'")]
        [FormerlySerializedAs("_reverseOrder")]
        private bool _reverseContentOrder;
        [SerializeField]
        [Tooltip("To align a box at the focusing position after sliding")]
        [FormerlySerializedAs("_alignMiddle")]
        [FormerlySerializedAs("_alignInCenter")]
        private bool _alignAtFocusingPosition;
        [SerializeField]
        [Tooltip("To reverse the scrolling direction")]
        [FormerlySerializedAs("_reverseDirection")]
        private bool _reverseScrollingDirection;
        [SerializeField]
        [Tooltip("Specify the initial content ID at the focusing position")]
        [FormerlySerializedAs(" _centeredContentID")]
        private int _initFocusingContentID;
        [SerializeField]
        [Tooltip("Move the selected box to the focusing position")]
        [FormerlySerializedAs("_centerSelectedBox")]
        private bool _focusSelectedBox;
        [SerializeField]
        [Tooltip("Whether to initialize the list on Start or not. " +
                 "If set to false, manually call Initialize() to initialize the list.")]
        private bool _initializeOnStart = true;

        #endregion

        #region List Appearance

        [SerializeField]
        [Tooltip("The factor that adjusting the distance between boxes. " +
                 "The larger, the closer.")]
        private float _boxDensity = 1.0f;
        [SerializeField]
        [Tooltip("The curve specifying the passive position of the box. " +
                 "The x axis is the major position of the box, which is mapped to [-1, 1]. " +
                 "The y axis defines the factor of the passive position of the box. " +
                 "Point (0, 0) is the center of the list layout.")]
        private AnimationCurve _boxPositionCurve =
            AnimationCurve.Constant(-1.0f, 1.0f, 0.0f);
        [SerializeField]
        [Tooltip("The curve specifying the box scale. " +
                 "The x axis is the major position of the box, which is mapped to [-1, 1]. " +
                 "The y axis specifies the value of 'localScale' of the box at the " +
                 "corresponding position.")]
        private AnimationCurve _boxScaleCurve =
            AnimationCurve.Constant(-1.0f, 1.0f, 1.0f);
        [SerializeField]
        [Tooltip("The curve specifying the velocity factor of the box after releasing. " +
                 "The x axis is the the moving duration in seconds, which starts from 0. " +
                 "The y axis is the factor of releasing velocity.")]
        private AnimationCurve _boxVelocityCurve =
            new AnimationCurve(
                new Keyframe(0.0f, 1.0f, 0.0f, -2.5f),
                new Keyframe(1.0f, 0.0f, 0.0f, 0.0f));
        [SerializeField]
        [Tooltip("The curve specifying the movement factor of the box. " +
                 "The x axis is the moving duration in seconds, which starts from 0. " +
                 "The y axis is the lerping factor for reaching the target position.")]
        private AnimationCurve _boxMovementCurve =
            new AnimationCurve(
                new Keyframe(0.0f, 0.0f, 0.0f, 8f),
                new Keyframe(0.25f, 1.0f, 0.0f, 0.0f));

        #endregion

        #region Events

        [SerializeField]
        [Tooltip("The callback to be invoked when a box is selected. " +
                 "The registered callbacks will be added to the 'onClick' event of boxes, " +
                 "therefore, boxes should be 'Button's.")]
        private ListBoxSelectedEvent _onBoxSelected;
        [SerializeField]
        [Tooltip("The callback to be invoked when the focusing box is changed. " +
                 "The first argument is previous focusing box, " +
                 "and the second one is current focusing box.")]
        [FormerlySerializedAs("_onCenteredBoxChanged")]
        private ListTwoBoxesEvent _onFocusingBoxChanged;
        [SerializeField]
        [Tooltip("The callback to be invoked when the movement is ended")]
        private UnityEvent _onMovementEnd;

        #endregion

        #region Setting Getter

        public CircularScrollingList.ListType ListType => _listType;
        public CircularScrollingList.Direction Direction => _direction;
        public CircularScrollingList.ControlMode ControlMode => _controlMode;
        public bool AlignAtFocusingPosition => _alignAtFocusingPosition;
        public bool ReverseScrollingDirection => _reverseScrollingDirection;
        public CircularScrollingList.FocusingPosition FocusingPosition =>
            _focusingPosition;
        public bool ReverseContentOrder => _reverseContentOrder;
        public int InitFocusingContentID => _initFocusingContentID;
        public bool FocusSelectedBox => _focusSelectedBox;
        public bool InitializeOnStart => _initializeOnStart;

        public float BoxDensity => _boxDensity;
        public AnimationCurve BoxPositionCurve => _boxPositionCurve;
        public AnimationCurve BoxScaleCurve => _boxScaleCurve;
        public AnimationCurve BoxVelocityCurve => _boxVelocityCurve;
        public AnimationCurve BoxMovementCurve => _boxMovementCurve;

        public ListBoxSelectedEvent OnBoxSelected => _onBoxSelected;
        public ListTwoBoxesEvent OnFocusingBoxChanged => _onFocusingBoxChanged;
        public UnityEvent OnMovementEnd => _onMovementEnd;

        #endregion

        #region Private Members

        /// <summary>
        /// The name of the list
        /// </summary>
        private string _name;
        /// <summary>
        /// Is the setting initialized?
        /// </summary>
        private bool _isInitialized;

        #endregion

        #region Property Setters

        public void SetListType(CircularScrollingList.ListType listType)
        {
            if (CheckIsInitialized())
                return;

            _listType = listType;
        }

        public void SetDirection(CircularScrollingList.Direction direction)
        {
            if (CheckIsInitialized())
                return;

            _direction = direction;
        }

        public void SetControlMode(CircularScrollingList.ControlMode controlMode)
        {
            if (CheckIsInitialized())
                return;

            _controlMode = controlMode;
        }

        public void SetAlignAtFocusingPosition(bool toAlign)
        {
            if (CheckIsInitialized())
                return;

            _alignAtFocusingPosition = toAlign;
        }

        public void SetReverseScrollingDirection(bool toReverse)
        {
            if (CheckIsInitialized())
                return;

            _reverseScrollingDirection = toReverse;
        }

        public void SetFocusingPosition(
            CircularScrollingList.FocusingPosition focusingPosition)
        {
            if (CheckIsInitialized())
                return;

            _focusingPosition = focusingPosition;
        }

        public void SetReverseContentOrder(bool toReverse)
        {
            if (CheckIsInitialized())
                return;

            _reverseContentOrder = toReverse;
        }

        public void SetInitFocusingContentID(int contentID)
        {
            if (CheckIsInitialized())
                return;

            _initFocusingContentID = contentID;
        }

        public void SetFocusSelectedBox(bool toFocus)
        {
            if (CheckIsInitialized())
                return;

            _focusSelectedBox = toFocus;
        }

        #endregion

        #region Appearance Setters

        public void SetBoxDensity(float boxDensity)
        {
            if (CheckIsInitialized())
                return;

            _boxDensity = boxDensity;
        }

        public void SetBoxPositionCurve(AnimationCurve curve)
        {
            if (CheckIsInitialized())
                return;

            _boxPositionCurve = curve;
        }

        public void SetBoxScaleCurve(AnimationCurve curve)
        {
            if (CheckIsInitialized())
                return;

            _boxScaleCurve = curve;
        }

        public void SetBoxVelocityCurve(AnimationCurve curve)
        {
            if (CheckIsInitialized())
                return;

            _boxVelocityCurve = curve;
        }

        public void SetBoxMovementCurve(AnimationCurve curve)
        {
            if (CheckIsInitialized())
                return;

            _boxMovementCurve = curve;
        }

        #endregion

        #region Event Setters

        public void AddOnBoxSelectedCallback(UnityAction<ListBox> callback) =>
            _onBoxSelected.AddListener(callback);

        public void RemoveOnBoxSelectedCallback(UnityAction<ListBox> callback) =>
            _onBoxSelected.RemoveListener(callback);

        public void AddOnFocusingBoxChangedCallback(
            UnityAction<ListBox, ListBox> callback) =>
            _onFocusingBoxChanged.AddListener(callback);

        public void RemoveOnFocusingBoxChangedCallback(
            UnityAction<ListBox, ListBox> callback) =>
            _onFocusingBoxChanged.RemoveListener(callback);

        public void AddOnMovementEndCallback(UnityAction callback) =>
            _onMovementEnd.AddListener(callback);

        public void RemoveOnMovementEndCallback(UnityAction callback) =>
            _onMovementEnd.RemoveListener(callback);

        #endregion

        #region Public Functions

        /// <summary>
        /// Is the list initialized?
        /// </summary>
        private bool CheckIsInitialized()
        {
            if (_isInitialized)
                Debug.LogWarning(
                    $"The list setting of the list '{_name}' is initialized. Skip");

            return _isInitialized;
        }

        /// <summary>
        /// Initialize the settings
        /// </summary>
        /// <param name="listBank">The list bank</param>
        /// <param name="name">The name of the list</param>
        public void Initialize(BaseListBank listBank, string name)
        {
            var contentCount = listBank.GetContentCount();
            if (_initFocusingContentID < 0
                || (contentCount > 0 && _initFocusingContentID >= contentCount))
                throw new IndexOutOfRangeException(
                    "The 'InitFocusingContentID' is negative "
                    + "or greater than the number of contents in the list bank "
                    + $"in the list '{name}'.");

            if (Mathf.Approximately(_boxDensity, 0f))
                throw new InvalidOperationException(
                    $"The 'BoxDensity' shouldn't be 0 in the list '{name}'");

            switch (_focusingPosition) {
                case CircularScrollingList.FocusingPosition.Top:
                    _reverseContentOrder = false;
                    break;
                case CircularScrollingList.FocusingPosition.Bottom:
                    _reverseContentOrder = true;
                    break;
            }

            _name = name;
            _isInitialized = true;
        }

        #endregion
    }
}
