/* Calculate the final position of ListBoxes.
 */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ListPositionCtrl : MonoBehaviour
{
	public static ListPositionCtrl Instance;
	public bool controlByButton = false;
	public bool alignToCenter = false;

	public ListBox[] listBoxes;
	public float centerPosY;

	public Button[] buttons;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		if ( !controlByButton )
			foreach ( Button button in buttons )
				button.gameObject.SetActive( false );
	}

	void Update()
	{
		if ( alignToCenter && !controlByButton )
			if ( Input.GetMouseButtonUp(0) ||
		   	 ( Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended ) )
				assignDeltaPositionY();
	}

	/* Find the listBox which is the closest to the center y position,
	 * And calculate the delta y position between them.
	 * Then assign the delta y position to all listBoxes.
	 */
	void assignDeltaPositionY()
	{
		float deltaPosY = findDeltaPositionY();

		for ( int i = 0; i < listBoxes.Length; ++i )
			listBoxes[i].setDeltaPosY( deltaPosY );
	}

	float findDeltaPositionY()
	{
		float minDeltaPosY = 99999.9f;
		float deltaPosY;

		foreach ( ListBox listBox in listBoxes )
		{
			deltaPosY = centerPosY - listBox.transform.position.y;

			if ( Mathf.Abs( deltaPosY ) < Mathf.Abs( minDeltaPosY ) )
				minDeltaPosY = deltaPosY;
		}

		return minDeltaPosY;
	}

	/* controlByButton is enabled!
	 * When the next content button is pressed,
	 * move all listBoxes 1 unit up.
	 */
	public void nextContent()
	{
		foreach( ListBox listbox in listBoxes )
			listbox.unitMove( 1, true );
	}

	/* controlByButton is enabled!
	 * When the last content button is pressed,
	 * move all listBoxes 1 unit down.
	 */
	public void lastContent()
	{
		foreach( ListBox listbox in listBoxes )
			listbox.unitMove( 1, false );
	}
}
