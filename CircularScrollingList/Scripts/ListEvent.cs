using System;
using UnityEngine.Events;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// This event is used for passing the selected box
    /// </summary>
    [Serializable]
    public class ListBoxSelectedEvent : UnityEvent<ListBox>
    {}

    /// <summary>
    /// This event is used for passing the two list boxes
    /// </summary>
    [Serializable]
    public class ListTwoBoxesEvent : UnityEvent<ListBox, ListBox>
    {}
}
