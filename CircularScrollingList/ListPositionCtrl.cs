/* Get inputs and assgin the delta distance to all ListBoxes.
 *
 * There are three controling modes:
 * 1. Free moving: Move ListBoxes by finger or mouse.
 *    You don't know where the ListBox will stop at.
 * 2. Align to center: It's the same as free moving.
 *    However, there is always a ListBox positioning at the center.
 * 3. Control by button: Control ListBoxes by UI buttons.
 *    There is always a ListBox positioning at the center.
 *
 * Author: LanKuDot <airlanser@gmail.com>
 */
using UnityEngine;
using UnityEngine.UI;

public class ListPositionCtrl : MonoBehaviour
{
	public enum Direction
	{
		VERTICAL,
		HORIZONTAL
	};

	public static ListPositionCtrl Instance;

	/*========== Settings ==========*/
	/* Initial settings.
	 *   Mode            controlByButton  alignToCenter
	 * --------------------------------------------------
	 *   Free moving          false           false
	 *   Align to center      false           true
	 *   Control by btn       true          Don't care
	 */
	public bool controlByButton = false;
	public bool alignToCenter = false;

	/* Containers */
	public ListBox[] listBoxes;
	public Button[] buttons;

	/* Parameters */
	public Direction direction = Direction.VERTICAL;
	// Set the distance between each ListBox. The larger, the closer.
	public float divideFactor = 2.0f;
	// Set the sliding duration. The larger, the longer.
	public int slidingFrames = 35;
	// Set the sliding speed. The larger, the quicker.
	[Range(0.0f, 1.0f)]
	public float slidingFactor = 0.2f;
	// Set the scrolling list curving to left/right, or up/down in HORIZONTAL mode.
	// Positive: Curve to right (up); Negative: Curve to left (down).
	[Range(-1.0f, 1.0f)]
	public float angularity = 0.3f;
	// Set the scale amount of the center listBox.
	public float scaleFactor = 0.32f;
	/*===============================*/

	private bool _isTouchingDevice;

	// The canvas plane which the scrolling list is at.
	private Canvas _parentCanvas;

	// The constrains of position in the local space of the canvas plane.
	private Vector2 _canvasMaxPos_L;
	private Vector2 _unitPos_L;
	private Vector2 _lowerBoundPos_L;
	private Vector2 _upperBoundPos_L;
	private Vector2 _shiftBoundPos_L;
	// The gets of the above variables
	public Vector2 canvasMaxPos_L { get { return _canvasMaxPos_L; } }
	public Vector2 unitPos_L { get { return _unitPos_L; } }
	public Vector2 lowerBoundPos_L { get { return _lowerBoundPos_L; } }
	public Vector2 upperBoundPos_L { get { return _upperBoundPos_L; } }
	public Vector2 shiftBoundPos_L { get { return _shiftBoundPos_L; } }

	// Input mouse/finger position in the local space of the list.
	private delegate void StoreInputPosition();
	private StoreInputPosition _storeInputPosition;
	private Vector3 _startInputPos_L;
	private Vector3 _lastInputPos_L;
	private Vector3 _currentInputPos_L;
	private Vector3 _deltaInputPos_L;
	private int _numofSlideFrames;

	// Store the calculation result of the sliding distance for aligning to the center.
	// If its value is NaN, the distance haven't been calcuated yet.
	private Vector3 _alignToCenterDistance;

	void Awake()
	{
		Instance = this;

		switch (Application.platform) {
		case RuntimePlatform.WindowsEditor:
			_isTouchingDevice = false;
			break;
		case RuntimePlatform.Android:
			_isTouchingDevice = true;
			break;
		}
	}

	/* Notice: ListBox will initialize its variables from here, so ListPositionCtrl
	 * must be executed before ListBox. You have to set the execution order in the inspector.
	 */
	void Start()
	{
		/* The the reference of canvas plane */
		_parentCanvas = GetComponentInParent<Canvas>();

		/* Get the max position of canvas plane in the canvas space.
		 * Assume that the origin of the canvas space is at the center of canvas plane. */
		RectTransform rectTransform = _parentCanvas.GetComponent<RectTransform>();
		_canvasMaxPos_L = new Vector2(rectTransform.rect.width / 2, rectTransform.rect.height / 2);

		_unitPos_L = _canvasMaxPos_L / divideFactor;
		_lowerBoundPos_L = _unitPos_L * (-1 * listBoxes.Length / 2 - 1);
		_upperBoundPos_L = _unitPos_L * (listBoxes.Length / 2 + 1);
		_shiftBoundPos_L = _unitPos_L * 0.3f;

		// If there are even number of ListBoxes, narrow the boundary for 1 unitPos.
		if ((listBoxes.Length & 0x1) == 0) {
			_lowerBoundPos_L += _unitPos_L / 2;
			_upperBoundPos_L -= _unitPos_L / 2;
		}

		/* Initialize the delegate function. */
		if (!controlByButton) {
			foreach (Button button in buttons)
				button.gameObject.SetActive(false);

			if (_isTouchingDevice)
				_storeInputPosition = storeFingerPosition;
			else
				_storeInputPosition = storeMousePosition;
		} else {
			_storeInputPosition = delegate() { };	// Empty delegate function
		}
	}

