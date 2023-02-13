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
        /// The finding result
        /// </summary>
        public struct Result
        {
            /// <summary>
            /// The current focusing box
            /// </summary>
            public IListBox FocusingBox;
            /// <summary>
            /// The distance to align the focusing box to the baseline
            /// </summary>
            public float AligningDistance;

            public void Deconstruct(out IListBox focusingBox, out float aligningDistance)
            {
                focusingBox = FocusingBox;
                aligningDistance = AligningDistance;
            }
        }

        /// <summary>
        /// The finding result for both ends
        /// </summary>
        public struct BothEndsResult
        {
            /// <summary>
            /// The result at the top of the list
            /// </summary>
            public Result Top;
            /// <summary>
            /// The result at the bottom of the list
            /// </summary>
            public Result Bottom;
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
        /// <returns>The result</returns>
        public Result Find()
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

            return new Result {
                FocusingBox = candidateBox,
                AligningDistance = deltaDistance
            };
        }

        /// <summary>
        /// Find the focusing boxes at both ends
        /// </summary>
        /// <returns>The result</returns>
        public BothEndsResult FindForBothEnds()
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

            return new BothEndsResult {
                Top = new Result {
                    FocusingBox = topCandidateBox,
                    AligningDistance = topDeltaDistance
                },
                Bottom = new Result {
                    FocusingBox = bottomCandidateBox,
                    AligningDistance = bottomDeltaDistance
                }
            };
        }
    }
}
