/* Store the contents for ListBoxes to display.
 */
using UnityEngine;

/* The interface of the list content container
 *
 * Create the individual ListBank for each list by inheriting this interface
 */
public interface IBaseListBank
{
	string GetListContent(int index);
	int GetListLength();
}

/* The example of the ListBank
 */
public class ListBank : MonoBehaviour, IBaseListBank
{
	private int[] contents = {
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10
	};

	public string GetListContent(int index)
	{
		return contents[index].ToString();
	}

	public int GetListLength()
	{
		return contents.Length;
	}
}
