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

        #region Private Members

        /// <summary>
        /// The name of the list
        /// </summary>
        private string _name;
        /// <summary>
        /// Is the setting initialized?
        /// </summary>
        private bool _isInitialized;

        #endregion

        #region Property Setter

        public void SetBoxRootTransform(RectTransform rootTransform)
        {
            if (CheckIsInitialized())
                return;

            _boxRootTransform = rootTransform;
        }

        public void SetBoxPrefab(ListBox boxPrefab)
        {
            if (CheckIsInitialized())
                return;

            _boxPrefab = boxPrefab;
        }

        public void SetNumOfBoxes(int numOfBoxes)
        {
            if (CheckIsInitialized())
                return;

            _numOfBoxes = numOfBoxes;
        }

        #endregion

        /// <summary>
        /// Is the box setting initialized?
        /// </summary>
        private bool CheckIsInitialized()
        {
            if (_isInitialized)
                Debug.LogWarning(
                    $"The list setting of the list '{_name}' is initialized. Skip");

            return _isInitialized;
        }

        /// <summary>
        /// Initialize the setting
        /// </summary>
        /// <param name="listObject">The game object of the scrolling list</param>
        public void Initialize(GameObject listObject)
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

            _name = listName;
            _isInitialized = true;
        }
    }
}
