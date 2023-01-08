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
        [Tooltip("The prefab of the box")]
        private ListBox _boxPrefab;
        [SerializeField]
        [Tooltip("The objects that are used for displaying the content. " +
                 "They should be derived from the class ListBox")]
        private List<ListBox> _listBoxes;

        public Transform BoxRootTransform
        {
            get => _boxRootTransform;
            set => _boxRootTransform = value;
        }
        public ListBox BoxPrefab => _boxPrefab;
        public List<ListBox> ListBoxes => _listBoxes;

        /// <summary>
        /// Validate the setting
        /// </summary>
        /// <param name="listObject">The game object of the scrolling list</param>
        public void Validate(GameObject listObject)
        {
            if (!BoxRootTransform) {
                Debug.LogWarning(
                    $"{listObject.name}: "
                    + "The box root transform is not specified. "
                    + "Use itself as the box root transform");
                BoxRootTransform = listObject.transform;
            }

            if (!BoxPrefab)
                throw new InvalidOperationException(
                    $"{listObject.name}: The box prefab is not set");

            if (_listBoxes.Count == 0)
                throw new InvalidOperationException(
                    $"{listObject.name}: The size of list boxes is 0");
        }
    }
}
