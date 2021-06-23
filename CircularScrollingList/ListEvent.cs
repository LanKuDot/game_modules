using System;
using UnityEngine.Events;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The event is fired when a box in the list is clicked<para/>
    /// The int parameter is the content ID of the clicked box
    /// </summary>
    [Serializable]
    public class ListBoxClickEvent : UnityEvent<int>
    {
    }
}