	void Update()
	{
		_storeInputPosition();
	}

	/* Store the position of mouse when the player clicks the left mouse button.
	 */
	void storeMousePosition()
	{
		if (Input.GetMouseButtonDown(0)) {
			_lastInputPos_L = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			_lastInputPos_L /= _parentCanvas.scaleFactor;
			_startInputPos_L = _lastInputPos_L;
			_numofSlideFrames = 0;
			// When the user starts to drag the list, all listBoxes stop free sliding.
			foreach (ListBox listBox in listBoxes)
				listBox.keepSliding = false;
		} else if (Input.GetMouseButton(0)) {
			_currentInputPos_L = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			_currentInputPos_L /= _parentCanvas.scaleFactor;
			_deltaInputPos_L = _currentInputPos_L - _lastInputPos_L;
			foreach (ListBox listbox in listBoxes)
				listbox.updatePosition(_deltaInputPos_L);

			_lastInputPos_L = _currentInputPos_L;
			++_numofSlideFrames;
		} else if (Input.GetMouseButtonUp(0))
			setSlidingEffect();
	}

	/* Store the position of touching on the mobile.
	 */
	void storeFingerPosition()
	{
		if (Input.GetTouch(0).phase == TouchPhase.Began) {
			_lastInputPos_L = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
			_lastInputPos_L /= _parentCanvas.scaleFactor;
			_startInputPos_L = _lastInputPos_L;
			_numofSlideFrames = 0;
			// When the user starts to drag the list, all listBoxes stop free sliding.
			foreach (ListBox listBox in listBoxes)
				listBox.keepSliding = false;
		} else if (Input.GetTouch(0).phase == TouchPhase.Moved) {
			_currentInputPos_L = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
			_currentInputPos_L /= _parentCanvas.scaleFactor;
			_deltaInputPos_L = _currentInputPos_L - _lastInputPos_L;
			foreach (ListBox listbox in listBoxes)
				listbox.updatePosition(_deltaInputPos_L);

			_lastInputPos_L = _currentInputPos_L;
			++_numofSlideFrames;
		} else if (Input.GetTouch(0).phase == TouchPhase.Ended)
			setSlidingEffect();
	}

	/* If the touching is ended, calculate the distance to slide and
	 * assign to the listBoxes.
	 */
	void setSlidingEffect()
	{
		Vector3 deltaPos = _deltaInputPos_L;
		Vector3 slideDistance = _lastInputPos_L - _startInputPos_L;
		bool fastSliding = isFastSliding(_numofSlideFrames, slideDistance);

		if (fastSliding)
			deltaPos *= 5.0f;   // Slide more longer!

		if (alignToCenter) {
			foreach (ListBox listbox in listBoxes) {
				listbox.setSlidingDistance(deltaPos, fastSliding ? slidingFrames >> 1 : slidingFrames >> 2);
				listbox.needToAlignToCenter = true;
			}
			// Make the distance uncalculated.
			_alignToCenterDistance = new Vector3(float.NaN, float.NaN, 0.0f);
		} else {
			foreach (ListBox listbox in listBoxes)
				listbox.setSlidingDistance(deltaPos, fastSliding ? slidingFrames * 2 : slidingFrames);
		}
	}

	/* Judge if this cursor or finger slide is the fast sliding.
	 * If the duration of a slide is within 15 frames and the distance is
	 * longer than the 1/3 of the distance of the list, the slide is the fast sliding.
	 */
	bool isFastSliding(int frames, Vector3 distance)
	{
		if (frames < 15) {
			switch (direction) {
			case Direction.HORIZONTAL:
				if (Mathf.Abs(distance.x) > _canvasMaxPos_L.x * 2.0f / 3.0f)
					return true;
				else
					return false;
			case Direction.VERTICAL:
				if (Mathf.Abs(distance.y) > _canvasMaxPos_L.y * 2.0f / 3.0f)
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
	public Vector3 findDeltaPositionToCenter()
	{
		float minDeltaPos = Mathf.Infinity;
		float deltaPos;

		// If the distance for aligning to the center was calculated,
		// return the result immediately.
		if (!float.IsNaN(_alignToCenterDistance.x) &&
			!float.IsNaN(_alignToCenterDistance.y))
			return _alignToCenterDistance;

		switch (direction) {
		case Direction.VERTICAL:
			foreach (ListBox listBox in listBoxes) {
				deltaPos = -listBox.transform.localPosition.y;
				if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
					minDeltaPos = deltaPos;
			}

			_alignToCenterDistance = new Vector3(0.0f, minDeltaPos, 0.0f);
			break;

		case Direction.HORIZONTAL:
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

	/* Divide each component of vector a by vector b.
	 */
	Vector3 divideComponent(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
	}

	/* controlByButton is enabled!
	 * When the next content button is pressed,
	 * move all listBoxes 1 unit up.
	 */
	public void nextContent()
	{
		foreach (ListBox listbox in listBoxes)
			listbox.unitMove(1, true);
	}

	/* controlByButton is enabled!
	 * When the last content button is pressed,
	 * move all listBoxes 1 unit down.
	 */
	public void lastContent()
	{
		foreach (ListBox listbox in listBoxes)
			listbox.unitMove(1, false);
	}
}
