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
        [Tooltip("The controlling mode of the list.")]
        private CircularScrollingList.ControlMode _controlMode =
            CircularScrollingList.ControlMode.Everything;
        [SerializeField]
        [Tooltip("To align a box in the center of the list after sliding")]
        [FormerlySerializedAs("_alignMiddle")]
        private bool _alignInCenter;
        [SerializeField]
        [Tooltip("To reverse the scrolling direction")]
        [FormerlySerializedAs("_reverseDirection")]
        private bool _reverseScrollingDirection;
        [SerializeField]
        [Tooltip("The major moving direction of the list.")]
        private CircularScrollingList.Direction _direction =
            CircularScrollingList.Direction.Vertical;
        [SerializeField]
        [Tooltip("Specify the initial content ID for the centered box.")]
        private int _centeredContentID;
        [SerializeField]
        [Tooltip("To center the selected box")]
        private bool _centerSelectedBox;
        [SerializeField]
        [Tooltip("To show the list contents in the reversed order")]
        [FormerlySerializedAs("_reverseOrder")]
        private bool _reverseContentOrder;
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
        [Obsolete("It will be replaced by _onCenteredBoxChanged event")]
        [Tooltip("The callback to be invoked when the centered content is changed. " +
                 "The int parameter is the new content ID.")]
        private ListBoxIntEvent _onCenteredContentChanged;
        [SerializeField]
        [Tooltip("The callback to be invoked when the centered box is changed. " +
                 "The first argument is previous centered box, " +
                 "and the second one is current centered box.")]
        private ListTwoBoxesEvent _onCenteredBoxChanged;
        [SerializeField]
        [Tooltip("The callback to be invoked when the movement is ended")]
        private UnityEvent _onMovementEnd;

        #endregion

        #region Setting Getter

        public CircularScrollingList.ListType ListType => _listType;
        public CircularScrollingList.ControlMode ControlMode => _controlMode;
        public bool AlignInCenter => _alignInCenter;
        public bool ReverseScrollingDirection => _reverseScrollingDirection;
        public CircularScrollingList.Direction Direction => _direction;
        public int CenteredContentID => _centeredContentID;
        public bool CenterSelectedBox => _centerSelectedBox;
        public bool ReverseContentOrder => _reverseContentOrder;
        public bool InitializeOnStart => _initializeOnStart;

        public float BoxDensity => _boxDensity;
        public AnimationCurve BoxPositionCurve => _boxPositionCurve;
        public AnimationCurve BoxScaleCurve => _boxScaleCurve;
        public AnimationCurve BoxVelocityCurve => _boxVelocityCurve;
        public AnimationCurve BoxMovementCurve => _boxMovementCurve;

        public ListBoxIntEvent OnBoxClick => _onBoxClick;
        public ListBoxIntEvent OnCenteredContentChanged => _onCenteredContentChanged;
        public ListTwoBoxesEvent OnCenteredBoxChanged => _onCenteredBoxChanged;
        public UnityEvent OnMovementEnd => _onMovementEnd;

        #endregion
    }
}
