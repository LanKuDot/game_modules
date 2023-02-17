using System;
using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.Util;
using UnityEngine;

namespace AirFishLab.ScrollingList.ListStateProcessing.Linear
{
    /// <summary>
    /// The component for finding the focusing boxes
    /// </summary>
    public class FocusingBoxFinder
    {
        #region Data

        /// <summary>
        /// The current focusing box
        /// </summary>
        public struct FocusingBox
        {
            /// <summary>
            /// The current focusing box
            /// </summary>
            public IListBox Box;
            /// <summary>
            /// The distance to align the focusing box to the baseline
            /// </summary>
            public float AligningDistance;

            public void Deconstruct(out IListBox box, out float aligningDistance)
            {
                box = Box;
                aligningDistance = AligningDistance;
            }
        }

        /// <summary>
        /// The finding result at the middle of the list
        /// </summary>
        public struct MiddleResult
        {
            /// <summary>
            /// The focusing state of the list
            /// </summary>
            public ListFocusingState ListFocusingState;
            /// <summary>
            /// The focusing box at the middle of the list
            /// </summary>
            public FocusingBox MiddleFocusing;
        }

        /// <summary>
        /// The finding result for both ends
        /// </summary>
        public struct BothEndsResult
        {
            /// <summary>
            /// The focusing state of the list
            /// </summary>
            public ListFocusingState ListFocusingState;
            /// <summary>
            /// The focusing box at the top of the list
            /// </summary>
            public FocusingBox TopFocusing;
            /// <summary>
            /// The focusing box at the bottom of the list
            /// </summary>
            public FocusingBox BottomFocusing;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// The target boxes
        /// </summary>
        private readonly List<IListBox> _boxes;
        /// <summary>
        /// The setting of the list
        /// </summary>
        private readonly ListSetting _setting;
        /// <summary>
        /// The function for getting the major position of the position
        /// </summary>
        private readonly Func<Vector2, float> _getMajorPosFunc;
        /// <summary>
        /// The baseline position at the top of the list
        /// </summary>
        private readonly float _topBaseline;
        /// <summary>
        /// The baseline position at the bottom of the list
        /// </summary>
        private readonly float _bottomBaseline;
        /// <summary>
        /// The tolerance for matching the baseline
        /// </summary>
        private const float DISTANCE_TOLERANCE = 1e-4f;

        #endregion

        public FocusingBoxFinder(
            List<IListBox> boxes, ListSetting setting,
            float topBaseline, float bottomBaseline)
        {
            _boxes = boxes;
            _setting = setting;
            if (setting.Direction == CircularScrollingList.Direction.Horizontal)
                _getMajorPosFunc = FactorUtility.GetVector2X;
            else
                _getMajorPosFunc = FactorUtility.GetVector2Y;
            _topBaseline = topBaseline;
            _bottomBaseline = bottomBaseline;
        }

        /// <summary>
        /// Find the currently focusing box
        /// </summary>
        /// <param name="contentCount">The number of contents</param>
        /// <returns>The result</returns>
        public MiddleResult FindForMiddle(int contentCount)
        {
            var deltaDistance = Mathf.Infinity;
            IListBox candidateBox = null;

            foreach (var box in _boxes) {
                if (!box.IsActivated
                    && box.ContentID != ListContentProvider.NO_CONTENT_ID)
                    continue;

                var boxPos = box.GetPosition();
                var boxDeltaDistance = -_getMajorPosFunc(boxPos);

                if (Mathf.Abs(boxDeltaDistance) >= Mathf.Abs(deltaDistance))
                    continue;

                deltaDistance = boxDeltaDistance;
                candidateBox = box;
            }

            var focusingBox = new FocusingBox {
                Box = candidateBox,
                AligningDistance = deltaDistance
            };
            var focusingState = FindFocusingStateForMiddle(focusingBox, contentCount);

            return new MiddleResult {
                ListFocusingState = focusingState,
                MiddleFocusing = focusingBox
            };
        }

