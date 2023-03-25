using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
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
            /// The distance how long the box is away from the baseline
            /// </summary>
            public float DistanceOffset;

            public void Deconstruct(out IListBox box, out float distanceOffset)
            {
                box = Box;
                distanceOffset = DistanceOffset;
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
        /// The baseline position at the top of the list
        /// </summary>
        private readonly float _topBaseline;
        /// <summary>
        /// The baseline position at the middle of the list
        /// </summary>
        private readonly float _middleBaseline;
        /// <summary>
        /// The baseline position at the bottom of the list
        /// </summary>
        private readonly float _bottomBaseline;

        #endregion

        public FocusingBoxFinder(
            List<IListBox> boxes, ListSetting setting,
            float topBaseline, float middleBaseline, float bottomBaseline)
        {
            _boxes = boxes;
            _setting = setting;
            _topBaseline = topBaseline;
            _middleBaseline = middleBaseline;
            _bottomBaseline = bottomBaseline;
        }

        /// <summary>
        /// Find the currently focusing box
        /// </summary>
        /// <param name="contentCount">The number of contents</param>
        /// <returns>The result</returns>
        public MiddleResult FindForMiddle(int contentCount)
        {
            var distanceOffset = Mathf.Infinity;
            IListBox candidateBox = null;

            foreach (var box in _boxes) {
                var idState =
                    ListContentProvider.GetIDState(box.ContentID, contentCount);
                if (idState == ContentIDState.Overflow
                    || idState == ContentIDState.Underflow)
                    continue;

                var boxDistanceOffset = box.GetPositionFactor() - _middleBaseline;
                if (Mathf.Abs(boxDistanceOffset) >= Mathf.Abs(distanceOffset))
                    continue;

                distanceOffset = boxDistanceOffset;
                candidateBox = box;
            }

            var focusingBox = new FocusingBox {
                Box = candidateBox,
                DistanceOffset = distanceOffset
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
            var topDistanceOffset = Mathf.Infinity;
            IListBox topCandidateBox = null;
            var bottomDistanceOffset = Mathf.Infinity;
            IListBox bottomCandidateBox = null;

            foreach (var box in _boxes) {
                var idState =
                    ListContentProvider.GetIDState(box.ContentID, contentCount);
                if (idState == ContentIDState.Overflow
                    || idState == ContentIDState.Underflow)
                    continue;

                var positionFactor = box.GetPositionFactor();
                var boxTopDistanceOffset = positionFactor - _topBaseline;
                var boxBottomDistanceOffset = positionFactor - _bottomBaseline;

                if (Mathf.Abs(boxTopDistanceOffset) < Mathf.Abs(topDistanceOffset)) {
                    topDistanceOffset = boxTopDistanceOffset;
                    topCandidateBox = box;
                }

                if (Mathf.Abs(boxBottomDistanceOffset) < Mathf.Abs(bottomDistanceOffset)) {
                    bottomDistanceOffset = boxBottomDistanceOffset;
                    bottomCandidateBox = box;
                }
            }

            var topFocusingBox = new FocusingBox {
                Box = topCandidateBox,
                DistanceOffset = topDistanceOffset
            };
            var bottomFocusingBox = new FocusingBox {
                Box = bottomCandidateBox,
                DistanceOffset = bottomDistanceOffset
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
            var (box, _) = focusingBox;
            var focusingContentID = box.ContentID;
            var isReversed = _setting.ReverseContentOrder;
            var isFirstContent = focusingContentID == 0;
            var isLastContent = focusingContentID == contentCount - 1;

            if (!(isFirstContent || isLastContent))
                return ListFocusingState.Middle;
            if (isFirstContent && isLastContent)
                return ListFocusingState.TopAndBottom;

            return isReversed ^ isFirstContent
                ? ListFocusingState.Top
                : ListFocusingState.Bottom;
        }
    }
}
