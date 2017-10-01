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
		_boxMaxPos = ListPositionCtrl.Instance.canvasMaxPos_L * ListPositionCtrl.Instance.angularity;
		_unitPos = ListPositionCtrl.Instance.unitPos_L;
		_lowerBoundPos = ListPositionCtrl.Instance.lowerBoundPos_L;
		_upperBoundPos = ListPositionCtrl.Instance.upperBoundPos_L;
		_shiftBoundPos = ListPositionCtrl.Instance.shiftBoundPos_L;
		_positionAdjust = ListPositionCtrl.Instance.positionAdjust;

		_originalLocalScale = transform.localScale;

		initialPosition(listBoxID);
		initialContent();
	}

	/* Initialize the content of ListBox.
	 */
	void initialContent()
	{
		if (listBoxID == ListPositionCtrl.Instance.listBoxes.Length / 2)
			_contentID = 0;
		else if (listBoxID < ListPositionCtrl.Instance.listBoxes.Length / 2)
			_contentID = ListBank.Instance.getListLength() - (ListPositionCtrl.Instance.listBoxes.Length / 2 - listBoxID);
		else
			_contentID = listBoxID - ListPositionCtrl.Instance.listBoxes.Length / 2;

		while (_contentID < 0)
			_contentID += ListBank.Instance.getListLength();
		_contentID = _contentID % ListBank.Instance.getListLength();

		updateContent(ListBank.Instance.getListContent(_contentID));
	}

	void updateContent(string content)
	{
		this.content.text = content;
	}

	/* Make the list box slide for delta x or y position.
	 */
	public void setSlidingDistance(Vector3 distance, int slidingFrames)
	{
		_keepSliding = true;
		_slidingFramesLeft = slidingFrames;

		_slidingDistanceLeft = distance;
		_slidingDistance = Vector3.Lerp(Vector3.zero, distance, ListPositionCtrl.Instance.slidingFactor);
	}

	/* Move the listBox for world position unit.
	 * Move up when "up" is true, or else, move down.
	 */
	public void unitMove(int unit, bool up_right)
	{
		Vector2 deltaPos;

		if (up_right)
			deltaPos = _unitPos * (float)unit;
		else
			deltaPos = _unitPos * (float)unit * -1;

		switch (ListPositionCtrl.Instance.direction) {
		case ListPositionCtrl.Direction.VERTICAL:
			setSlidingDistance(new Vector3(0.0f, deltaPos.y, 0.0f), ListPositionCtrl.Instance.slidingFrames);
			break;
		case ListPositionCtrl.Direction.HORIZONTAL:
			setSlidingDistance(new Vector3(deltaPos.x, 0.0f, 0.0f), ListPositionCtrl.Instance.slidingFrames);
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
					setSlidingDistance(ListPositionCtrl.Instance.findDeltaPositionToCenter(),
						ListPositionCtrl.Instance.slidingFrames);
					_needToAlignToCenter = false;
					return;
				}

				// At the last sliding frame, move to that position.
				// At free moving mode, this function is disabled.
				if (ListPositionCtrl.Instance.alignToCenter ||
					ListPositionCtrl.Instance.controlByButton) {
					updatePosition(_slidingDistanceLeft);
				}
				return;
			}

			updatePosition(_slidingDistance);
			_slidingDistanceLeft -= _slidingDistance;
			_slidingDistance = Vector3.Lerp(Vector3.zero, _slidingDistanceLeft, ListPositionCtrl.Instance.slidingFactor);
		}
	}

	/* Initialize the local position of the list box accroding to its ID.
	 */
	void initialPosition(int listBoxID)
	{
		// If there are even number of ListBoxes, adjust the initial position by an half unitPos.
		if ((ListPositionCtrl.Instance.listBoxes.Length & 0x1) == 0) {
			switch (ListPositionCtrl.Instance.direction) {
			case ListPositionCtrl.Direction.VERTICAL:
				transform.localPosition = new Vector3(0.0f,
					_unitPos.y * (listBoxID * -1 + ListPositionCtrl.Instance.listBoxes.Length / 2) - _unitPos.y / 2,
					0.0f);
				updateXPosition();
				break;
			case ListPositionCtrl.Direction.HORIZONTAL:
				transform.localPosition = new Vector3(_unitPos.x * (listBoxID - ListPositionCtrl.Instance.listBoxes.Length / 2) - _unitPos.x / 2,
				0.0f, 0.0f);
				updateYPosition();
				break;
			}
		} else {
			switch (ListPositionCtrl.Instance.direction) {
			case ListPositionCtrl.Direction.VERTICAL:
				transform.localPosition = new Vector3(0.0f,
					_unitPos.y * (listBoxID * -1 + ListPositionCtrl.Instance.listBoxes.Length / 2),
					0.0f);
				updateXPosition();
				break;
			case ListPositionCtrl.Direction.HORIZONTAL:
				transform.localPosition = new Vector3(_unitPos.x * (listBoxID - ListPositionCtrl.Instance.listBoxes.Length / 2),
					0.0f, 0.0f);
				updateYPosition();
				break;
			}
		}
	}

	/* Update the local position of ListBox accroding to the delta position at each frame.
	 * Note that the deltaPosition must be in local space.
	 */
	public void updatePosition(Vector3 deltaPosition_L)
	{
		switch (ListPositionCtrl.Instance.direction) {
		case ListPositionCtrl.Direction.VERTICAL:
			transform.localPosition += new Vector3(0.0f, deltaPosition_L.y, 0.0f);
			updateXPosition();
			checkBoundaryY();
			break;
		case ListPositionCtrl.Direction.HORIZONTAL:
			transform.localPosition += new Vector3(deltaPosition_L.x, 0.0f, 0.0f);
			updateYPosition();
			checkBoundaryX();
			break;
		}
	}

	/* Calculate the x position accroding to the y position.
	 * Formula: x = boxMax_x * (cos( radian controlled by y ) - positionAdjust)
	 * radian = (y / upper_y) * pi / 2, so the range of radian is from pi/2 to 0 to -pi/2,
	 * and corresponding cosine value is from 0 to 1 to 0.
	 */
	void updateXPosition()
	{
		transform.localPosition = new Vector3(
			_boxMaxPos.x * (_positionAdjust +
			Mathf.Cos(transform.localPosition.y / _upperBoundPos.y * Mathf.PI / 2.0f)),
			transform.localPosition.y, transform.localPosition.z);
		updateSize(_upperBoundPos.y, transform.localPosition.y);
	}

	/* Calculate the y position accroding to the x position.
	 */
	void updateYPosition()
	{
		transform.localPosition = new Vector3(
			transform.localPosition.x,
			_boxMaxPos.y * (_positionAdjust +
			Mathf.Cos(transform.localPosition.x / _upperBoundPos.x * Mathf.PI / 2.0f)),
			transform.localPosition.z);
		updateSize(_upperBoundPos.x, transform.localPosition.x);
	}

	/* Check if the ListBox is beyond the upper or lower bound or not.
	 * If does, move the ListBox to the other side and update the content.
	 */
	void checkBoundaryY()
	{
		float beyondPosY_L = 0.0f;

		// Narrow the checking boundary in order to avoid the list swaying to one side
		if (transform.localPosition.y < _lowerBoundPos.y + _shiftBoundPos.y) {
			beyondPosY_L = (_lowerBoundPos.y + _shiftBoundPos.y - transform.localPosition.y);
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				_upperBoundPos.y - _unitPos.y + _shiftBoundPos.y - beyondPosY_L,
				transform.localPosition.z);
			updateToLastContent();
		} else if (transform.localPosition.y > _upperBoundPos.y - _shiftBoundPos.y) {
			beyondPosY_L = (transform.localPosition.y - _upperBoundPos.y + _shiftBoundPos.y);
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				_lowerBoundPos.y + _unitPos.y - _shiftBoundPos.y + beyondPosY_L,
				transform.localPosition.z);
			updateToNextContent();
		}

		updateXPosition();
	}

	void checkBoundaryX()
	{
		float beyondPosX_L = 0.0f;

		// Narrow the checking boundary in order to avoid the list swaying to one side
		if (transform.localPosition.x < _lowerBoundPos.x + _shiftBoundPos.x) {
			beyondPosX_L = (_lowerBoundPos.x + _shiftBoundPos.x - transform.localPosition.x);
			transform.localPosition = new Vector3(
				_upperBoundPos.x - _unitPos.x + _shiftBoundPos.x - beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z);
			updateToNextContent();
		} else if (transform.localPosition.x > _upperBoundPos.x - _shiftBoundPos.x) {
			beyondPosX_L = (transform.localPosition.x - _upperBoundPos.x + _shiftBoundPos.x);
			transform.localPosition = new Vector3(
				_lowerBoundPos.x + _unitPos.x - _shiftBoundPos.x + beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z);
			updateToLastContent();
		}

		updateYPosition();
	}

	/* Scale the size of listBox accroding to the position.
	 */
	void updateSize(float smallest_at, float target_value)
	{
		transform.localScale = _originalLocalScale *
			(1.0f + ListPositionCtrl.Instance.scaleFactor * Mathf.InverseLerp(smallest_at, 0.0f, Mathf.Abs(target_value)));
	}

	public int getCurrentContentID()
	{
		return _contentID;
	}

	/* Update to the last content of the next ListBox
	 * when the ListBox appears at the top of camera.
	 */
	void updateToLastContent()
	{
		_contentID = nextListBox.getCurrentContentID() - 1;
		_contentID = (_contentID < 0) ? ListBank.Instance.getListLength() - 1 : _contentID;

		updateContent(ListBank.Instance.getListContent(_contentID));
	}

	/* Update to the next content of the last ListBox
	 * when the ListBox appears at the bottom of camera.
	 */
	void updateToNextContent()
	{
		_contentID = lastListBox.getCurrentContentID() + 1;
		_contentID = (_contentID == ListBank.Instance.getListLength()) ? 0 : _contentID;

		updateContent(ListBank.Instance.getListContent(_contentID));
	}
}
