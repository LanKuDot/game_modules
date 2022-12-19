using System;
using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// Store the contents for the list boxes to display
    /// </summary>
    public abstract class BaseListBank : MonoBehaviour, IListContentProvider
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
        private CircularScrollingListSetting _listSetting;
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

        #region Abstract Functions

        public abstract IListContent GetListContent(int index);
        public abstract int GetContentCount();

        #endregion

        #region IListContentProvider

        public void Initialize(ListSetupData setupData)
        {
            _listSetting = setupData.Setting;
            _numOfBoxes = setupData.ListBoxes.Count;

            _idFactor = _listSetting.reverseOrder ? -1 : 1;
            if (_listSetting.listType == CircularScrollingList.ListType.Circular)
                _idCalculationFunc = GetLoopedContentID;
            else
                _idCalculationFunc = GetNonLoopedContentID;
        }

        public int GetInitialContentID(int listBoxID)
        {
            if (GetContentCount() == 0)
                return NO_CONTENT_ID;

            var contentID =
                _listSetting.reverseOrder
                    ? _numOfBoxes / 2 - listBoxID
                    : listBoxID - _numOfBoxes / 2;

            return _idCalculationFunc(contentID);
        }

        public int GetRefreshedContentID(int contentID) =>
            GetContentCount() == 0 ? NO_CONTENT_ID : _idCalculationFunc(contentID);

        public int GetContentIDByNextBox(int nextBoxContentID) =>
            _idCalculationFunc(nextBoxContentID - _idFactor);

        public int GetContentIDByLastBox(int lastBoxContentID) =>
            _idCalculationFunc(lastBoxContentID + _idFactor);

        public bool TryGetContent(int contentID, out IListContent content)
        {
            var isIDValid = IsIDValid(contentID);
            content = isIDValid ? GetListContent(contentID) : null;
            return isIDValid;
        }

        public ContentIDState GetIDState(int contentID) =>
            contentID < 0
                ? ContentIDState.Underflow
                : contentID >= GetContentCount()
                    ? ContentIDState.Overflow
                    : ContentIDState.Valid;

        public bool IsIDValid(int contentID) =>
            contentID >= 0 && contentID < GetContentCount();

        public int GetShortestIDDiff(int fromContentID, int toContentID)
        {
            if (!IsIDValid(fromContentID))
                throw new IndexOutOfRangeException(nameof(fromContentID));
            if (!IsIDValid(toContentID))
                throw new IndexOutOfRangeException(nameof(toContentID));

            var length = toContentID - fromContentID;

            if (_listSetting.listType == CircularScrollingList.ListType.Linear)
                return length;

            var numOfContents = GetContentCount();
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
            (int)Mathf.Repeat(contentID, GetContentCount());

        /// <summary>
        /// Just return the input ID
        /// </summary>
        private int GetNonLoopedContentID(int contentID) => contentID;

        #endregion
    }

/* The example of the ListBank
 */
    public class ListBank : BaseListBank
    {
        private int[] contents = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };

        private readonly Content _contentWrapper = new Content();

        public override IListContent GetListContent(int index)
        {
            _contentWrapper.Value = contents[index];
            return _contentWrapper;
        }

        public override int GetContentCount()
        {
            return contents.Length;
        }

        public class Content : IListContent
        {
            public int Value;
        }
    }
}
