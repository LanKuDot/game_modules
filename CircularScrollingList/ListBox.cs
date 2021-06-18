using System;
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
        #region Setting Properties

        public int listBoxID { set; get; }
        public ListBox lastListBox { set; get; }
        public ListBox nextListBox { set; get; }

        #endregion

        private int _contentID;

        private ListPositionCtrl _positionCtrl;
        private BaseListBank _listBank;
        private CurveResolver _positionCurve;
        private CurveResolver _scaleCurve;

        public Action<float> UpdatePosition { private set; get; }

        #region Position Controlling Variables

        // Position calculated here is in the local space of the list
        /// <summary>
        /// The distance between boxes
        /// </summary>
        private float _unitPos;
        /// <summary>
        /// The left/down-most position of the box
        /// </summary>
        private float _lowerBoundPos;
        /// <summary>
        /// The right/up-most position of the box
        /// </summary>
        private float _upperBoundPos;
        /// <summary>
        /// The lower boundary where the box will be moved to the other end
        /// </summary>
        private float _changeSideLowerBoundPos;
        /// <summary>
        /// The upper boundary where the box will be moved to the other end
        /// </summary>
        private float _changeSideUpperBoundPos;

        #endregion

        /// <summary>
        /// Output the information of the box to the Debug.Log
        /// </summary>
        public void ShowBoxInfo()
        {
            Debug.Log("Box ID: " + listBoxID.ToString() +
                      ", Content ID: " + _contentID.ToString() +
                      ", Content: " + _listBank.GetListContent(_contentID));
        }

        /// <summary>
        /// Get the content ID of the box
        /// </summary>
        public int GetContentID()
        {
            return _contentID;
        }

        #region Initialization

        /// <summary>
        /// Initialize the box
        /// </summary>
        /// <param name="listPositionCtrl">The position controller of this box</param>
        public void Initialize(ListPositionCtrl listPositionCtrl)
        {
            _positionCtrl = listPositionCtrl;
            _listBank = _positionCtrl.listBank;

            switch (_positionCtrl.direction) {
                case ListPositionCtrl.Direction.Vertical:
                    UpdatePosition = MoveVertically;
                    break;
                case ListPositionCtrl.Direction.Horizontal:
                    UpdatePosition = MoveHorizontally;
                    break;
            }

            _unitPos = _positionCtrl.unitPos;
            _lowerBoundPos = _positionCtrl.lowerBoundPos;
            _upperBoundPos = _positionCtrl.upperBoundPos;
            _changeSideLowerBoundPos = _lowerBoundPos + _unitPos * 0.5f;
            _changeSideUpperBoundPos = _upperBoundPos - _unitPos * 0.5f;

            _positionCurve = new CurveResolver(
                _positionCtrl.boxPositionCurve,
                _changeSideLowerBoundPos, _changeSideUpperBoundPos);
            _scaleCurve = new CurveResolver(
                _positionCtrl.boxScaleCurve,
                _changeSideLowerBoundPos, _changeSideUpperBoundPos);

            InitialPosition();
            InitialContent();
            AddClickEvent();
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
                    () => _positionCtrl.onBoxClick.Invoke(_contentID));
        }

        /// <summary>
        /// Initialize the local position of the list box according to its ID
        /// </summary>
        private void InitialPosition()
        {
            var numOfBoxes = _positionCtrl.listBoxes.Length;
            var majorPosition = _unitPos * (listBoxID * -1 + numOfBoxes / 2);
            var passivePosition = 0f;

            // If there are even number of boxes, adjust the position one half unitPos down.
            if ((numOfBoxes & 0x1) == 0) {
                majorPosition =
                    _unitPos * (listBoxID * -1 + numOfBoxes / 2) - _unitPos / 2;
            }

            passivePosition = GetPassivePosition(majorPosition);

            switch (_positionCtrl.direction) {
                case ListPositionCtrl.Direction.Vertical:
                    transform.localPosition =
                        new Vector3(
                            passivePosition, majorPosition, transform.localPosition.z);
                    break;
                case ListPositionCtrl.Direction.Horizontal:
                    transform.localPosition =
                        new Vector3(
                            majorPosition, passivePosition, transform.localPosition.z);
                    break;
            }

            UpdateScale(majorPosition);
        }

        #endregion

        #region Position Controlling

        /// <summary>
        /// Move the box vertically and adjust its final position and size
        /// </summary>
        /// <param name="delta">The moving distance</param>
        private void MoveVertically(float delta)
        {
            var needToUpdateToLastContent = false;
            var needToUpdateToNextContent = false;
            var majorPosition = GetMajorPosition(transform.localPosition.y + delta,
                ref needToUpdateToLastContent, ref needToUpdateToNextContent);
            var passivePosition = GetPassivePosition(majorPosition);

            transform.localPosition =
                new Vector3(
                    passivePosition, majorPosition, transform.localPosition.z);
            UpdateScale(majorPosition);

            if (needToUpdateToLastContent)
                UpdateToLastContent();
            else if (needToUpdateToNextContent)
                UpdateToNextContent();
        }

        /// <summary>
        /// Move the box horizontally and adjust its final position and size
        /// </summary>
        /// <param name="delta">The moving distance</param>
        private void MoveHorizontally(float delta)
        {
            var needToUpdateToLastContent = false;
            var needToUpdateToNextContent = false;
            var majorPosition = GetMajorPosition(transform.localPosition.x + delta,
                ref needToUpdateToLastContent, ref needToUpdateToNextContent);
            var passivePosition = GetPassivePosition(majorPosition);

            transform.localPosition =
                new Vector3(
                    majorPosition, passivePosition, transform.localPosition.z);
            UpdateScale(majorPosition);

            if (needToUpdateToLastContent)
                UpdateToLastContent();
            else if (needToUpdateToNextContent)
                UpdateToNextContent();
        }

        /// <summary>
        /// Get the major position according to the requested position
        /// If the box exceeds the boundary, one of the passed flags will be set
        /// to indicate that the content needs to be updated.
        /// </summary>
        /// <param name="positionValue">The requested position</param>
        /// <param name="needToUpdateToLastContent">
        /// Does it need to update to the last content?
        /// </param>
        /// <param name="needToUpdateToNextContent">
        /// Does it need to update to the next content?
        /// </param>
        /// <returns>The final major position</returns>
        private float GetMajorPosition(
            float positionValue,
            ref bool needToUpdateToLastContent, ref bool needToUpdateToNextContent)
        {
            var beyondPos = 0.0f;
            var majorPos = positionValue;

            if (positionValue < _changeSideLowerBoundPos) {
                beyondPos = positionValue - _lowerBoundPos;
                majorPos = _upperBoundPos - _unitPos + beyondPos;
                needToUpdateToLastContent = true;
            } else if (positionValue > _changeSideUpperBoundPos) {
                beyondPos = positionValue - _upperBoundPos;
                majorPos = _lowerBoundPos + _unitPos + beyondPos;
                needToUpdateToNextContent = true;
            }

            return majorPos;
        }

        /// <summary>
        /// Get the passive position according to the major position
        /// </summary>
        /// <param name="majorPosition">The major position</param>
        /// <returns>The passive position</returns>
        private float GetPassivePosition(float majorPosition)
        {
            var passivePosFactor = _positionCurve.Evaluate(majorPosition);
            return _upperBoundPos * passivePosFactor;
        }

        /// <summary>
        /// Scale the listBox according to the major position
        /// </summary>
        private void UpdateScale(float majorPosition)
        {
            var scaleValue = _scaleCurve.Evaluate(majorPosition);
            transform.localScale =
                new Vector3(scaleValue, scaleValue, transform.localScale.z);
        }

        #endregion

        #region Content Handling

        /// <summary>
        /// Initialize the content of ListBox.
        /// </summary>
        private void InitialContent()
        {
            // Get the content ID of the centered box
            _contentID = _positionCtrl.centeredContentID;

            // Adjust the contentID according to its initial order.
            _contentID += listBoxID - _positionCtrl.listBoxes.Length / 2;

            // In the linear mode, disable the box if needed
            if (_positionCtrl.listType == ListPositionCtrl.ListType.Linear) {
                // Disable the boxes at the upper half of the list
                // which will hold the item at the tail of the contents.
                if (_contentID < 0) {
                    _positionCtrl.numOfUpperDisabledBoxes += 1;
                    gameObject.SetActive(false);
                }
                // Disable the box at the lower half of the list
                // which will hold the repeated item.
                else if (_contentID >= _listBank.GetListLength()) {
                    _positionCtrl.numOfLowerDisabledBoxes += 1;
                    gameObject.SetActive(false);
                }
            }

            // Round the content id
            while (_contentID < 0)
                _contentID += _listBank.GetListLength();
            _contentID = _contentID % _listBank.GetListLength();

            UpdateDisplayContent(GetListContent());
        }

        /// <summary>
        /// Update the content to the last content of the next ListBox
        /// </summary>
        private void UpdateToLastContent()
        {
            _contentID = nextListBox.GetContentID() - 1;
            _contentID = (_contentID < 0) ? _listBank.GetListLength() - 1 : _contentID;

            if (_positionCtrl.listType == ListPositionCtrl.ListType.Linear) {
                if (_contentID == _listBank.GetListLength() - 1 ||
                    !nextListBox.isActiveAndEnabled) {
                    // If the box has been disabled at the other side,
                    // decrease the counter of the other side.
                    if (!isActiveAndEnabled)
                        --_positionCtrl.numOfLowerDisabledBoxes;

                    // In linear mode, don't display the content of the other end
                    gameObject.SetActive(false);
                    ++_positionCtrl.numOfUpperDisabledBoxes;
                } else if (!isActiveAndEnabled) {
                    // The disabled boxes from the other end will be enabled again,
                    // if the next box is enabled.
                    gameObject.SetActive(true);
                    --_positionCtrl.numOfLowerDisabledBoxes;
                }
            }

            UpdateDisplayContent(GetListContent());
        }

        /// <summary>
        /// Update the content to the next content of the last ListBox
        /// </summary>
        private void UpdateToNextContent()
        {
            _contentID = lastListBox.GetContentID() + 1;
            _contentID = (_contentID == _listBank.GetListLength()) ? 0 : _contentID;

            if (_positionCtrl.listType == ListPositionCtrl.ListType.Linear) {
                if (_contentID == 0 || !lastListBox.isActiveAndEnabled) {
                    if (!isActiveAndEnabled)
                        --_positionCtrl.numOfUpperDisabledBoxes;

                    // In linear mode, don't display the content of the other end
                    gameObject.SetActive(false);
                    ++_positionCtrl.numOfLowerDisabledBoxes;
                } else if (!isActiveAndEnabled) {
                    gameObject.SetActive(true);
                    --_positionCtrl.numOfUpperDisabledBoxes;
                }
            }

            UpdateDisplayContent(GetListContent());
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
        /// Get the content of box's content ID from the list bank
        /// </summary>
        /// <returns>The object of the content</returns>
        private object GetListContent()
        {
            return _listBank.GetListContent(_contentID);
        }

        #endregion

    }
}
