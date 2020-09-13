/* The basic component of the scrolling list.
 * Control the position and update the content of the list element.
 */

using System;
using UnityEngine;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
	public Text content; // The display text for the content of the list box

	// These public variables will be initialized
	// in ListPositionCtrl.InitializeBoxDependency().
	[HideInInspector] public int listBoxID; // The same as the order in the `listBoxes`
	[HideInInspector] public ListBox lastListBox;
	[HideInInspector] public ListBox nextListBox;
	private int _contentID;

	private ListPositionCtrl _positionCtrl;
	private BaseListBank _listBank;
	private CurveResolver _positionCurve;
	private CurveResolver _scaleCurve;

	public Action<float> UpdatePosition { private set; get; }

	/* ====== Position variables ====== */
	// Position calculated here is in the local space of the list
	private float _unitPos; // The distance between boxes
	private float _lowerBoundPos; // The left/down-most position of the box
	private float _upperBoundPos; // The right/up-most position of the box
	// _changeSide(Lower/Upper)BoundPos is the boundary for checking that
	// whether to move the box to the other end or not
	private float _changeSideLowerBoundPos;
	private float _changeSideUpperBoundPos;

	/* Output the information of the box to the Debug.Log
	 */
	public void ShowBoxInfo()
	{
		Debug.Log("Box ID: " + listBoxID.ToString() +
		          ", Content ID: " + _contentID.ToString() +
		          ", Content: " + _listBank.GetListContent(_contentID));
	}

	/* Get the content ID of the box
	 */
	public int GetContentID()
	{
		return _contentID;
	}

	/* Initialize the box.
	 */
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

	/* Add an additional listener to Button.onClick event for passing the content ID
	 * of the clicked box to the event handlers registered at ListPositionCtrl.onBoxClick
	 */
	private void AddClickEvent()
	{
		Button button = transform.GetComponent<Button>();
		if (button != null)
			button.onClick.AddListener(() => _positionCtrl.onBoxClick.Invoke(_contentID));
	}

	/* Initialize the local position of the list box according to its ID
	 */
	private void InitialPosition()
	{
		int numOfBoxes = _positionCtrl.listBoxes.Length;
		float majorPosition = _unitPos * (listBoxID * -1 + numOfBoxes / 2);
		float passivePosition;

		// If there are even number of boxes, adjust the position one half unitPos down.
		if ((numOfBoxes & 0x1) == 0) {
			majorPosition = _unitPos * (listBoxID * -1 + numOfBoxes / 2) - _unitPos / 2;
		}

		passivePosition = GetPassivePosition(majorPosition);

		switch (_positionCtrl.direction) {
			case ListPositionCtrl.Direction.Vertical:
				transform.localPosition = new Vector3(
					passivePosition, majorPosition, transform.localPosition.z);
				break;
			case ListPositionCtrl.Direction.Horizontal:
				transform.localPosition = new Vector3(
					majorPosition, passivePosition, transform.localPosition.z);
				break;
		}

		UpdateScale(majorPosition);
	}

	/* Move the box vertically and adjust its final position and size.
	 *
	 * This function is the UpdatePosition in the vertical mode.
	 *
	 * @param delta The moving distance
	 */
	private void MoveVertically(float delta)
	{
		bool needToUpdateToLastContent = false;
		bool needToUpdateToNextContent = false;
		float majorPosition = GetMajorPosition(transform.localPosition.y + delta,
			ref needToUpdateToLastContent, ref needToUpdateToNextContent);
		float passivePosition = GetPassivePosition(majorPosition);

		transform.localPosition = new Vector3(
			passivePosition, majorPosition, transform.localPosition.z);
		UpdateScale(majorPosition);

		if (needToUpdateToLastContent)
			UpdateToLastContent();
		else if (needToUpdateToNextContent)
			UpdateToNextContent();
	}

	/* Move the box horizontally and adjust its final position and size.
	 *
	 * This function is the UpdatePosition in the horizontal mode.
	 *
	 * @param delta The moving distance
	 */
	private void MoveHorizontally(float delta)
	{
		bool needToUpdateToLastContent = false;
		bool needToUpdateToNextContent = false;
		float majorPosition = GetMajorPosition(transform.localPosition.x + delta,
			ref needToUpdateToLastContent, ref needToUpdateToNextContent);
		float passivePosition = GetPassivePosition(majorPosition);

		transform.localPosition = new Vector3(
			majorPosition, passivePosition, transform.localPosition.z);
		UpdateScale(majorPosition);

		if (needToUpdateToLastContent)
			UpdateToLastContent();
		else if (needToUpdateToNextContent)
			UpdateToNextContent();
	}

	/* Get the major position according to the requested position
	 * If the box exceeds the boundary, one of the passed flags will be set
	 * to indicate that the content needs to be updated.
	 *
	 * @param positionValue The requested position
	 * @param needToUpdateToLastContent Is it need to update to the last content?
	 * @param needToUpdateToNextContent Is it need to update to the next content?
	 * @return The decided major position
	 */
	private float GetMajorPosition(float positionValue,
		ref bool needToUpdateToLastContent, ref bool needToUpdateToNextContent)
	{
		float beyondPos = 0.0f;
		float majorPos = positionValue;

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

	/* Get the passive position according to the major position
	 */
	private float GetPassivePosition(float majorPosition)
	{
		float passivePosFactor = _positionCurve.Evaluate(majorPosition);
		return _upperBoundPos * passivePosFactor;
	}

	/* Scale the listBox according to the major position
	 */
	private void UpdateScale(float majorPosition)
	{
		float scaleValue = _scaleCurve.Evaluate(majorPosition);
		transform.localScale = new Vector3(scaleValue, scaleValue, transform.localScale.z);
	}

	/* Initialize the content of ListBox.
	 */
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

		UpdateDisplayContent();
	}

	/* Update the displaying content on the ListBox.
	 */
	private void UpdateDisplayContent()
	{
		// Update the content according to its contentID.
		content.text = _listBank.GetListContent(_contentID);
	}

	/* Update the content to the last content of the next ListBox
	 */
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

		UpdateDisplayContent();
	}

	/* Update the content to the next content of the last ListBox
	 */
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

		UpdateDisplayContent();
	}

	/* The class for converting the custom range to fit the AnimationCurve for
	 * evaluating the final value.
	 */
	private class CurveResolver
	{
		private AnimationCurve _curve;
		private float _maxValue;
		private float _minValue;

		/* Constructor
		 *
		 * @param curve The target AnimationCurve to fit
		 * @param minValue The custom minimum value
		 * @param maxValue The custom maximum value
		 */
		public CurveResolver(AnimationCurve curve, float minValue, float maxValue)
		{
			_curve = curve;
			_minValue = minValue;
			_maxValue = maxValue;
		}

		/* Convert the input value to the value of interpolation between [minValue, maxValue]
		 * and pass the result to the curve to get the final value.
		 */
		public float Evaluate(float value)
		{
			float lerpValue = Mathf.InverseLerp(_minValue, _maxValue, value);
			return _curve.Evaluate(lerpValue);
		}
	}
}
