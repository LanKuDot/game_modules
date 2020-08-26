/* Handle the controlling event and send the moving information to the boxes it has
 */

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IControlEventHandler:
	IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{}

/* The callback for passing the onClick event sent from the clicked ListBox.
 * The int parameter will be the ID of the content which the clicked ListBox holds.
 */
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
	[Tooltip("The type of the list.")]
	public ListType listType = ListType.Circular;
	[Tooltip("The controlling mode of the list.")]
	public ControlMode controlMode = ControlMode.Drag;
	[Tooltip("Should a box align in the middle of the list after sliding?")]
	public bool alignMiddle = false;
	[Tooltip("The major moving direction of the list.")]
	public Direction direction = Direction.Vertical;

	/* Containers */
	[Tooltip("The game object which holds the content bank for the list. " +
		"It will be the derived class of the BaseListBank.")]
	public BaseListBank listBank;
	[Tooltip("Specify the initial content ID for the centered box.")]
	public int centeredContentID = 0;
	[Tooltip("The boxes which belong to this list.")]
	public ListBox[] listBoxes;
	[Tooltip("The callbacks for the event of the clicking on boxes." +
		"The registered callbacks will be added to the 'onClick' event of boxes, " +
		"therefore, boxes should be 'Button's.")]
	public ListBoxClickEvent onBoxClick;
	[Tooltip("The Buttons used by the Button mode.")]
	public Button[] controlButtons;

	/* Parameters */
	[Tooltip("The distance between each box. The larger, the closer.")]
	public float boxDensity = 2.0f;
	[Tooltip("The curve of the box position. " +
		"The valid range of the x axis is [0, 1]. " +
		"The y axis specifies the shape of the list. " +
		"Point (0.5, 0) is the center of the list layout.")]
	public AnimationCurve boxPositionCurve = AnimationCurve.Constant(0.0f, 1.0f, 0.0f);
	[Tooltip("The curve of the box scale. " +
		"The valid range of the x axis is [0, 1]. " +
		"The y axis specifies the 'localScale' of the box.")]
	public AnimationCurve boxScaleCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
	public AnimationCurve boxMovementCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
	public AnimationCurve boxAligningCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.25f, 1.0f);
	public AnimationCurve boxBouncingCurve = new AnimationCurve(
		new Keyframe(0.0f, 0.0f),
		new Keyframe(0.05f, 1.0f),
		new Keyframe(0.1f, 0.0f));
	/*===============================*/

	// The canvas plane which the scrolling list is at.
	private Canvas _parentCanvas;

	// The constrains of position in the local space of the canvas plane.
	private float _canvasMaxPos;
	public float unitPos { get; private set; }
	public float lowerBoundPos { get; private set; }
	public float upperBoundPos { get; private set; }

	// Delegate functions
	private Action<PointerEventData, TouchPhase> _inputPositionHandler;
	private Action<Vector2> _scrollHandler;

	// Variables for moving listBoxes
	private IMovementCtrl _movementCtrl;
	// Input mouse/finger position in the local space of the list.
	private float _deltaInputPos;
	private float _deltaDistanceToCenter = 0.0f;

	// Variables for linear mode
	private float _movingDirection;
	private bool _isListReachingEnd = false;
	[HideInInspector]
	public int numOfUpperDisabledBoxes = 0;
	[HideInInspector]
	public int numOfLowerDisabledBoxes = 0;
	private int _maxNumOfDisabledBoxes = 0;

	/* Notice: ListBox will initialize its variables from here, so ListPositionCtrl
	 * must be executed before ListBox. You have to set the execution order in the inspector.
	 */
	private void Start()
	{
		InitializePositionVars();
		InitializeInputFunction();
		InitializeBoxDependency();
		_maxNumOfDisabledBoxes = listBoxes.Length / 2;
		foreach (ListBox listBox in listBoxes)
			listBox.Initialize(this);
	}

	private void InitializePositionVars()
	{
		/* The the reference of canvas plane */
		_parentCanvas = GetComponentInParent<Canvas>();

		/* Get the max position of canvas plane in the canvas space.
		 * Assume that the origin of the canvas space is at the center of canvas plane. */
		RectTransform rectTransform = _parentCanvas.GetComponent<RectTransform>();

		switch (direction) {
			case Direction.Vertical:
				_canvasMaxPos = rectTransform.rect.height / 2;
				break;
			case Direction.Horizontal:
				_canvasMaxPos = rectTransform.rect.width / 2;
				break;
		}

		unitPos = _canvasMaxPos / boxDensity;
		lowerBoundPos = unitPos * (-1 * listBoxes.Length / 2 - 1);
		upperBoundPos = unitPos * (listBoxes.Length / 2 + 1);

		// If there are even number of ListBoxes, narrow the boundary for 1 unitPos.
		if ((listBoxes.Length & 0x1) == 0) {
			lowerBoundPos += unitPos / 2;
			upperBoundPos -= unitPos / 2;
		}
	}

	private void InitializeBoxDependency()
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
	private void InitializeInputFunction()
	{
		Func<float> getAligningDistance = () => _deltaDistanceToCenter;
		Func<bool> isListReachingEnd = () => _isListReachingEnd;

		switch (controlMode) {
			case ControlMode.Drag:
				_movementCtrl = new FreeMovementCtrl(
					boxMovementCurve, boxAligningCurve, alignMiddle,
					getAligningDistance, isListReachingEnd);
				_inputPositionHandler = DragPositionHandler;
				_scrollHandler = (Vector2 v) => { };

				foreach (Button button in controlButtons)
					button.gameObject.SetActive(false);
				break;

			case ControlMode.Button:
				_movementCtrl = new UnitMovementCtrl(
					boxMovementCurve, boxBouncingCurve, unitPos * 0.3f,
					getAligningDistance, isListReachingEnd);
				_inputPositionHandler =
					(PointerEventData pointer, TouchPhase phase) => { };
				_scrollHandler = (Vector2 v) => { };
				break;

			case ControlMode.MouseWheel:
				_movementCtrl = new UnitMovementCtrl(
					boxMovementCurve, boxBouncingCurve, unitPos * 0.3f,
					getAligningDistance, isListReachingEnd);
				_inputPositionHandler =
					(PointerEventData pointer, TouchPhase phase) => { };
				_scrollHandler = ScrollDeltaHandler;

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


	/* Move the list according to the dragging position and the dragging state
	 */
	private void DragPositionHandler(PointerEventData pointer, TouchPhase state)
	{
		switch (state) {
			case TouchPhase.Began:
				break;

			case TouchPhase.Moved:
				_deltaInputPos = GetInputCanvasPosition(pointer.delta);
				// Slide the list as long as the moving distance of the pointer
				_movingDirection = Mathf.Sign(_deltaInputPos);
				_movementCtrl.SetMovement(_deltaInputPos, true);
				break;

			case TouchPhase.Ended:
				_movementCtrl.SetMovement(_deltaInputPos / Time.deltaTime, false);
				break;
		}
	}

	/* Scroll the list according to the delta of the mouse scrolling
	 */
	private void ScrollDeltaHandler(Vector2 mouseScrollDelta)
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

	/* Get the input position in the canvas space and
	 * return the value of the corresponding axis according to the moving direction.
	 */
	private float GetInputCanvasPosition(Vector3 pointerPosition)
	{
		switch (direction) {
			case Direction.Vertical:
				return pointerPosition.y / _parentCanvas.scaleFactor;
			case Direction.Horizontal:
				return pointerPosition.x / _parentCanvas.scaleFactor;
			default:
				return 0.0f;
		}
	}


	/* ====== Movement functions ====== */
	/* Control the movement of listBoxes
	 */
	private void Update()
	{
		if (!_movementCtrl.IsMovementEnded()) {
			var distance = _movementCtrl.GetDistance(Time.deltaTime);
			foreach (ListBox listBox in listBoxes)
				listBox.UpdatePosition(distance);
		}
	}

	/* Check the status of the list
	 */
	private void LateUpdate()
	{
		FindDeltaDistanceToCenter();
		if (listType == ListType.Linear)
			CheckIfListReachEnd();
	}

	/* Find the listBox which is the closest to the center position,
	 * and calculate the delta x or y position between it and the center position.
	 */
	private void FindDeltaDistanceToCenter()
	{
		float minDeltaPos = Mathf.Infinity;
		float deltaPos = 0.0f;

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
				break;
		}

		_deltaDistanceToCenter = minDeltaPos;
	}

	/* Move the list for the distance of times of unit position
	 */
	private void SetUnitMove(int unit)
	{
		_movingDirection = Mathf.Sign(unit);
		// Additional check for the moving direction changing
		CheckIfListReachEnd();

		_movementCtrl.SetMovement(unit * unitPos, false);
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
	private void CheckIfListReachEnd()
	{
		_isListReachingEnd = false;

		switch (direction) {
			case Direction.Vertical:
				// If the list reaches the head and it keeps going down, or
				// the list reaches the tail and it keeps going up,
				// make the list end be stopped at the center.
				if ((numOfUpperDisabledBoxes >= _maxNumOfDisabledBoxes &&
				     _deltaDistanceToCenter > -1e-6 && _movingDirection < 0) ||
					(numOfLowerDisabledBoxes >= _maxNumOfDisabledBoxes &&
					 _deltaDistanceToCenter < 1e-6 && _movingDirection > 0)) {
					_isListReachingEnd = true;
				}

				break;

			case Direction.Horizontal:
				// If the list reaches the head and it keeps going left, or
				// the list reaches the tail and it keeps going right,
				// make the list end be stopped at the center.
				if ((numOfUpperDisabledBoxes >= _maxNumOfDisabledBoxes &&
				     _deltaDistanceToCenter > -1e-6 && _movingDirection < 0) ||
				    (numOfLowerDisabledBoxes >= _maxNumOfDisabledBoxes &&
				     _deltaDistanceToCenter < 1e-6 && _movingDirection > 0)) {
					_isListReachingEnd = true;
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
		ListBox candidateBox = null;

		switch (direction) {
			case Direction.Vertical:
				foreach (ListBox listBox in listBoxes) {
					position = Mathf.Abs(listBox.transform.localPosition.y);
					if (position < minPosition) {
						minPosition = position;
						candidateBox = listBox;
					}
				}
				break;
			case Direction.Horizontal:
				foreach (ListBox listBox in listBoxes) {
					position = Mathf.Abs(listBox.transform.localPosition.x);
					if (position < minPosition) {
						minPosition = position;
						candidateBox = listBox;
					}
				}
				break;
		}

		return candidateBox;
	}

	/* Get the content ID of the centered box
	 */
	public int GetCenteredContentID()
	{
		return GetCenteredBox().GetContentID();
	}
}
