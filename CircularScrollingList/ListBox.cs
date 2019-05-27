/* The basic component of scrolling list.
 * Control the position and the contents of the list element.
 *
 * Author: LanKuDot <airlanser@gmail.com>
 */
using UnityEngine;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
	public int listBoxID;   // Must be unique, and count from 0
	public Text content;        // The content of the list box

	public ListBox lastListBox;
	public ListBox nextListBox;

	private ListPositionCtrl _positionCtrl;
	private ListBank _listBank;
	private int _contentID;

	// All position calculations here are in the local space of the list
	private Vector2 _boxMaxPos;
	private Vector2 _unitPos;
	private Vector2 _lowerBoundPos;
	private Vector2 _upperBoundPos;
	private Vector2 _shiftBoundPos;
	private float _positionAdjust;

	private Vector3 _slidingDistance;   // The sliding distance at each frame
	private Vector3 _slidingDistanceLeft;

	private Vector3 _originalLocalScale;

	private bool _keepSliding = false;
	private int _slidingFramesLeft;
	private bool _needToAlignToCenter = false;

	public bool keepSliding { set { _keepSliding = value; } }
	public bool needToAlignToCenter { set { _needToAlignToCenter = value; } }

	/* Notice: ListBox will initialize its variables from ListPositionCtrl.
	 * Make sure that the execution order of script ListPositionCtrl is prior to
	 * ListBox.
	 */
	void Start()
	{
		_positionCtrl = transform.GetComponentInParent<ListPositionCtrl>();
		_listBank = transform.GetComponentInParent<ListBank>();

		_boxMaxPos = _positionCtrl.canvasMaxPos_L * _positionCtrl.listCurvature;
		_unitPos = _positionCtrl.unitPos_L;
		_lowerBoundPos = _positionCtrl.lowerBoundPos_L;
		_upperBoundPos = _positionCtrl.upperBoundPos_L;
		_shiftBoundPos = _positionCtrl.shiftBoundPos_L;
		_positionAdjust = _positionCtrl.positionAdjust;

		_originalLocalScale = transform.localScale;

		InitialPosition(listBoxID);
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

		UpdateListContent();
	}

	/* Update the dispalying content on the ListBox.
	 */
	void UpdateListContent()
	{
		// Update the content accroding to its contentID.
		content.text = _listBank.getListContent(_contentID);
	}

	/* Get the content of this ListBox
	 */
	public void GetContent()
	{
		Debug.Log("Box ID: " + listBoxID.ToString() +
			", Content ID: " + _contentID.ToString() +
			", Content: " + _listBank.getListContent(_contentID));
	}

	/* Make the list box slide for delta x or y position.
	 */
	public void SetSlidingDistance(Vector3 distance, int slidingFrames)
	{
		_keepSliding = true;
		_slidingFramesLeft = slidingFrames;

		_slidingDistanceLeft = distance;
		_slidingDistance = Vector3.Lerp(Vector3.zero, distance, _positionCtrl.boxSlidingSpeedFactor);
	}

	/* Move the listBox for world position unit.
	 * Move up when "up" is true, or else, move down.
	 */
	public void UnitMove(int unit, bool up_right)
	{
		Vector2 deltaPos;

		if (up_right)
			deltaPos = _unitPos * (float)unit;
		else
			deltaPos = _unitPos * (float)unit * -1;

		if (_keepSliding)
			deltaPos += (Vector2)_slidingDistanceLeft;

		switch (_positionCtrl.direction) {
		case ListPositionCtrl.Direction.Vertical:
			SetSlidingDistance(new Vector3(0.0f, deltaPos.y, 0.0f), _positionCtrl.boxSlidingFrames);
			break;
		case ListPositionCtrl.Direction.Horizontal:
			SetSlidingDistance(new Vector3(deltaPos.x, 0.0f, 0.0f), _positionCtrl.boxSlidingFrames);
			break;
		}
	}

	void Update()
	{
		if (_keepSliding) {
			--_slidingFramesLeft;
			if (_slidingFramesLeft == 0) {
				_keepSliding = false;

				// Set the distance to the center after free sliding.
				if (_needToAlignToCenter) {
					SetSlidingDistance(_positionCtrl.FindDeltaPositionToCenter(),
						_positionCtrl.boxSlidingFrames);
					_needToAlignToCenter = false;
					return;
				}

				// At the last sliding frame, move to that position.
				// At free moving mode, this function is disabled.
				if (_positionCtrl.alignToCenter ||
					_positionCtrl.controlMode == ListPositionCtrl.ControlMode.Button ||
					_positionCtrl.controlMode == ListPositionCtrl.ControlMode.MouseWheel) {
					UpdatePosition(_slidingDistanceLeft);
				}
				return;
			}

			UpdatePosition(_slidingDistance);
			_slidingDistanceLeft -= _slidingDistance;
			_slidingDistance = Vector3.Lerp(Vector3.zero, _slidingDistanceLeft, _positionCtrl.boxSlidingSpeedFactor);
		}
	}

	/* Initialize the local position of the list box accroding to its ID.
	 */
	void InitialPosition(int listBoxID)
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
				transform.localPosition = new Vector3(_unitPos.x * (listBoxID - _positionCtrl.listBoxes.Length / 2) - _unitPos.x / 2,
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
				transform.localPosition = new Vector3(_unitPos.x * (listBoxID - _positionCtrl.listBoxes.Length / 2),
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
	 * Formula: x = boxMax_x * (cos( radian controlled by y ) - positionAdjust)
	 * radian = (y / upper_y) * pi / 2, so the range of radian is from pi/2 to 0 to -pi/2,
	 * and corresponding cosine value is from 0 to 1 to 0.
	 */
	void UpdateXPosition()
	{
		transform.localPosition = new Vector3(
			_boxMaxPos.x * (_positionAdjust +
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
			_boxMaxPos.y * (_positionAdjust +
			Mathf.Cos(transform.localPosition.x / _upperBoundPos.x * Mathf.PI / 2.0f)),
			transform.localPosition.z);
		UpdateSize(_upperBoundPos.x, transform.localPosition.x);
	}

	/* Check if the ListBox is beyond the upper or lower bound or not.
	 * If does, move the ListBox to the other side and update the content.
	 */
	void CheckBoundaryY()
	{
		float beyondPosY_L = 0.0f;

		// Narrow the checking boundary in order to avoid the list swaying to one side
		if (transform.localPosition.y < _lowerBoundPos.y + _shiftBoundPos.y) {
			beyondPosY_L = (_lowerBoundPos.y + _shiftBoundPos.y - transform.localPosition.y);
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				_upperBoundPos.y - _unitPos.y + _shiftBoundPos.y - beyondPosY_L,
				transform.localPosition.z);
			UpdateToLastContent();
		} else if (transform.localPosition.y > _upperBoundPos.y - _shiftBoundPos.y) {
			beyondPosY_L = (transform.localPosition.y - _upperBoundPos.y + _shiftBoundPos.y);
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				_lowerBoundPos.y + _unitPos.y - _shiftBoundPos.y + beyondPosY_L,
				transform.localPosition.z);
			UpdateToNextContent();
		}

		UpdateXPosition();
	}

	void CheckBoundaryX()
	{
		float beyondPosX_L = 0.0f;

		// Narrow the checking boundary in order to avoid the list swaying to one side
		if (transform.localPosition.x < _lowerBoundPos.x + _shiftBoundPos.x) {
			beyondPosX_L = (_lowerBoundPos.x + _shiftBoundPos.x - transform.localPosition.x);
			transform.localPosition = new Vector3(
				_upperBoundPos.x - _unitPos.x + _shiftBoundPos.x - beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z);
			UpdateToNextContent();
		} else if (transform.localPosition.x > _upperBoundPos.x - _shiftBoundPos.x) {
			beyondPosX_L = (transform.localPosition.x - _upperBoundPos.x + _shiftBoundPos.x);
			transform.localPosition = new Vector3(
				_lowerBoundPos.x + _unitPos.x - _shiftBoundPos.x + beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z);
			UpdateToLastContent();
		}

		UpdateYPosition();
	}

	/* Scale the size of listBox accroding to the position.
	 */
	void UpdateSize(float smallest_at, float target_value)
	{
		transform.localScale = _originalLocalScale *
			(1.0f + _positionCtrl.centerBoxScaleRatio * Mathf.InverseLerp(smallest_at, 0.0f, Mathf.Abs(target_value)));
	}

	public int GetCurrentContentID()
	{
		return _contentID;
	}

	/* Update to the last content of the next ListBox
	 * when the ListBox appears at the top of camera.
	 */
	void UpdateToLastContent()
	{
		_contentID = nextListBox.GetCurrentContentID() - 1;
		_contentID = (_contentID < 0) ? _listBank.getListLength() - 1 : _contentID;

		UpdateListContent();
	}

	/* Update to the next content of the last ListBox
	 * when the ListBox appears at the bottom of camera.
	 */
	void UpdateToNextContent()
	{
		_contentID = lastListBox.GetCurrentContentID() + 1;
		_contentID = (_contentID == _listBank.getListLength()) ? 0 : _contentID;

		UpdateListContent();
	}
}
