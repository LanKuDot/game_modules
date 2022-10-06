using UnityEngine;

namespace AirFishLab.ScrollingList.ListBoxManagement
{
    /// <summary>
    /// The data of the state of the list box
    /// </summary>
    public class ListBoxState
    {
        public Vector3 LocalPosition { get; private set; }
        public Vector3 LocalRotation { get; private set; }
        public Vector3 LocalScale { get; private set; }
        public int ContentID { get; private set; }
    }
}
