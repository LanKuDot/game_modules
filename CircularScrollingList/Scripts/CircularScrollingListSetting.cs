using System;
using UnityEngine;
using UnityEngine.Events;

namespace AirFishLab.ScrollingList
{
    [Serializable]
    public class CircularScrollingListSetting
    {
        #region List Mode

        [SerializeField]
        [Tooltip("The type of the list.")]
        private CircularScrollingList.ListType _listType =
            CircularScrollingList.ListType.Circular;
        [SerializeField]
        [Tooltip("The controlling mode of the list.")]
        private CircularScrollingList.ControlMode _controlMode =
            CircularScrollingList.ControlMode.Drag;
        [SerializeField]
        [Tooltip("Should a box align in the middle of the list after sliding?")]
        private bool _alignMiddle = false;
        [SerializeField]
        [Tooltip("Whether to reverse the scrolling direction or not")]
        private bool _reverseDirection = false;
        [SerializeField]
        [Tooltip("The major moving direction of the list.")]
        private CircularScrollingList.Direction _direction =
            CircularScrollingList.Direction.Vertical;
        [SerializeField]
        [Tooltip("Specify the initial content ID for the centered box.")]
        private int _centeredContentID = 0;
        [SerializeField]
        [Tooltip("Whether to center the selected box or not")]
        private bool _centerSelectedBox;
        [SerializeField]
        [Tooltip("To show the list in the reverse order")]
        private bool _reverseOrder;
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
                 "The y axis is the factor for reaching the target position.")]
        private AnimationCurve _boxMovementCurve =
            new AnimationCurve(
                new Keyframe(0.0f, 0.0f, 0.0f, 8f),
                new Keyframe(0.25f, 1.0f, 0.0f, 0.0f));

        #endregion

        #region Events

        [SerializeField]
        [Tooltip("The callback to be invoked when a box is clicked. " +
                 "The registered callbacks will be added to the 'onClick' event of boxes, " +
                 "therefore, boxes should be 'Button's.")]
        private ListBoxIntEvent _onBoxClick;
        [SerializeField]
        [Tooltip("The callback to be invoked when the centered content is changed. " +
                 "The int parameter is the new content ID.")]
        private ListBoxIntEvent _onCenteredContentChanged;
        [SerializeField]
        [Tooltip("The callback to be invoked when the movement is ended")]
        private UnityEvent _onMovementEnd;

        #endregion

        #region Setting Getter

        public CircularScrollingList.ListType listType => _listType;
        public CircularScrollingList.ControlMode controlMode => _controlMode;
        public bool alignMiddle => _alignMiddle;
        public bool reverseDirection => _reverseDirection;
        public CircularScrollingList.Direction direction => _direction;
        public int centeredContentID => _centeredContentID;
        public bool centerSelectedBox => _centerSelectedBox;
        public bool reverseOrder => _reverseOrder;
        public bool initializeOnStart => _initializeOnStart;

        public float boxDensity => _boxDensity;
        public AnimationCurve boxPositionCurve => _boxPositionCurve;
        public AnimationCurve boxScaleCurve => _boxScaleCurve;
        public AnimationCurve boxVelocityCurve => _boxVelocityCurve;
        public AnimationCurve boxMovementCurve => _boxMovementCurve;

        public ListBoxIntEvent onBoxClick => _onBoxClick;
        public ListBoxIntEvent onCenteredContentChanged => _onCenteredContentChanged;
        public UnityEvent onMovementEnd => _onMovementEnd;

        #endregion
    }
}
