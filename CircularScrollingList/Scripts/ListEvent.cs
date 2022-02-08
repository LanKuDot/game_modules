using System;
using UnityEngine.Events;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// This event is used for passing the content ID
    /// The int parameter is the content ID of the clicked box
    /// </summary>
    [Serializable]
    public class ListBoxIntEvent : UnityEvent<int>
    {}

    /// <summary>
    /// This event is used for passing the two list boxes
    /// </summary>
    [Serializable]
    public class ListTwoBoxesEvent : UnityEvent<ListBox, ListBox>
    {}
}
