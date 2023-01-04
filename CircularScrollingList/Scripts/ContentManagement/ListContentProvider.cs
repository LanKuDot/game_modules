using System;
using UnityEngine;

namespace AirFishLab.ScrollingList.ContentManagement
{
    public class ListContentProvider : IListContentProvider
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
        private ListSetting _listSetting;
        /// <summary>
        /// The components holding the list contents
        /// </summary>
        private IListBank _listBank;
        /// <summary>
        /// The number of the list boxes
        /// </summary>
        private int _numOfBoxes;
        /// <summary>
        /// The factor for getting the next/last content ID
        /// </summary>
        private int _idFactor;
        /// <summary>
        /// The function for calculating the final content ID
        /// </summary>
        private Func<int, int> _idCalculationFunc;

        #endregion

        #region IListContentProvider

        public void Initialize(ListSetupData setupData)
        {
            _listSetting = setupData.Setting;
            _listBank = setupData.ListBank;
            _numOfBoxes = setupData.ListBoxes.Count;

            _idFactor = _listSetting.ReverseContentOrder ? -1 : 1;
            if (_listSetting.ListType == CircularScrollingList.ListType.Circular)
                _idCalculationFunc = GetLoopedContentID;
            else
                _idCalculationFunc = GetNonLoopedContentID;
        }

        public int GetInitialContentID(int listBoxID)
        {
            if (GetContentCount() == 0)
                return NO_CONTENT_ID;

            var contentID =
                _listSetting.ReverseContentOrder
                    ? _numOfBoxes / 2 - listBoxID
                    : listBoxID - _numOfBoxes / 2;

            return _idCalculationFunc(contentID);
        }

        public int GetRefreshedContentID(int contentID) =>
            _listBank.GetContentCount() == 0
                ? NO_CONTENT_ID
                : _idCalculationFunc(contentID);

        public int GetContentIDByNextBox(int nextBoxContentID) =>
            _idCalculationFunc(nextBoxContentID - _idFactor);

        public int GetContentIDByLastBox(int lastBoxContentID) =>
            _idCalculationFunc(lastBoxContentID + _idFactor);

        public int GetContentCount() => _listBank.GetContentCount();

        public bool TryGetContent(int contentID, out IListContent content)
        {
            var isIDValid = IsIDValid(contentID);
            content = isIDValid ? _listBank.GetListContent(contentID) : null;
            return isIDValid;
        }

        public ContentIDState GetIDState(int contentID) =>
            contentID < 0
                ? ContentIDState.Underflow
                : contentID >= _listBank.GetContentCount()
                    ? ContentIDState.Overflow
                    : ContentIDState.Valid;

        public bool IsIDValid(int contentID) =>
            contentID >= 0 && contentID < _listBank.GetContentCount();

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

        #endregion

        #region Content ID Calculation

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
    }
}
