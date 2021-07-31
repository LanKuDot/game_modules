using System.Collections.Generic;
using AirFishLab.ScrollingList.BoxTransformCtrl;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The basic component of the scrolling list.
    /// Control the position and update the content of the list element.
    /// </summary>
    public class ListBox : MonoBehaviour
    {
        #region Exposed Properties

        /// <summary>
        /// The ID of this box in the registered boxes
        /// </summary>
        public int listBoxID { get; private set; }
        /// <summary>
        /// The previous box of this box
        /// </summary>
        public ListBox lastListBox { get; private set; }
        /// <summary>
        /// The next box of this box
        /// </summary>
        public ListBox nextListBox { get; private set; }
        /// <summary>
        /// The ID of the content that the box references
        /// </summary>
        public int contentID { get; private set; }

        #endregion

        #region Referenced Components

        private CircularScrollingListSetting _listSetting;
        private ListPositionCtrl _positionCtrl;
        private ListContentManager _contentManager;
        private List<ListBox> _listBoxes;

        #endregion

        #region Private Memebers

        private IBoxTransformCtrl _boxTransformCtrl;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the box
        /// </summary>
        /// <param name="setting">The setting of the list</param>
        /// <param name="listPositionCtrl">The position controller of this box</param>
        /// <param name="listContentManager">The content controller</param>
        /// <param name="listBoxes">The boxes that belongs to the list</param>
        /// <param name="listBoxID">The ID of this box</param>
        public void Initialize(
            CircularScrollingListSetting setting,
            ListPositionCtrl listPositionCtrl,
            ListContentManager listContentManager,
            List<ListBox> listBoxes,
            int listBoxID)
        {
            _listSetting = setting;
            _positionCtrl = listPositionCtrl;
            _contentManager = listContentManager;
            _listBoxes = listBoxes;
            this.listBoxID = listBoxID;

            InitializePosition();
            InitializeBoxDependency();
            InitializeContent();
            AddClickEvent();
        }

        /// <summary>
        /// Initialize the local position of the list box according to its ID
        /// </summary>
        private void InitializePosition()
        {
            _boxTransformCtrl =
                new LinearBoxTransformCtrl(
                    _positionCtrl,
                    _listSetting.boxPositionCurve,
                    _listSetting.boxScaleCurve,
                    _listSetting.direction);
            _boxTransformCtrl.SetInitialTransform(
                transform, listBoxID, _listBoxes.Count);
        }

        /// <summary>
        /// Initialize the box dependency
        /// </summary>
        private void InitializeBoxDependency()
        {
            var numOfBoxes = _listBoxes.Count;

            lastListBox = _listBoxes[(int) Mathf.Repeat(listBoxID - 1, numOfBoxes)];
            nextListBox = _listBoxes[(int) Mathf.Repeat(listBoxID + 1, numOfBoxes)];
        }

        /// <summary>
        /// Add an additional listener to Button.onClick event for passing click
        /// event to target <c>ListPositionCtrl</c>
        /// </summary>
        private void AddClickEvent()
        {
            var button = transform.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(
                    () => _listSetting.onBoxClick.Invoke(contentID));
        }

        #endregion

        #region Position Controlling

        /// <summary>
        /// Update the position of the box
        /// </summary>
        /// <param name="delta">The delta distance in the major direction</param>
        public void UpdatePosition(float delta)
        {
            _boxTransformCtrl.SetLocalTransform(
                transform, delta,
                out var needToUpdateToLastContent,
                out var needToUpdateToNextContent);

            if (needToUpdateToLastContent)
                UpdateToLastContent();
            else if (needToUpdateToNextContent)
                UpdateToNextContent();
        }

        /// <summary>
        /// Pop to the to the front of the image sorting
        /// </summary>
        public void PopToFront()
        {
            transform.SetAsLastSibling();
        }

        /// <summary>
        /// Push the box to the back of the image sorting
        /// </summary>
        public void PushToBack()
        {
            transform.SetAsFirstSibling();
        }

        #endregion

        #region Content Handling

        /// <summary>
        /// Initialize the content of ListBox.
        /// </summary>
        private void InitializeContent()
        {
            contentID = _contentManager.GetInitialContentID(listBoxID);

            CheckToBeDisabled();
            if (gameObject.activeSelf)
                UpdateDisplayContentPrivate();
        }

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
        /// Update the content to the last content of the next ListBox
        /// </summary>
        private void UpdateToLastContent()
        {
            contentID = _contentManager.GetIDFromNextBox(nextListBox.contentID);

            if (_listSetting.listType == CircularScrollingList.ListType.Linear) {
                if (!_contentManager.IsIDValid(contentID)) {
                    // If the box has been disabled at the other side,
                    // decrease the counter of the other side.
                    if (!isActiveAndEnabled)
                        --_positionCtrl.numOfLowerDisabledBoxes;

                    // In linear mode, don't display the content of the other end
                    gameObject.SetActive(false);
                    ++_positionCtrl.numOfUpperDisabledBoxes;

                    return;
                }

                // The disabled boxes from the other end will be enabled again
                if (!isActiveAndEnabled) {
                    gameObject.SetActive(true);
                    --_positionCtrl.numOfLowerDisabledBoxes;
                }
            }

            UpdateDisplayContentPrivate();
            PushToBack();
        }

        /// <summary>
        /// Update the content to the next content of the last ListBox
        /// </summary>
        private void UpdateToNextContent()
        {
            contentID = _contentManager.GetIDFromLastBox(lastListBox.contentID);

            if (_listSetting.listType == CircularScrollingList.ListType.Linear) {
                if (!_contentManager.IsIDValid(contentID)) {
                    if (!isActiveAndEnabled)
                        --_positionCtrl.numOfUpperDisabledBoxes;

                    // In linear mode, don't display the content of the other end
                    gameObject.SetActive(false);
                    ++_positionCtrl.numOfLowerDisabledBoxes;

                    return;
                }

                if (!isActiveAndEnabled) {
                    gameObject.SetActive(true);
                    --_positionCtrl.numOfUpperDisabledBoxes;
                }
            }

            UpdateDisplayContentPrivate();
            PushToBack();
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
                tempBoxID -= _listBoxes.Count;
            else if (listBoxID < centerBoxID && posFactor < 0)
                tempBoxID += _listBoxes.Count;

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
