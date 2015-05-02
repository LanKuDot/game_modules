using UnityEngine;
using System.Collections;

public class ListBank : MonoBehaviour
{
	public static ListBank Instance;

	public int numOfListBoxes;

	void Awake()
	{
		Instance = this;
	}
}
