using System;
using System.Collections.Generic;
using UnityEngine;

namespace AirFishLab.ScrollingList
{
    [Serializable]
    public class ListBoxSetting
    {
        [SerializeField]
        [Tooltip("The root transform that holding the list boxes")]
        private Transform _boxRootTransform;
        [SerializeField]
        [Tooltip("The number of boxes to be generated")]
        private int _numOfBoxes = 5;
        [SerializeField]
        [Tooltip("The prefab of the box")]
        private ListBox _boxPrefab;
        [SerializeField]
        [Tooltip("The objects that are used for displaying the content. " +
                 "They should be derived from the class ListBox")]
        private List<ListBox> _listBoxes;

        public Transform BoxRootTransform => _boxRootTransform;
        public int NumOfBoxes => _numOfBoxes;
        public ListBox BoxPrefab => _boxPrefab;
        public List<ListBox> ListBoxes => _listBoxes;
    }
}
