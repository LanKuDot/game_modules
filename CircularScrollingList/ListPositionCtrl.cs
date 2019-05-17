/* Get user inputs and control the movement of listBoxes it has.
 *
 * Author: LanKuDot <airlanser@gmail.com>
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public interface IControlEventHandler:
	IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{}

public class ListPositionCtrl : MonoBehaviour, IControlEventHandler
{
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
	/* Control mode */
	public ControlMode controlMode = ControlMode.Drag;
	public bool alignToCenter = false;
	public Direction direction = Direction.Vertical;

	/* Containers */
	public ListBox[] listBoxes;
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
	public Vector2 shiftBoundPos_L { get; private set; }

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

	// Store the calculation result of the sliding distance for aligning to the center.
	// If its value is NaN, the distance haven't been calcuated yet.
	private Vector3 _alignToCenterDistance;

	/* Notice: ListBox will initialize its variables from here, so ListPositionCtrl
	 * must be executed before ListBox. You have to set the execution order in the inspector.
	 */
	void Start()
	{
		InitializePositionVars();
		InitializeInputFunction();
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
		shiftBoundPos_L = unitPos_L * 0.3f;

		// If there are even number of ListBoxes, narrow the boundary for 1 unitPos.
		if ((listBoxes.Length & 0x1) == 0) {
			lowerBoundPos_L += unitPos_L / 2;
			upperBoundPos_L -= unitPos_L / 2;
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
		switch (state)
		{
			case TouchPhase.Began:
				_numOfInputFrames = 0;
				_startInputPos_L = ScreenToCanvasSpace(pointer.position);
				foreach (ListBox listBox in listBoxes)
					listBox.keepSliding = false;
				break;

			case TouchPhase.Moved:
				++_numOfInputFrames;
				_deltaInputPos_L = ScreenToCanvasSpace(pointer.delta);
				foreach (ListBox listBox in listBoxes)
					listBox.UpdatePosition(_deltaInputPos_L);
				break;

			case TouchPhase.Ended:
				_endInputPos_L = ScreenToCanvasSpace(pointer.position);
				SetSlidingEffect();
				break;
		}
	}

	/* Transform the coordinate from the screen space to the canvas space
	 */
	Vector3 ScreenToCanvasSpace(Vector3 position)
	{
		return position / _parentCanvas.scaleFactor;
	}

	/* Scroll the list accroding to the scrollDelta of the mouse.
	 */
	void ScrollDeltaHandler(Vector2 mouseScrollDelta)
	{
		if (mouseScrollDelta.y > 0)
			NextContent();
		else if (mouseScrollDelta.y < 0)
			LastContent();
	}

	/* Calculate the sliding distance and assign it to the listBoxes
	 */
	void SetSlidingEffect()
	{
		Vector3 deltaPos = _deltaInputPos_L;
		Vector3 slideDistance = _endInputPos_L - _startInputPos_L;
		bool fastSliding = IsFastSliding(_numOfInputFrames, slideDistance);

		if (fastSliding)
			deltaPos *= 5.0f;   // Slide more longer!

		if (alignToCenter) {
			foreach (ListBox listbox in listBoxes) {
				listbox.SetSlidingDistance(deltaPos, fastSliding ? boxSlidingFrames >> 1 : boxSlidingFrames >> 2);
				listbox.needToAlignToCenter = true;
			}
			// Make the distance uncalculated.
			_alignToCenterDistance = new Vector3(float.NaN, float.NaN, 0.0f);
		} else {
			foreach (ListBox listbox in listBoxes)
				listbox.SetSlidingDistance(deltaPos, fastSliding ? boxSlidingFrames * 2 : boxSlidingFrames);
		}
	}

	/* Judge if this cursor or finger slide is the fast sliding.
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

	/* Find the listBox which is the closest to the center position,
	 * And calculate the delta position of x or y between them.
	 */
	public Vector3 FindDeltaPositionToCenter()
	{
		float minDeltaPos = Mathf.Infinity;
		float deltaPos;

		// If the distance for aligning to the center was calculated,
		// return the result immediately.
		if (!float.IsNaN(_alignToCenterDistance.x) &&
			!float.IsNaN(_alignToCenterDistance.y))
			return _alignToCenterDistance;

		switch (direction) {
		case Direction.Vertical:
			foreach (ListBox listBox in listBoxes) {
				deltaPos = -listBox.transform.localPosition.y;
				if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
					minDeltaPos = deltaPos;
			}

			_alignToCenterDistance = new Vector3(0.0f, minDeltaPos, 0.0f);
			break;

		case Direction.Horizontal:
			foreach (ListBox listBox in listBoxes) {
				deltaPos = -listBox.transform.localPosition.x;
				if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
					minDeltaPos = deltaPos;
			}

			_alignToCenterDistance = new Vector3(minDeltaPos, 0.0f, 0.0f);
			break;

		default:
			_alignToCenterDistance = Vector3.zero;
			break;
		}

		return _alignToCenterDistance;
	}

	/*
	 * Get the object of the centered ListBox.
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

	/* Divide each component of vector a by vector b.
	 */
	Vector3 DivideComponent(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
	}

	/* controlByButton is enabled!
	 * When the next content button is pressed,
	 * move all listBoxes 1 unit up.
	 */
	public void NextContent()
	{
		foreach (ListBox listbox in listBoxes)
			listbox.UnitMove(1, true);
	}

	/* controlByButton is enabled!
	 * When the last content button is pressed,
	 * move all listBoxes 1 unit down.
	 */
	public void LastContent()
	{
		foreach (ListBox listbox in listBoxes)
			listbox.UnitMove(1, false);
	}
}
