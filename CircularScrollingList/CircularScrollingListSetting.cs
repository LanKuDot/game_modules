﻿using System;
using System.Collections.Generic;
using UnityEngine;

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
        [Tooltip("The major moving direction of the list.")]
        private CircularScrollingList.Direction _direction =
            CircularScrollingList.Direction.Vertical;

        #endregion

        #region Containers

        [SerializeField]
        [Tooltip("The game object which holds the content bank for the list. " +
                 "It will be the derived class of the BaseListBank.")]
        private BaseListBank _listBank;
        [SerializeField]
        [Tooltip("Specify the initial content ID for the centered box.")]
        private int _centeredContentID = 0;
        [SerializeField]
        [Tooltip("The boxes which belong to this list.")]
        private List<ListBox> _listBoxes;

        #endregion

        #region List Appearance

        [SerializeField]
        [Tooltip("The distance between each box. The larger, the closer.")]
        private float _boxDensity = 2.0f;
        [SerializeField]
        [Tooltip("The curve specifying the box position. " +
                 "The x axis is the major position of the box, which is mapped to [0, 1]. " +
                 "The y axis defines the factor of the passive position of the box. " +
                 "Point (0.5, 0) is the center of the list layout.")]
        private AnimationCurve _boxPositionCurve =
            AnimationCurve.Constant(0.0f, 1.0f, 0.0f);
        [SerializeField]
        [Tooltip("The curve specifying the box scale. " +
                 "The x axis is the major position of the box, which is mapped to [0, 1]. " +
                 "The y axis specifies the value of 'localScale' of the box at the " +
                 "corresponding position.")]
        private AnimationCurve _boxScaleCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
        [SerializeField]
        [Tooltip("The curve specifying the movement of the box. " +
                 "The x axis is the moving duration in seconds, which starts from 0. " +
                 "The y axis is the factor of the releasing velocity in Drag mode, or " +
                 "the factor of the target position in Function and Mouse Wheel modes.")]
        private AnimationCurve _boxMovementCurve = new AnimationCurve(
            new Keyframe(0.0f, 1.0f, 0.0f, -2.5f),
            new Keyframe(1.0f, 0.0f, 0.0f, 0.0f));

        #endregion

        #region Events

        [SerializeField]
        [Tooltip("The callbacks for the event of the clicking on boxes." +
                 "The registered callbacks will be added to the 'onClick' event of boxes, " +
                 "therefore, boxes should be 'Button's.")]
        private ListBoxClickEvent _onBoxClick;

        #endregion

        #region Setting Getter

        public CircularScrollingList.ListType listType => _listType;
        public CircularScrollingList.ControlMode controlMode => _controlMode;
        public bool alignMiddle => _alignMiddle;
        public CircularScrollingList.Direction direction => _direction;

        public BaseListBank listBank => _listBank;
        public int centeredContentId => _centeredContentID;
        public List<ListBox> listBoxes => _listBoxes;

        public float boxDensity => _boxDensity;
        public AnimationCurve boxPositionCurve => _boxPositionCurve;
        public AnimationCurve boxScaleCurve => _boxScaleCurve;
        public AnimationCurve boxMovementCurve => _boxMovementCurve;

        public ListBoxClickEvent onBoxClick => _onBoxClick;

        #endregion
    }
}
