using System;
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
        [SerializeField, Min(1)]
        [Tooltip("The number of boxes to be generated")]
        private int _numOfBoxes = 5;

        public Transform BoxRootTransform
        {
            get => _boxRootTransform;
            set => _boxRootTransform = value;
        }
        public ListBox BoxPrefab => _boxPrefab;
        public int NumOfBoxes => _numOfBoxes;

        /// <summary>
        /// Validate the setting
        /// </summary>
        /// <param name="listObject">The game object of the scrolling list</param>
        public void Validate(GameObject listObject)
        {
            var listName = listObject.name;

            if (!BoxRootTransform) {
                Debug.LogWarning(
                    $"The 'BoxRootTransform' is not assigned in the list '{listName}'. "
                    + "Use itself as the 'BoxRootTransform'");
                BoxRootTransform = listObject.transform;
            }

            if (!BoxPrefab)
                throw new UnassignedReferenceException(
                    $"The 'BoxPrefab' is not assigned in the list '{listName}'");

            if (NumOfBoxes <= 0)
                throw new InvalidOperationException(
                    $"The 'NumOfBoxes' is 0 or negative in the list '{listName}'");
        }
    }
}