        /// <summary>
        /// Find the focusing state of the list
        /// </summary>
        private ListFocusingState FindFocusingStateForMiddle(
            FocusingBox focusingBox, int contentCount)
        {
            return _setting.ListType != CircularScrollingList.ListType.Linear
                ? ListFocusingState.Middle
                : FindFocusingState(focusingBox, contentCount);
        }

        /// <summary>
        /// Find the focusing boxes at both ends
        /// </summary>
        /// <param name="contentCount">The number of contents</param>
        /// <returns>The result</returns>
        public BothEndsResult FindForBothEnds(int contentCount)
        {
            var topDeltaDistance = Mathf.Infinity;
            IListBox topCandidateBox = null;
            var bottomDeltaDistance = Mathf.NegativeInfinity;
            IListBox bottomCandidateBox = null;

            foreach (var box in _boxes) {
                if (!box.IsActivated
                    && box.ContentID != ListContentProvider.NO_CONTENT_ID)
                    continue;

                var boxPos = box.GetPosition();
                var majorPos = _getMajorPosFunc(boxPos);
                var boxTopDeltaDistance = _topBaseline - majorPos;
                var boxBottomDeltaDistance = _bottomBaseline - majorPos;

                if (Mathf.Abs(boxTopDeltaDistance) < Mathf.Abs(topDeltaDistance)) {
                    topDeltaDistance = boxTopDeltaDistance;
                    topCandidateBox = box;
                }

                if (Mathf.Abs(boxBottomDeltaDistance) < Mathf.Abs(bottomDeltaDistance)) {
                    bottomDeltaDistance = boxBottomDeltaDistance;
                    bottomCandidateBox = box;
                }
            }

            var topFocusingBox = new FocusingBox {
                Box = topCandidateBox,
                AligningDistance = topDeltaDistance
            };
            var bottomFocusingBox = new FocusingBox {
                Box = bottomCandidateBox,
                AligningDistance = bottomDeltaDistance
            };
            var focusingState =
                FindFocusingStateForBothEnds(
                    topFocusingBox, bottomFocusingBox, contentCount);

            return new BothEndsResult {
                ListFocusingState = focusingState,
                TopFocusing = topFocusingBox,
                BottomFocusing = bottomFocusingBox
            };
        }

        /// <summary>
        /// Find the focusing state of the list
        /// </summary>
        private ListFocusingState FindFocusingStateForBothEnds(
            FocusingBox topFocusingBox, FocusingBox bottomFocusingBox,
            int contentCount)
        {
            if (_setting.ListType == CircularScrollingList.ListType.Circular)
                return ListFocusingState.Middle;

            var topFocusingState = FindFocusingState(topFocusingBox, contentCount);
            var bottomFocusingState = FindFocusingState(bottomFocusingBox, contentCount);
            var focusingState = ListFocusingState.None;

            // It is possible that the showing content is the whole list
            if (topFocusingState != ListFocusingState.Middle)
                focusingState |= topFocusingState;
            if (bottomFocusingState != ListFocusingState.Middle)
                focusingState |= bottomFocusingState;

            return focusingState == ListFocusingState.None
                ? ListFocusingState.Middle
                : focusingState;
        }

        /// <summary>
        /// Find the focusing state of the box
        /// </summary>
        private ListFocusingState FindFocusingState(
            FocusingBox focusingBox, int contentCount)
        {
            var (box, aligningDistance) = focusingBox;
            var focusingContentID = box.ContentID;
            var isReversed = _setting.ReverseContentOrder;
            var isFirstContent = focusingContentID == 0;
            var isLastContent = focusingContentID == contentCount - 1;

            if (!(isFirstContent || isLastContent))
                return ListFocusingState.Middle;
            if (isReversed ^ isFirstContent
                && aligningDistance > -DISTANCE_TOLERANCE)
                return ListFocusingState.Top;
            if (isReversed ^ isLastContent
                && aligningDistance < DISTANCE_TOLERANCE)
                return ListFocusingState.Bottom;

            return ListFocusingState.Middle;
        }
    }
}
