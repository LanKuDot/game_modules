/* Store the contents for ListBoxes to display.
 */
using UnityEngine;

public class ListBank : MonoBehaviour
{
	public static ListBank Instance;

	private int[] contents = {
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10
	};

	void Awake()
	{
		Instance = this;
	}

	public string getListContent(int index)
	{
		return contents[index].ToString();
	}

	public int getListLength()
	{
		return contents.Length;
	}
}
