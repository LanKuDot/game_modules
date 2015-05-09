using UnityEngine;
using System.Collections;

public class ListBank : MonoBehaviour
{
	public static ListBank Instance;

	public int numOfListBoxes;

	private int[] contents = {
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10
	};

	void Awake()
	{
		Instance = this;
	}

	public int getListContent( int index )
	{
		return contents[ index ];
	}

	public int getListLength()
	{
		return contents.Length;
	}
}
