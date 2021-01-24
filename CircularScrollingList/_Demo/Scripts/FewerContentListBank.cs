using UnityEngine;

public class FewerContentListBank : BaseListBank
{
	private int[] contents = {
		1, 2, 3,
	};

	public override string GetListContent(int index)
	{
		return contents[index].ToString();
	}

	public override int GetListLength()
	{
		return contents.Length;
	}
}
