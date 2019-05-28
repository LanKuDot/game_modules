/* The basic component of the scrolling list.
 * Control the position and update the content of the list element.
 */

using UnityEngine;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
	public int listBoxID;   // Must be unique, and count from 0
	public Text content;    // The display text for the content of the list box

	public ListBox lastListBox;
	public ListBox nextListBox;

	private ListPositionCtrl _positionCtrl;
	private ListBank _listBank;
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

	/* Get the content of this ListBox
	 */
	public void GetContent()
	{
		Debug.Log("Box ID: " + listBoxID.ToString() +
			", Content ID: " + _contentID.ToString() +
			", Content: " + _listBank.getListContent(_contentID));
	}

	/* Notice: ListBox will initialize its variables from ListPositionCtrl.
	 * Make sure that the execution order of script ListPositionCtrl is prior to
	 * ListBox.
	 */
	void Start()
	{
		_positionCtrl = transform.GetComponentInParent<ListPositionCtrl>();
		_listBank = transform.GetComponentInParent<ListBank>();

		_maxCurvePos = _positionCtrl.canvasMaxPos_L * _positionCtrl.listCurvature;
		_unitPos = _positionCtrl.unitPos_L;
		_lowerBoundPos = _positionCtrl.lowerBoundPos_L;
		_upperBoundPos = _positionCtrl.upperBoundPos_L;
		_changeSideLowerBoundPos = _lowerBoundPos + _unitPos * 0.3f;
		_changeSideUpperBoundPos = _upperBoundPos - _unitPos * 0.3f;
		_cosValueAdjust = _positionCtrl.positionAdjust;

		_initialLocalScale = transform.localScale;

		InitialPosition();
		// CUSTOM: Put in the centered ID you want to show
		InitialContent(0);
	}

	/* Initialize the content of ListBox.
	 * centeredContentID is used for setting the content of the centered ListBox.
	 */
	void InitialContent(int centeredContentID)
	{
		_contentID = centeredContentID;

		// Adjust the contentID accroding to its initial order.
		if (listBoxID < _positionCtrl.listBoxes.Length / 2)
			_contentID += _listBank.getListLength() - (_positionCtrl.listBoxes.Length / 2 - listBoxID);
		else
			_contentID += listBoxID - _positionCtrl.listBoxes.Length / 2;

		while (_contentID < 0)
			_contentID += _listBank.getListLength();
		_contentID = _contentID % _listBank.getListLength();

		UpdateDisplayContent();
	}

	/* Update the dispalying content on the ListBox.
	 */
	void UpdateDisplayContent()
	{
		// Update the content accroding to its contentID.
		content.text = _listBank.getListContent(_contentID);
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
				UpdateXPosition();
				CheckBoundaryY();
				break;
			case ListPositionCtrl.Direction.Horizontal:
				transform.localPosition += new Vector3(deltaPosition_L.x, 0.0f, 0.0f);
				UpdateYPosition();
				CheckBoundaryX();
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

		UpdateXPosition();
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

		UpdateYPosition();
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
		_contentID = (_contentID < 0) ? _listBank.getListLength() - 1 : _contentID;

		UpdateDisplayContent();
	}

	/* Update the content to the next content of the last ListBox
	 */
	void UpdateToNextContent()
	{
		_contentID = lastListBox.GetCurrentContentID() + 1;
		_contentID = (_contentID == _listBank.getListLength()) ? 0 : _contentID;

		UpdateDisplayContent();
	}
}
