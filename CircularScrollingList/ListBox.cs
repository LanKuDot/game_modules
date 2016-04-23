/* The basic component of scrolling list.
 * Control the position of the list element.
 *
 * Author: LanKuDot <airlanser@gmail.com>
 *
 * As long as you retain this notice you can do whatever you want with this stuff.
 * If we meet some day, and you think this stuff is worth it,
 * you can buy me a coffee in return. LanKuDot
 */
using UnityEngine;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
	public int listBoxID;	// Must be unique, and count from 0
	public Text content;		// The content of the list box

	public ListBox lastListBox;
	public ListBox nextListBox;

	private int numOfListBox;
	private int contentID;

	// The max coordinate of canvas plane in local space.
	private Vector2 canvasMaxPos_L;
	private Vector2 unitPos_L;
	private Vector2 lowerBoundPos_L;
	private Vector2 upperBoundPos_L;
	private Vector2 rangeBoundPos_L;
	private Vector2 shiftBoundPos_L;

	private Vector3 slidingDistance_L;	// The sliding distance at each frame
	private Vector3 slidingDistanceLeft_L;

	private Vector3 originalLocalScale;

	private bool keepSliding = false;
	private int slidingFrames;

	void Start()
	{
		numOfListBox = ListPositionCtrl.Instance.listBoxes.Length;

		/* The minimum position is at left-bottom corner of camera which coordinate is (0,0),
		 * and the maximum position is at right-top corner of camera. For perspective view,
		 * we have to take the distance between canvas plane and camera into account. */
		canvasMaxPos_L = Camera.main.ScreenToWorldPoint(
			new Vector3( Camera.main.pixelWidth, Camera.main.pixelHeight, ListPositionCtrl.Instance.canvasDistance ) ) -
			Camera.main.ScreenToWorldPoint( new Vector3( 0.0f, 0.0f, ListPositionCtrl.Instance.canvasDistance ) );
		/* Assume that the origin of canvas plane is at the center of canvas plane.
		 * Finally, divide the result with the localScale of canvas plane to get the
		 * correct local position. */
		canvasMaxPos_L /= ( 2.0f * ListPositionCtrl.Instance.transform.parent.localScale.x );

		unitPos_L = canvasMaxPos_L / ListPositionCtrl.Instance.divideFactor;

		lowerBoundPos_L = unitPos_L * (float)( -1 * numOfListBox / 2 - 1 );
		upperBoundPos_L = unitPos_L * (float)( numOfListBox / 2 + 1 );
		rangeBoundPos_L = unitPos_L * (float)numOfListBox;
		shiftBoundPos_L = unitPos_L * 0.3f;

		originalLocalScale = transform.localScale;

		initialPosition( listBoxID );
		initialContent();
	}

	/* Initialize the content of ListBox.
	 */
	void initialContent()
	{
		if ( listBoxID == numOfListBox / 2 )
			contentID = 0;
		else if ( listBoxID < numOfListBox / 2 )
			contentID = ListBank.Instance.getListLength() - ( numOfListBox / 2 - listBoxID );
		else
			contentID = listBoxID - numOfListBox / 2;

		while ( contentID < 0 )
			contentID += ListBank.Instance.getListLength();
		contentID = contentID % ListBank.Instance.getListLength();

		updateContent( ListBank.Instance.getListContent( contentID ) );
	}

	void updateContent( string content )
	{
		this.content.text = content;
	}

	/* Make the list box slide for delta x or y position.
	 */
	public void setSlidingDistance( Vector3 distance )
	{
		keepSliding = true;
		slidingFrames = ListPositionCtrl.Instance.slidingFrames;

		slidingDistanceLeft_L = distance;
		slidingDistance_L = Vector3.Lerp( Vector3.zero, distance, ListPositionCtrl.Instance.slidingFactor );
	}

	/* Move the listBox for world position unit.
	 * Move up when "up" is true, or else, move down.
	 */
	public void unitMove( int unit, bool up_right )
	{
		Vector2 deltaPos;

		if ( up_right )
			deltaPos = unitPos_L * (float)unit;
		else
			deltaPos = unitPos_L * (float)unit * -1;

		switch ( ListPositionCtrl.Instance.direction ) {
		case ListPositionCtrl.Direction.VERTICAL:
			setSlidingDistance( new Vector3( 0.0f, deltaPos.y, 0.0f ) );
			break;
		case ListPositionCtrl.Direction.HORIZONTAL:
			setSlidingDistance( new Vector3( deltaPos.x, 0.0f, 0.0f ) );
			break;
		}
	}

	void Update()
	{
		if ( keepSliding )
		{
			--slidingFrames;
			if ( slidingFrames == 0 )
			{
				keepSliding = false;
				// At the last sliding frame, move to that position.
				// At free moving mode, this function is disabled.
				if ( ListPositionCtrl.Instance.alignToCenter ||
				    ListPositionCtrl.Instance.controlByButton )
					updatePosition( slidingDistanceLeft_L );
				return;
			}

			updatePosition( slidingDistance_L );
			slidingDistanceLeft_L -= slidingDistance_L;
			slidingDistance_L = Vector3.Lerp( Vector3.zero, slidingDistanceLeft_L, ListPositionCtrl.Instance.slidingFactor );
		}
	}

	/* Initialize the local position of the list box accroding to its ID.
	 */
	void initialPosition( int listBoxID )
	{
		switch( ListPositionCtrl.Instance.direction ) {
		case ListPositionCtrl.Direction.VERTICAL:
			transform.localPosition = new Vector3( 0.0f,
		    	                             unitPos_L.y * (float)( listBoxID * -1 + numOfListBox / 2 ),
		    	                             0.0f );
			updateXPosition();
			break;
		case ListPositionCtrl.Direction.HORIZONTAL:
			transform.localPosition = new Vector3( unitPos_L.x * (float)( listBoxID - numOfListBox / 2 ),
			                                 0.0f, 0.0f );
			updateYPosition();
			break;
		}
	}

	/* Update the local position of ListBox accroding to the delta position at each frame.
	 * Note that the deltaPosition must be in local space.
	 */
	public void updatePosition( Vector3 deltaPosition_L )
	{
		switch ( ListPositionCtrl.Instance.direction ) {
		case ListPositionCtrl.Direction.VERTICAL:
			transform.localPosition += new Vector3( 0.0f, deltaPosition_L.y, 0.0f );
			updateXPosition();
			checkBoundaryY();
			break;
		case ListPositionCtrl.Direction.HORIZONTAL:
			transform.localPosition += new Vector3( deltaPosition_L.x, 0.0f, 0.0f );
			updateYPosition();
			checkBoundaryX();
			break;
		}
	}

	/* Calculate the x position accroding to the y position.
	 * Formula: x = max_x * angularity * cos( radian controlled by y )
	 * radian = (y / upper_y) * pi / 2, so the range of radian is from pi/2 to 0 to -pi/2,
	 * and corresponding cosine value is from 0 to 1 to 0.
	 */
	void updateXPosition()
	{
		transform.localPosition = new Vector3(
			canvasMaxPos_L.x * ListPositionCtrl.Instance.angularity * Mathf.Cos( transform.localPosition.y / upperBoundPos_L.y * Mathf.PI / 2.0f ),
			transform.localPosition.y,
			transform.localPosition.z );
		updateSize( upperBoundPos_L.y, transform.localPosition.y );
	}

	/* Calculate the y position accroding to the x position.
	 */
	void updateYPosition()
	{
		transform.localPosition = new Vector3(
			transform.localPosition.x,
			canvasMaxPos_L.y * ListPositionCtrl.Instance.angularity * Mathf.Cos( transform.localPosition.x / upperBoundPos_L.x * Mathf.PI / 2.0f ),
			transform.localPosition.z );
		updateSize( upperBoundPos_L.x, transform.localPosition.x );
	}

	/* Check if the ListBox is beyond the upper or lower bound or not.
	 * If does, move the ListBox to the other side and update the content.
	 */
	void checkBoundaryY()
	{
		float beyondPosY_L = 0.0f;

		// Narrow the checking boundary in order to avoid the list swaying to one side
		if ( transform.localPosition.y < lowerBoundPos_L.y + shiftBoundPos_L.y )
		{
			beyondPosY_L = ( lowerBoundPos_L.y + shiftBoundPos_L.y - transform.localPosition.y ) % rangeBoundPos_L.y;
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				upperBoundPos_L.y + shiftBoundPos_L.y - unitPos_L.y - beyondPosY_L,
				transform.localPosition.z );
			updateToLastContent();
		}
		else if ( transform.localPosition.y > upperBoundPos_L.y - shiftBoundPos_L.y )
		{
			beyondPosY_L = ( transform.localPosition.y - upperBoundPos_L.y + shiftBoundPos_L.y ) % rangeBoundPos_L.y;
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				lowerBoundPos_L.y - shiftBoundPos_L.y + unitPos_L.y + beyondPosY_L,
				transform.localPosition.z );
			updateToNextContent();
		}

		updateXPosition();
	}

	void checkBoundaryX()
	{
		float beyondPosX_L = 0.0f;

		// Narrow the checking boundary in order to avoid the list swaying to one side
		if ( transform.localPosition.x < lowerBoundPos_L.x + shiftBoundPos_L.x )
		{
			beyondPosX_L = ( lowerBoundPos_L.x + shiftBoundPos_L.x - transform.localPosition.x ) % rangeBoundPos_L.x;
			transform.localPosition = new Vector3(
				upperBoundPos_L.x + shiftBoundPos_L.x - unitPos_L.x - beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z );
			updateToNextContent();
		}
		else if ( transform.localPosition.x > upperBoundPos_L.x - shiftBoundPos_L.x )
		{
			beyondPosX_L = ( transform.localPosition.x - upperBoundPos_L.x + shiftBoundPos_L.x ) % rangeBoundPos_L.x;
			transform.localPosition = new Vector3(
				lowerBoundPos_L.x - shiftBoundPos_L.x + unitPos_L.x + beyondPosX_L,
				transform.localPosition.y,
				transform.localPosition.z );
			updateToLastContent();
		}

		updateYPosition();
	}

	/* Scale the size of listBox accroding to the position.
	 */
	void updateSize( float smallest_at, float target_value )
	{
		transform.localScale = originalLocalScale *
			( 1.0f + ListPositionCtrl.Instance.scaleFactor * Mathf.InverseLerp( smallest_at, 0.0f, Mathf.Abs( target_value )));
	}

	public int getCurrentContentID()
	{
		return contentID;
	}

	/* Update to the last content of the next ListBox
	 * when the ListBox appears at the top of camera.
	 */
	void updateToLastContent()
	{
		contentID = nextListBox.getCurrentContentID() - 1;
		contentID = ( contentID < 0 ) ? ListBank.Instance.getListLength() - 1 : contentID;

		updateContent( ListBank.Instance.getListContent( contentID ) );
	}

	/* Update to the next content of the last ListBox
	 * when the ListBox appears at the bottom of camera.
	 */
	void updateToNextContent()
	{
		contentID = lastListBox.getCurrentContentID() + 1;
		contentID = ( contentID == ListBank.Instance.getListLength() ) ? 0 : contentID;

		updateContent( ListBank.Instance.getListContent( contentID ) );
	}
}
