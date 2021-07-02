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
        }

        /// <summary>
        /// Get the initial content ID
        /// </summary>
        /// In the linear mode, the content ID will not be rounded,
        /// because the value will be used for considering that the box
        /// should be inactivated or not.
        /// <param name="listBoxID">The ID of requested list box</param>
        /// <returns>The content ID</returns>
        public int GetInitialContentID(int listBoxID)
        {
            var contentID = _listSetting.centeredContnetID;

            // Adjust the contentID according to its initial order
            contentID += listBoxID - _numOfBoxes / 2;

            return RepeatIDIfNeeded(contentID);
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
            var contentID = nextBoxContentID - 1;
            return RepeatIDIfNeeded(contentID);
        }

        /// <summary>
        /// Get the content ID according to the last list box
        /// </summary>
        /// <param name="lastBoxContentID">The content ID of the last list box</param>
        /// <returns></returns>
        public int GetIDFromLastBox(int lastBoxContentID)
        {
            var contentID = lastBoxContentID + 1;
            return RepeatIDIfNeeded(contentID);
        }

        /// <summary>
        /// If the list is in the circular mode, repeat the id
        /// </summary>
        private int RepeatIDIfNeeded(int id)
        {
            if (_listSetting.listType == CircularScrollingList.ListType.Circular)
                return (int) Mathf.Repeat(id, _listBank.GetListLength());

            return id;
        }

        /// <summary>
        /// Check if the id is valid or not
        /// </summary>
        public bool IsIDValid(int id)
        {
            return id >= 0 && id < _listBank.GetListLength();
        }
    }
}
