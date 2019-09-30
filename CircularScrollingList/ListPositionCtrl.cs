/* Handle the controlling event and send the moving information to the boxes it has
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IControlEventHandler:
	IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{}

[System.Serializable]
public class ListBoxClickEvent : UnityEvent<int>
{}

public class ListPositionCtrl : MonoBehaviour, IControlEventHandler
{
	public enum ListType
	{
		Circular,
		Linear
	};

	public enum ControlMode
	{
		Drag,       // By the mouse pointer or finger
		Button,     // By the up/down button
		MouseWheel  // By the mouse wheel
	};

	public enum Direction
	{
		Vertical,
		Horizontal
	};

	/*========== Settings ==========*/
	/* List mode */
	public ListType listType = ListType.Circular;
	public ControlMode controlMode = ControlMode.Drag;
	public bool alignMiddle = false;
	public Direction direction = Direction.Vertical;

	/* Containers */
	public BaseListBank listBank;
	// Specify the centered content ID
	public int centeredContentID = 0;
	public ListBox[] listBoxes;
	// The event handler for the onClick event of the list box (containing Button component)
	// The handler function must have 1 int parameter for passing the content ID
	// of the clicked box.
	public ListBoxClickEvent onBoxClick;
	public Button[] controlButtons;

	/* Parameters */
	// Set the distance between each ListBox. The larger, the closer.
	public float boxGapFactor = 2.0f;
	// Set the sliding duration in frames. The larger, the longer.
	public int boxSlidingFrames = 35;
	// Set the sliding speed. The larger, the quicker.
	[Range(0.0f, 1.0f)]
	public float boxSlidingSpeedFactor = 0.2f;
	// Set the scrolling list curving to left/right, or up/down in HORIZONTAL mode.
	// Positive: Curve to right (up); Negative: Curve to left (down).
	[Range(-1.0f, 1.0f)]
	public float listCurvature = 0.3f;
	// Set this value to make the whole list not to sway to one side.
	// Adjust the horizontal position in the Vertical mode or
	// the vertical position in the Horizontal mode.
	// This value will be used in ListBox.update[X/Y]Position().
	[Range(-1.0f, 1.0f)]
	public float positionAdjust = -0.7f;
	// Set the scale ratio of the center listBox.
	public float centerBoxScaleRatio = 0.32f;
	/*===============================*/

	// The canvas plane which the scrolling list is at.
	private Canvas _parentCanvas;

	// The constrains of position in the local space of the canvas plane.
	public Vector2 canvasMaxPos_L { get; private set; }
	public Vector2 unitPos_L { get; private set; }
	public Vector2 lowerBoundPos_L { get; private set; }
	public Vector2 upperBoundPos_L { get; private set; }

	// Delegate functions
	private delegate void InputPositionHandlerDelegate(
		PointerEventData pointer, TouchPhase state);
	private InputPositionHandlerDelegate _inputPositionHandler;
	private delegate void ScrollHandlerDelegate(Vector2 scrollDelta);
	private ScrollHandlerDelegate _scrollHandler;

	// Input mouse/finger position in the local space of the list.
	private Vector3 _startInputPos_L;
	private Vector3 _endInputPos_L;
	private Vector3 _curFrameInputPos_L;
	private Vector3 _deltaInputPos_L;
	private int _numOfInputFrames;

	// Variables for moving listBoxes
	private int _slidingFramesLeft;
	private Vector3 _slidingDistance;     // The sliding distance for each frame
	private Vector3 _slidingDistanceLeft;
	// The flag indicating that one of the boxes need to be centered after the sliding
	private bool _needToAlignToCenter = false;

	// Variables for linear mode
	[HideInInspector]
	public int numOfUpperDisabledBoxes = 0;
	[HideInInspector]
	public int numOfLowerDisabledBoxes = 0;
	private int _maxNumOfDisabledBoxes = 0;

	/* Notice: ListBox will initialize its variables from here, so ListPositionCtrl
	 * must be executed before ListBox. You have to set the execution order in the inspector.
	 */
	void Start()
	{
		InitializePositionVars();
		InitializeInputFunction();
		InitializeBoxDependency();
		_maxNumOfDisabledBoxes = listBoxes.Length / 2;
	}

	void InitializePositionVars()
	{
		/* The the reference of canvas plane */
		_parentCanvas = GetComponentInParent<Canvas>();

		/* Get the max position of canvas plane in the canvas space.
		 * Assume that the origin of the canvas space is at the center of canvas plane. */
		RectTransform rectTransform = _parentCanvas.GetComponent<RectTransform>();
		canvasMaxPos_L = new Vector2(rectTransform.rect.width / 2, rectTransform.rect.height / 2);

		unitPos_L = canvasMaxPos_L / boxGapFactor;
		lowerBoundPos_L = unitPos_L * (-1 * listBoxes.Length / 2 - 1);
		upperBoundPos_L = unitPos_L * (listBoxes.Length / 2 + 1);

		// If there are even number of ListBoxes, narrow the boundary for 1 unitPos.
		if ((listBoxes.Length & 0x1) == 0) {
			lowerBoundPos_L += unitPos_L / 2;
			upperBoundPos_L -= unitPos_L / 2;
		}
	}

	void InitializeBoxDependency()
	{
		// Set the box ID according to the order in the container `listBoxes`
		for (int i = 0; i < listBoxes.Length; ++i)
			listBoxes[i].listBoxID = i;

		// Set the neighbor boxes
		for (int i = 0; i < listBoxes.Length; ++i) {
			listBoxes[i].lastListBox = listBoxes[(i - 1 >= 0) ? i - 1 : listBoxes.Length - 1];
			listBoxes[i].nextListBox = listBoxes[(i + 1 < listBoxes.Length) ? i + 1 : 0];
		}
	}

	/* Initialize the corresponding handlers for the selected controlling mode
	 *
	 * The unused handler will be assigned a dummy function to
	 * prevent the handling of the event.
	 */
	void InitializeInputFunction()
	{
		switch (controlMode) {
			case ControlMode.Drag:
				_inputPositionHandler = DragPositionHandler;

				_scrollHandler = delegate (Vector2 v) { };
				foreach (Button button in controlButtons)
					button.gameObject.SetActive(false);
				break;

			case ControlMode.Button:
				_inputPositionHandler =
					delegate (PointerEventData pointer, TouchPhase phase) { };
				_scrollHandler = delegate (Vector2 v) { };
				break;

			case ControlMode.MouseWheel:
				_scrollHandler = ScrollDeltaHandler;

				_inputPositionHandler =
					delegate (PointerEventData pointer, TouchPhase phase) { };
				foreach (Button button in controlButtons)
					button.gameObject.SetActive(false);
				break;
		}
	}

	/* ====== Callback functions for the unity event system ====== */
	public void OnBeginDrag(PointerEventData pointer)
	{
		_inputPositionHandler(pointer, TouchPhase.Began);
	}

	public void OnDrag(PointerEventData pointer)
	{
		_inputPositionHandler(pointer, TouchPhase.Moved);
	}

	public void OnEndDrag(PointerEventData pointer)
	{
		_inputPositionHandler(pointer, TouchPhase.Ended);
	}

	public void OnScroll(PointerEventData pointer)
	{
		_scrollHandler(pointer.scrollDelta);
	}


	/* Move the list accroding to the dragging position and the dragging state
	 */
	void DragPositionHandler(PointerEventData pointer, TouchPhase state)
	{
		switch (state) {
			case TouchPhase.Began:
				_numOfInputFrames = 0;
				_startInputPos_L = ScreenToCanvasSpace(pointer.position);
				_slidingFramesLeft = 0; // Make the list stop sliding
				break;

			case TouchPhase.Moved:
				++_numOfInputFrames;
				_deltaInputPos_L = ScreenToCanvasSpace(pointer.delta);
				// Slide the list as long as the moving distance of the pointer
				_slidingDistanceLeft = _deltaInputPos_L;
				_slidingFramesLeft = 1;
				break;

			case TouchPhase.Ended:
				_endInputPos_L = ScreenToCanvasSpace(pointer.position);
				SetSlidingEffect();
				break;
		}
	}

	/* Scroll the list accroding to the delta of the mouse scrolling
	 */
	void ScrollDeltaHandler(Vector2 mouseScrollDelta)
	{
		switch (direction) {
			case Direction.Vertical:
				if (mouseScrollDelta.y > 0)
					MoveOneUnitUp();
				else if (mouseScrollDelta.y < 0)
					MoveOneUnitDown();
				break;

			case Direction.Horizontal:
				if (mouseScrollDelta.y > 0)
					MoveOneUnitDown();
				else if (mouseScrollDelta.y < 0)
					MoveOneUnitUp();
				break;
		}
	}

	/* Transform the coordinate from the screen space to the canvas space
	 */
	Vector3 ScreenToCanvasSpace(Vector3 position)
	{
		return position / _parentCanvas.scaleFactor;
	}


	/* ====== Movement functions ====== */
	/* Control the movement of listBoxes
	 */
	void Update()
	{
		if (_slidingFramesLeft > 0) {
			if (listType == ListType.Linear) {
				StopListWhenReachEnd();
			}

			--_slidingFramesLeft;

			// Set sliding distance for this frame
			if (_slidingFramesLeft == 0) {
				if (_needToAlignToCenter) {
					_needToAlignToCenter = false;
					SetSlidingToCenter();
				} else {
					_slidingDistance = _slidingDistanceLeft;
				}
			} else
				_slidingDistance = Vector3.Lerp(Vector3.zero, _slidingDistanceLeft,
					boxSlidingSpeedFactor);

			foreach (ListBox listBox in listBoxes)
				listBox.UpdatePosition(_slidingDistance);

			_slidingDistanceLeft -= _slidingDistance;
		}
	}

	/* Calculate the sliding distance and sliding frames
	 */
	void SetSlidingEffect()
	{
		Vector3 deltaPos = _deltaInputPos_L;
		Vector3 slideDistance = _endInputPos_L - _startInputPos_L;
		bool fastSliding = IsFastSliding(_numOfInputFrames, slideDistance);

		if (fastSliding)
			deltaPos *= 5.0f;   // Slide more longer!

		_slidingDistanceLeft = deltaPos;

		if (alignMiddle) {
			_slidingFramesLeft = fastSliding ? boxSlidingFrames >> 1 : boxSlidingFrames >> 2;
			_needToAlignToCenter = true;
		} else {
			_slidingFramesLeft = fastSliding ? boxSlidingFrames * 2 : boxSlidingFrames;
		}
	}

	/* Determine if the finger or mouse sliding is the fast sliding.
	 * If the duration of a slide is within 15 frames and the distance is
	 * longer than the 1/3 of the distance of the list, the slide is the fast sliding.
	 */
	bool IsFastSliding(int frames, Vector3 distance)
	{
		if (frames < 15) {
			switch (direction) {
				case Direction.Horizontal:
					if (Mathf.Abs(distance.x) > canvasMaxPos_L.x * 2.0f / 3.0f)
						return true;
					else
						return false;
				case Direction.Vertical:
					if (Mathf.Abs(distance.y) > canvasMaxPos_L.y * 2.0f / 3.0f)
						return true;
					else
						return false;
			}
		}
		return false;
	}

	/* Set the sliding effect to make one of boxes align to center
	 */
	void SetSlidingToCenter()
	{
		_slidingDistanceLeft = FindDeltaPositionToCenter();
		_slidingFramesLeft = boxSlidingFrames;
	}

	/* Find the listBox which is the closest to the center position,
	 * and calculate the delta x or y position between it and the center position.
	 */
	Vector3 FindDeltaPositionToCenter()
	{
		float minDeltaPos = Mathf.Infinity;
		float deltaPos;
		Vector3 alignToCenterDistance;

		switch (direction) {
			case Direction.Vertical:
				foreach (ListBox listBox in listBoxes) {
					// Skip the disabled box in linear mode
					if (!listBox.isActiveAndEnabled)
						continue;

					deltaPos = -listBox.transform.localPosition.y;
					if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
						minDeltaPos = deltaPos;
				}

				alignToCenterDistance = new Vector3(0.0f, minDeltaPos, 0.0f);
				break;

			case Direction.Horizontal:
				foreach (ListBox listBox in listBoxes) {
					// Skip the disabled box in linear mode
					if (!listBox.isActiveAndEnabled)
						continue;

					deltaPos = -listBox.transform.localPosition.x;
					if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
						minDeltaPos = deltaPos;
				}

				alignToCenterDistance = new Vector3(minDeltaPos, 0.0f, 0.0f);
				break;

			default:
				alignToCenterDistance = Vector3.zero;
				break;
		}

		return alignToCenterDistance;
	}

	/* Move the list for the distance of times of unit position
	 */
	void SetUnitMove(int unit)
	{
		Vector2 deltaPos = unitPos_L * unit;

		if (_slidingFramesLeft != 0)
			deltaPos += (Vector2)_slidingDistanceLeft;

		_slidingDistanceLeft = deltaPos;
		_slidingFramesLeft = boxSlidingFrames;
	}

	/* Move all listBoxes 1 unit up.
	 */
	public void MoveOneUnitUp()
	{
		SetUnitMove(1);
	}

	/* Move all listBoxes 1 unit down.
	 */
	public void MoveOneUnitDown()
	{
		SetUnitMove(-1);
	}

	/* Make list can't go further, when the it reaches the end.
	 *
	 * This method is used for the linear mode.
	 */
	void StopListWhenReachEnd()
	{
		switch (direction) {
			case Direction.Vertical:
				// If the list reaches the head and it keeps going down, or
				// the list reaches the tail and it keeps going up,
				// make the list end be stopped at the center.
				if ((numOfUpperDisabledBoxes >= _maxNumOfDisabledBoxes && _slidingDistanceLeft.y < 0) ||
					(numOfLowerDisabledBoxes >= _maxNumOfDisabledBoxes && _slidingDistanceLeft.y > 0)) {
					Vector3 remainDistance = FindDeltaPositionToCenter();
					_slidingDistanceLeft.y = remainDistance.y;

					if (_slidingFramesLeft == 1)
						_slidingFramesLeft = boxSlidingFrames;
				}

				break;

			case Direction.Horizontal:
				// If the list reaches the head and it keeps going left, or
				// the list reaches the tail and it keeps going right,
				// make the list end be stopped at the center.
				if ((numOfUpperDisabledBoxes >= _maxNumOfDisabledBoxes && _slidingDistanceLeft.x > 0) ||
				    (numOfLowerDisabledBoxes >= _maxNumOfDisabledBoxes && _slidingDistanceLeft.x < 0)) {
					Vector3 remainDitance = FindDeltaPositionToCenter();
					_slidingDistanceLeft.x = remainDitance.x;
				}

				break;
		}
	}


	/* Get the object of the centered ListBox.
	 * The centered ListBox is found by comparing which one is the closest
	 * to the center.
	 */
	public ListBox GetCenteredBox()
	{
		float minPosition = Mathf.Infinity;
		float position;
		ListBox candicateBox = null;

		switch (direction) {
			case Direction.Vertical:
				foreach (ListBox listBox in listBoxes) {
					position = Mathf.Abs(listBox.transform.localPosition.y);
					if (position < minPosition) {
						minPosition = position;
						candicateBox = listBox;
					}
				}
				break;
			case Direction.Horizontal:
				foreach (ListBox listBox in listBoxes) {
					position = Mathf.Abs(listBox.transform.localPosition.x);
					if (position < minPosition) {
						minPosition = position;
						candicateBox = listBox;
					}
				}
				break;
		}

		return candicateBox;
	}

	/* Get the content ID of the centered box
	 */
	public int GetCenteredContentID()
	{
		return GetCenteredBox().GetContentID();
	}

	/* Divide each component of vector a by vector b.
	 */
	Vector3 DivideComponent(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
	}
}
