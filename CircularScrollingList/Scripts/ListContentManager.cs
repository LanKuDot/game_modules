using System;
using UnityEngine;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// Manage the list content
    /// </summary>
    public class ListContentManager
    {
        #region Exposed Properties

        /// <summary>
        /// The number of contents in the list bank
        /// </summary>
        public int ContentCount => _listBank.GetListLength();

        #endregion

        #region Referenced Components

        private readonly CircularScrollingListSetting _listSetting;
        private readonly BaseListBank _listBank;
        private readonly int _numOfBoxes;

        #endregion

        #region Private Members

        /// <summary>
        /// The factor for getting the next/last content ID
        /// </summary>
        private readonly int _idFactor;
        /// <summary>
        /// Handle the input id and return a proper id
        /// </summary>
        private readonly Func<int, int> _idHandler;

        #endregion

        /// <summary>
        /// Manage the list content
        /// </summary>
        /// <param name="setting">The settings of the list</param>
        /// <param name="listBank">
        /// The bank containing the contents for list to display
        /// </param>
        /// <param name="numOfBoxes">The number of list boxes</param>
        public ListContentManager(
            CircularScrollingListSetting setting,
            BaseListBank listBank,
            int numOfBoxes)
        {
            _listSetting = setting;
            _listBank = listBank;
            _numOfBoxes = numOfBoxes;

            _idFactor = setting.reverseOrder ? -1 : 1;
            _idHandler =
                setting.listType == CircularScrollingList.ListType.Circular
                    ? (Func<int, int>) (x =>
                        (int) Mathf.Repeat(x, _listBank.GetListLength()))
                    : x => x;
        }

        /// <summary>
        /// Get the initial content ID
        /// </summary>
        /// In the linear mode, the content ID will not be rounded,
        /// because the value will be used for considering that the box
        /// should be inactivated or not.
        /// <param name="listBoxID">The ID of requested list box</param>
        /// <returns>The content ID<para />
        /// If there is no content in the bank, return int.MinValue
        /// </returns>
        public int GetInitialContentID(int listBoxID)
        {
            if (_listBank.GetListLength() == 0)
                return int.MinValue;

            var contentID = _listSetting.centeredContentID;

            // Adjust the contentID according to its initial order
            contentID +=
                _listSetting.reverseOrder ?
                    _numOfBoxes / 2 - listBoxID :
                    listBoxID - _numOfBoxes / 2;

            return _idHandler(contentID);
        }

        /// <summary>
        /// Get the content ID
        /// </summary>
        /// Similar to <c>GetInitialContentID</c> but for the situation that
        /// the list is not in the initial state
        /// <param name="boxIDOffset">
        /// The offset of the box ID relative to the centered box ID
        /// </param>
        /// <param name="centerContentID">
        /// The content ID of the centered box
        /// </param>
        /// <returns>The content ID<para />
        /// If there is no content in the bank, return int.MinValue
        /// </returns>
        public int GetContentID(int boxIDOffset, int centerContentID)
        {
            if (_listBank.GetListLength() == 0)
                return int.MinValue;

            var contentID =
                _listSetting.reverseOrder
                    ? centerContentID - boxIDOffset
                    : centerContentID + boxIDOffset;

            return _idHandler(contentID);
        }

        /// <summary>
        /// Get the specified content
        /// </summary>
        /// <param name="contentID">The ID of the content in the list bank</param>
        /// <returns>The object of the content</returns>
        public object GetListContent(int contentID)
        {
            return _listBank.GetListContent(contentID);
        }

        /// <summary>
        /// Get the content ID according the next list box
        /// </summary>
        /// <param name="nextBoxContentID">The content ID of the next list box</param>
        /// <returns></returns>
        public int GetIDFromNextBox(int nextBoxContentID)
        {
            var contentID = nextBoxContentID - _idFactor;
            return _idHandler(contentID);
        }

        /// <summary>
        /// Get the content ID according to the last list box
        /// </summary>
        /// <param name="lastBoxContentID">The content ID of the last list box</param>
        /// <returns></returns>
        public int GetIDFromLastBox(int lastBoxContentID)
        {
            var contentID = lastBoxContentID + _idFactor;
            return _idHandler(contentID);
        }

        /// <summary>
        /// Check if the id is valid or not
        /// </summary>
        public bool IsIDValid(int id)
        {
            return id >= 0 && id < _listBank.GetListLength();
        }

        /// <summary>
        /// Get the shortest ID difference between two IDs
        /// </summary>
        /// <param name="fromID">The starting point</param>
        /// <param name="toID">The goal</param>
        /// <returns>The shortest ID difference</returns>
        public int GetShortestDiff(int fromID, int toID)
        {
            if (!IsIDValid(fromID))
                throw new IndexOutOfRangeException(nameof(fromID));
            if (!IsIDValid(toID))
                throw new IndexOutOfRangeException(nameof(toID));

            var difference = toID - fromID;

            if (_listSetting.listType == CircularScrollingList.ListType.Linear)
                return difference;

            var numOfContent = _listBank.GetListLength();
            var halfNumOfContent = numOfContent / 2;

            if (Mathf.Abs(difference) > halfNumOfContent)
                difference -= (int) Mathf.Sign(difference) * numOfContent;

            return difference;
        }
    }
}
