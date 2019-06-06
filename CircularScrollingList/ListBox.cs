/* The basic component of the scrolling list.
 * Control the position and update the content of the list element.
 */

using UnityEngine;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
	public Text content;    // The display text for the content of the list box

	// These public variables will be initialized
	// in ListPositionCtrl.InitializeBoxDependency().
	[HideInInspector]
	public int listBoxID;   // The same as the order in the `listBoxes`
	[HideInInspector]
	public ListBox lastListBox;
	[HideInInspector]
	public ListBox nextListBox;

	private ListPositionCtrl _positionCtrl;
	private BaseListBank _listBank;
	private int _contentID;

	/* ====== Position variables ====== */
	// Position caculated here is in the local space of the list
	private Vector2 _maxCurvePos;     // The maximum outer position
	private Vector2 _unitPos;         // The distance between boxes
	private Vector2 _lowerBoundPos;   // The left/down-most position of the box
	private Vector2 _upperBoundPos;   // The right/up-most position of the box
	// _changeSide(Lower/Upper)BoundPos is the boundary for checking that
	// whether to move the box to the other end or not
	private Vector2 _changeSideLowerBoundPos;
	private Vector2 _changeSideUpperBoundPos;
	private float _cosValueAdjust;

	private Vector3 _slidingDistance;   // The sliding distance for each frame
	private Vector3 _slidingDistanceLeft;

	private Vector3 _initialLocalScale;

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

	/* Notice: ListBox will initialize its variables from ListPositionCtrl.
	 * Make sure that the execution order of script ListPositionCtrl is prior to
	 * ListBox.
	 */
	void Start()
	{
		_positionCtrl = transform.GetComponentInParent<ListPositionCtrl>();
		_listBank = _positionCtrl.listBank;

		_maxCurvePos = _positionCtrl.canvasMaxPos_L * _positionCtrl.listCurvature;
		_unitPos = _positionCtrl.unitPos_L;
		_lowerBoundPos = _positionCtrl.lowerBoundPos_L;
		_upperBoundPos = _positionCtrl.upperBoundPos_L;
		_changeSideLowerBoundPos = _lowerBoundPos + _unitPos * 0.3f;
		_changeSideUpperBoundPos = _upperBoundPos - _unitPos * 0.3f;
		_cosValueAdjust = _positionCtrl.positionAdjust;

		_initialLocalScale = transform.localScale;

		InitialPosition();
		InitialContent();
		AddClickEvent();
	}

	/* Add an additional listener to Button.onClick event for passing the content ID
	 * of the clicked box to the event handlers registered at ListPositionCtrl.onBoxClick
	 */
	void AddClickEvent()
	{
		Button button = transform.GetComponent<Button>();
		if (button != null)
			button.onClick.AddListener(() => _positionCtrl.onBoxClick.Invoke(_contentID));
	}

	/* Initialize the content of ListBox.
	 */
	void InitialContent()
	{
		// Get the content ID of the centered box
		_contentID = _positionCtrl.centeredContentID;

		// Adjust the contentID accroding to its initial order.
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

	/* Update the dispalying content on the ListBox.
	 */
	void UpdateDisplayContent()
	{
		// Update the content accroding to its contentID.
		content.text = _listBank.GetListContent(_contentID);
	}

	/* Initialize the local position of the list box accroding to its ID
	 */
	void InitialPosition()
	{
		// If there are even number of ListBoxes, adjust the initial position by an half unitPos.
		if ((_positionCtrl.listBoxes.Length & 0x1) == 0) {
			switch (_positionCtrl.direction) {
				case ListPositionCtrl.Direction.Vertical:
					transform.localPosition = new Vector3(0.0f,
						_unitPos.y * (listBoxID * -1 + _positionCtrl.listBoxes.Length / 2) - _unitPos.y / 2,
						0.0f);
					UpdateXPosition();
					break;
				case ListPositionCtrl.Direction.Horizontal:
					transform.localPosition = new Vector3(
						_unitPos.x * (listBoxID - _positionCtrl.listBoxes.Length / 2) - _unitPos.x / 2,
						0.0f, 0.0f);
					UpdateYPosition();
					break;
			}
		} else {
			switch (_positionCtrl.direction) {
				case ListPositionCtrl.Direction.Vertical:
					transform.localPosition = new Vector3(0.0f,
						_unitPos.y * (listBoxID * -1 + _positionCtrl.listBoxes.Length / 2),
						0.0f);
					UpdateXPosition();
					break;
				case ListPositionCtrl.Direction.Horizontal:
					transform.localPosition = new Vector3(
						_unitPos.x * (listBoxID - _positionCtrl.listBoxes.Length / 2),
						0.0f, 0.0f);
					UpdateYPosition();
					break;
			}
		}
	}

	/* Update the local position of ListBox accroding to the delta position at each frame.
	 * Note that the deltaPosition must be in local space.
	 */
	public void UpdatePosition(Vector3 deltaPosition_L)
	{
		switch (_positionCtrl.direction) {
			case ListPositionCtrl.Direction.Vertical:
				transform.localPosition += new Vector3(0.0f, deltaPosition_L.y, 0.0f);
				CheckBoundaryY();
				UpdateXPosition();
				break;
			case ListPositionCtrl.Direction.Horizontal:
				transform.localPosition += new Vector3(deltaPosition_L.x, 0.0f, 0.0f);
				CheckBoundaryX();
				UpdateYPosition();
				break;
		}
	}

	/* Calculate the x position accroding to the y position.
	 */
	void UpdateXPosition()
	{
		// Formula: x = maxCurvePos_x * (cos(r) + cosValueAdjust),
		// where r = (y / upper_y) * pi / 2, then r is in range [- pi / 2, pi / 2],
		// and corresponding cosine value is from 0 to 1 to 0.
		transform.localPosition = new Vector3(
			_maxCurvePos.x * (_cosValueAdjust +
			Mathf.Cos(transform.localPosition.y / _upperBoundPos.y * Mathf.PI / 2.0f)),
			transform.localPosition.y, transform.localPosition.z);
		UpdateSize(_upperBoundPos.y, transform.localPosition.y);
	}

	/* Calculate the y position accroding to the x position.
	 */
	void UpdateYPosition()
	{
		transform.localPosition = new Vector3(
			transform.localPosition.x,
			_maxCurvePos.y * (_cosValueAdjust +
			Mathf.Cos(transform.localPosition.x / _upperBoundPos.x * Mathf.PI / 2.0f)),
			transform.localPosition.z);
		UpdateSize(_upperBoundPos.x, transform.localPosition.x);
	}

	/* Check if the ListBox is beyond the checking boundary or not
	 * If it does, move the ListBox to the other end of the list
	 * and update the content.
	 */
	void CheckBoundaryY()
	{
		float beyondPosY_L = 0.0f;

		if (transform.localPosition.y < _changeSideLowerBoundPos.y) {
			beyondPosY_L = transform.localPosition.y - _lowerBoundPos.y;
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				_upperBoundPos.y - _unitPos.y + beyondPosY_L,
				transform.localPosition.z);
			UpdateToLastContent();
		} else if (transform.localPosition.y > _changeSideUpperBoundPos.y) {
			beyondPosY_L = transform.localPosition.y - _upperBoundPos.y;
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				_lowerBoundPos.y + _unitPos.y + beyondPosY_L,
				transform.localPosition.z);
			UpdateToNextContent();
		}
	}

	void CheckBoundaryX()
	{
		float beyondPosX_L = 0.0f;

		if (transform.localPosition.x < _changeSideLowerBoundPos.x) {
			beyondPosX_L = transform.localPosition.x - _lowerBoundPos.x;
			transform.localPosition = new Vector3(
				_upperBoundPos.x - _unitPos.x + beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z);
			UpdateToNextContent();
		} else if (transform.localPosition.x > _changeSideUpperBoundPos.x) {
			beyondPosX_L = transform.localPosition.x - _upperBoundPos.x;
			transform.localPosition = new Vector3(
				_lowerBoundPos.x + _unitPos.x + beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z);
			UpdateToLastContent();
		}
	}

	/* Scale the listBox accroding to its position
	 *
	 * @param smallest_at The position at where the smallest listBox will be
	 * @param target_value The position of the target listBox
	 */
	void UpdateSize(float smallest_at, float target_value)
	{
		// The scale of the box at the either end is initialLocalScale.
		// The scale of the box at the center is initialLocalScale * (1 + centerBoxScaleRatio).
		transform.localScale = _initialLocalScale *
			(1.0f + _positionCtrl.centerBoxScaleRatio *
			 Mathf.InverseLerp(smallest_at, 0.0f, Mathf.Abs(target_value)));
	}

	public int GetCurrentContentID()
	{
		return _contentID;
	}

	/* Update the content to the last content of the next ListBox
	 */
	void UpdateToLastContent()
	{
		_contentID = nextListBox.GetCurrentContentID() - 1;
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
	void UpdateToNextContent()
	{
		_contentID = lastListBox.GetCurrentContentID() + 1;
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
}
