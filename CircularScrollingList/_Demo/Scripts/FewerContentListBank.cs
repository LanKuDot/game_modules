using AirFishLab.ScrollingList;
using UnityEngine;

public class FewerContentListBank : BaseListBank
{
    private int[] contents = {
        1, 2, 3,
    };

    public override object GetListContent(int index)
    {
        return contents[index];
    }

    public override int GetListLength()
    {
        return contents.Length;
    }
}
