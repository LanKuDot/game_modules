/* Calculate the final position of ListBoxes.
 */
using UnityEngine;
using System.Collections;

public class ListPositionCtrl : MonoBehaviour
{
	public static ListPositionCtrl Instance;
	public bool alignToCenter = false;

	public ListBox[] listBoxes;
	public float centerPosY;

	void Awake()
	{
		Instance = this;
	}

	void Update()
	{
		if ( alignToCenter )
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
}
