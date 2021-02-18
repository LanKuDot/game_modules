using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyListBank : BaseListBank
{
	private int[] _contents = {
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10
	};

	public override string GetListContent(int index)
	{
		return _contents[index].ToString();
	}

	public override int GetListLength()
	{
		return _contents.Length;
	}

	public void GetSelectedContentID(int contentID)
	{
		Debug.Log("Selected content ID: " + contentID.ToString() +
			", Content: " + GetListContent(contentID));
	}
}
