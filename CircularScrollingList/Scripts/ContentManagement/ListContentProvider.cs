using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.ContentManagement
{
    public class ListContentProvider
    {
        #region Constant Value

        /// <summary>
        /// The content id that represents there has no content to display
        /// </summary>
        public const int NO_CONTENT_ID = int.MinValue;

        #endregion

        #region Private Members

        /// <summary>
        /// The setting of the list
        /// </summary>
        private readonly ListSetting _listSetting;
        /// <summary>
        /// The components holding the list contents
        /// </summary>
        private readonly IListBank _listBank;
        /// <summary>
        /// The number of the list boxes
        /// </summary>
        private readonly int _numOfBoxes;
        /// <summary>
        /// The factor for getting the next/last content ID
        /// </summary>
        private readonly int _idFactor;
        /// <summary>
        /// The function for calculating the final content ID
        /// </summary>
        private readonly Func<int, int> _idCalculationFunc;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="listSetting">The setting of the list</param>
        /// <param name="listBank">The list content bank</param>
        /// <param name="numOfBoxes">The number of boxes</param>
        public ListContentProvider(
            ListSetting listSetting, IListBank listBank, int numOfBoxes)
        {
            _listSetting = listSetting;
            _listBank = listBank;
            _numOfBoxes = numOfBoxes;

            _idFactor = _listSetting.ReverseContentOrder ? -1 : 1;
            if (_listSetting.ListType == CircularScrollingList.ListType.Circular)
                _idCalculationFunc = GetLoopedContentID;
            else
                _idCalculationFunc = GetNonLoopedContentID;
        }

        #region Content ID Calculation

        /// <summary>
        /// Get the initial content ID for the specified box
        /// </summary>
        /// <param name="listBoxID">The id of the box</param>
        /// <returns>The content ID</returns>
        public int GetInitialContentID(int listBoxID)
        {
            var contentCount = GetContentCount();
            if (contentCount == 0)
                return NO_CONTENT_ID;

            var contentID = 0;
            var initFocusedContentID = _listSetting.InitFocusingContentID;
            switch (_listSetting.FocusingPosition) {
                case CircularScrollingList.FocusingPosition.Top:
                case CircularScrollingList.FocusingPosition.Bottom:
                    if (_listSetting.ListType
                        == CircularScrollingList.ListType.Circular)
                        // No need to do content adjusting in the circular mode
                        contentID = 0;
                    else if (contentCount <= _numOfBoxes)
                        initFocusedContentID = 0;
                    else {
                        var numOfLackingContents =
                            contentCount - initFocusedContentID - _numOfBoxes;
                        if (numOfLackingContents < 0)
                            initFocusedContentID += numOfLackingContents;
                    }

                    // The reverse content order will be true,
                    // if focusing position is 'bottom'.
                    // Otherwise, it will be false.
                    contentID =
                        _listSetting.ReverseContentOrder
                            ? _numOfBoxes - 1 - listBoxID + initFocusedContentID
                            : listBoxID + initFocusedContentID;
                    break;

                case CircularScrollingList.FocusingPosition.Center:
                    contentID =
                        _listSetting.ReverseContentOrder
                            ? _numOfBoxes / 2 - listBoxID
                            : listBoxID - _numOfBoxes / 2;
                    contentID += initFocusedContentID;
                    break;
            }

            return _idCalculationFunc(contentID);
        }

        /// <summary>
        /// Get the content id after the list content is refreshed
        /// </summary>
        /// <param name="origContentID">The original content ID</param>
        /// <returns>The converted content ID</returns>
        public int GetRefreshedContentID(int origContentID) =>
            _listBank.GetContentCount() == 0
                ? NO_CONTENT_ID
                : _idCalculationFunc(origContentID);

        /// <summary>
        /// Get the content ID according to the content ID of the next box
        /// </summary>
        /// <param name="nextBoxContentID">The content ID of the next box</param>
        /// <returns>The content ID</returns>
        public int GetContentIDByNextBox(int nextBoxContentID) =>
            _idCalculationFunc(nextBoxContentID - _idFactor);

        /// <summary>
        /// Get the content ID according to the content ID of the last box
        /// </summary>
        /// <param name="lastBoxContentID">The content ID of the last box</param>
        /// <returns>The content ID</returns>
        public int GetContentIDByLastBox(int lastBoxContentID) =>
            _idCalculationFunc(lastBoxContentID + _idFactor);

        /// <summary>
        /// Check the state of the specified id
        /// </summary>
        /// <param name="contentID">The content id</param>
        /// <returns>The state of the id</returns>
        public ContentIDState GetIDState(int contentID)
        {
            return GetIDState(contentID, _listBank.GetContentCount());
        }

        /// <summary>
        /// Check the state of the specified id
        /// </summary>
        /// <param name="contentID">The content id</param>
        /// <param name="contentCount">The number of the contents</param>
        /// <returns>The state of the id</returns>
        public static ContentIDState GetIDState(int contentID, int contentCount)
        {
            if (contentID == NO_CONTENT_ID)
                return ContentIDState.NoContent;

            var state = contentID < 0
                ? ContentIDState.Underflow
                : contentID >= contentCount
                    ? ContentIDState.Overflow
                    : ContentIDState.Valid;

            if (state != ContentIDState.Valid)
                return state;

            if (contentID == 0)
                state |= ContentIDState.First;
            if (contentID == contentCount - 1)
                state |= ContentIDState.Last;

            return state;
        }

        /// <summary>
        /// Is the content id valid for getting the list content?
        /// </summary>
        /// <param name="contentID">The content id</param>
        /// <returns>The content id is valid or not</returns>
        public bool IsIDValid(int contentID) =>
            contentID >= 0 && contentID < _listBank.GetContentCount();

        /// <summary>
        /// Get the shortest length for starting from one id to another id
        /// </summary>
        /// <param name="fromContentID">The starting content id</param>
        /// <param name="toContentID">The target content id</param>
        /// <returns>The shortest length</returns>
        public int GetShortestIDDiff(int fromContentID, int toContentID)
        {
            if (!IsIDValid(fromContentID))
                throw new IndexOutOfRangeException(nameof(fromContentID));
            if (!IsIDValid(toContentID))
                throw new IndexOutOfRangeException(nameof(toContentID));

            var length = toContentID - fromContentID;

            if (_listSetting.ListType == CircularScrollingList.ListType.Linear)
                return length;

            var numOfContents = _listBank.GetContentCount();
            var halfNumOfContents = numOfContents / 2;

            if (Mathf.Abs(length) > halfNumOfContents)
                length -= (int) Mathf.Sign(length) * numOfContents;

            return length;
        }

        /// <summary>
        /// Loop the input ID within the range of the indexes of the contents
        /// </summary>
        private int GetLoopedContentID(int contentID) =>
            (int)Mathf.Repeat(contentID, _listBank.GetContentCount());

        /// <summary>
        /// Just return the input ID
        /// </summary>
        private int GetNonLoopedContentID(int contentID) => contentID;

        #endregion

        #region Content Handling

        /// <summary>
        /// Get the number of the contents
        /// </summary>
        public int GetContentCount() => _listBank.GetContentCount();

        /// <summary>
        /// Try to get the content of the list
        /// </summary>
        /// <param name="contentID">The id of the content</param>
        /// <param name="content">
        /// The content. If the content is not available, it will be null.
        /// </param>
        /// <returns>It the content available?</returns>
        public bool TryGetContent(int contentID, out IListContent content)
        {
            var isIDValid = IsIDValid(contentID);
            content = isIDValid ? _listBank.GetListContent(contentID) : null;
            return isIDValid;
        }

        #endregion
    }
}
