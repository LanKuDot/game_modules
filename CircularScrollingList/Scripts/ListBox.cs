using AirFishLab.ScrollingList.BoxTransformCtrl;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.Util;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The basic component of the scrolling list.
    /// Control the position and update the content of the list element.
    /// </summary>
    public class ListBox : MonoBehaviour, IListBox
    {
        #region Properties of IListBox

        public Transform Transform => transform;
        public int ListBoxID { get; private set; }
        public int ContentID { get; private set; }
        public IListBox LastListBox { get; private set; }
        public IListBox NextListBox { get; private set; }
        public ListBoxIntEvent OnBoxClick { get; } = new ListBoxIntEvent();
        public bool IsActivated
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        #endregion

        #region Exposed Properties

        /// <summary>
        /// The list which this box belongs to
        /// </summary>
        public CircularScrollingList scrollingList { get; private set; }
        /// <summary>
        /// The ID of this box in the registered boxes
        /// </summary>
        public int listBoxID { get; private set; }
        /// <summary>
        /// The ID of the content that the box references
        /// </summary>
        public int contentID { get; private set; }

        #endregion

        #region Referenced Components

        private CircularScrollingListSetting _listSetting;
        private ListPositionCtrl _positionCtrl;
        private ListContentManager _contentManager;
        private ListBox[] _listBoxes;

        #endregion

        #region Private Memebers

        private IBoxTransformCtrl _boxTransformCtrl;

        #endregion

        #region IListBox

        public void Initialize(
            int listBoxID, IListBox lastListBox, IListBox nextListBox)
        {
            ListBoxID = listBoxID;
            LastListBox = lastListBox;
            NextListBox = nextListBox;
            RegisterClickEvent();
        }

        public void SetContent(int contentID, IListContent content)
        {
            ContentID = contentID;
            UpdateDisplayContent(contentID);
        }

        public void PopToFront()
        {
            transform.SetAsLastSibling();
        }

        public void PushToBack()
        {
            transform.SetAsFirstSibling();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the box
        /// </summary>
        /// <param name="scrollingList">The list which this box belongs to</param>
        /// <param name="listPositionCtrl">The position controller of this box</param>
        /// <param name="listContentManager">The content controller</param>
        /// <param name="listBoxID">The ID of this box</param>
        public void Initialize(
            CircularScrollingList scrollingList,
            ListPositionCtrl listPositionCtrl,
            ListContentManager listContentManager,
            int listBoxID)
        {
            this.scrollingList = scrollingList;
            this.listBoxID = listBoxID;

            _listSetting = scrollingList.setting;
            _positionCtrl = listPositionCtrl;
            _contentManager = listContentManager;
            _listBoxes = scrollingList.listBoxes;
        }

        /// <summary>
        /// Register the callback to the button clicking event
        /// </summary>
        private void RegisterClickEvent()
        {
            if (TryGetComponent<Button>(out var button)) {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnButtonClick()
        {
            OnBoxClick?.Invoke(ContentID);
        }

        #endregion

        #region Content Handling

        /// <summary>
        /// Disable the box if needed in the linear mode
        /// </summary>
        private void CheckToBeDisabled()
        {
            if (contentID == int.MinValue) {
                gameObject.SetActive(false);
                return;
            }

            if (_listSetting.listType != CircularScrollingList.ListType.Linear)
                return;

            // Disable the boxes at the upper half of the list
            // which will hold the item at the tail of the contents.
            if (contentID < 0) {
                if (_listSetting.reverseOrder)
                    _positionCtrl.numOfLowerDisabledBoxes += 1;
                else
                    _positionCtrl.numOfUpperDisabledBoxes += 1;
                gameObject.SetActive(false);
            }
            // Disable the box at the lower half of the list
            // which will hold the repeated item.
            else if (contentID >= _contentManager.ContentCount) {
                if (_listSetting.reverseOrder)
                    _positionCtrl.numOfUpperDisabledBoxes += 1;
                else
                    _positionCtrl.numOfLowerDisabledBoxes += 1;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Update the displaying content on the ListBox
        /// </summary>
        /// <param name="content">The content to be displayed</param>
        protected virtual void UpdateDisplayContent(object content)
        {
            Debug.Log(content);
        }

        /// <summary>
        /// The wrapper for invoking the custom UpdateDisplayContent
        /// </summary>
        private void UpdateDisplayContentPrivate()
        {
            UpdateDisplayContent(_contentManager.GetListContent(contentID));
        }

        /// <summary>
        /// Recalculate the contentID and reacquire the content from the bank
        /// </summary>
        /// <param name="centerBoxID">The id of the centered box</param>
        /// <param name="centerContentID">The content ID for the centered box</param>>
        public void Refresh(int centerBoxID, int centerContentID)
        {
            var localPos = transform.localPosition;
            var posFactor =
                _listSetting.direction == CircularScrollingList.Direction.Horizontal
                    ? FactorUtility.GetVector2X(localPos)
                    : FactorUtility.GetVector2Y(localPos);
            var tempBoxID = listBoxID;

            // Make the box ID be "in order"
            if (listBoxID > centerBoxID && posFactor > 0)
                tempBoxID -= _listBoxes.Length;
            else if (listBoxID < centerBoxID && posFactor < 0)
                tempBoxID += _listBoxes.Length;

            contentID =
                _contentManager.GetContentID(tempBoxID - centerBoxID, centerContentID);

            if (_contentManager.IsIDValid(contentID)) {
                // Activate the previous inactivated box
                gameObject.SetActive(true);
                UpdateDisplayContentPrivate();
            } else
                CheckToBeDisabled();
        }

        #endregion

    }
}
