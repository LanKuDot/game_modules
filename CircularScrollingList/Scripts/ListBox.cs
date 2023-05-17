using System;
using AirFishLab.ScrollingList.ContentManagement;
using AirFishLab.ScrollingList.Util;
using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The basic component of the scrolling list.
    /// Control the position and update the content of the list element.
    /// </summary>
    public class ListBox : MonoBehaviour, IListBox
    {
        #region Properties of IListBox

        public int ListBoxID { get; private set; }
        public int ContentID { get; private set; }
        public IListBox LastListBox { get; private set; }
        public IListBox NextListBox { get; private set; }
        public ListBoxSelectedEvent OnBoxSelected { get; } = new ListBoxSelectedEvent();
        public CircularScrollingList ScrollingList { get; private set; }
        public bool IsActivated
        {
            get => _gameObject.activeSelf;
            set => _gameObject.SetActive(value);
        }

        #endregion

        #region Private Members

        private GameObject _gameObject;
        private Transform _transform;
        private Func<Vector2, float> _factorFunc;

        #endregion

        #region IListBox

        public void Initialize(
            CircularScrollingList scrollingList,
            int listBoxID, IListBox lastListBox, IListBox nextListBox)
        {
            ScrollingList = scrollingList;
            ListBoxID = listBoxID;
            LastListBox = lastListBox;
            NextListBox = nextListBox;

            _gameObject = gameObject;
            _transform = transform;
            if (scrollingList.ListSetting.Direction
                == CircularScrollingList.Direction.Horizontal)
                _factorFunc = FactorUtility.GetVector2X;
            else
                _factorFunc = FactorUtility.GetVector2Y;

            RegisterClickEvent();

            OnInitialized();
        }

        public Transform GetTransform()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return transform;
#endif
            return _transform;
        }

        public float GetPositionFactor()
        {
            return _factorFunc(_transform.localPosition);
        }

        public virtual void OnBoxMoved(float positionRatio)
        {}

        public void SetContentID(int contentID)
        {
            ContentID = contentID;
        }

        public void SetContent(IListContent content)
        {
            UpdateDisplayContent(content);
        }

        public void PopToFront()
        {
            transform.SetAsLastSibling();
        }

        public void PushToBack()
        {
            transform.SetAsFirstSibling();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Register the callback to the button clicking event
        /// </summary>
        private void RegisterClickEvent()
        {
            if (TryGetComponent<Button>(out var button)) {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnButtonClick()
        {
            OnBoxSelected?.Invoke(this);
        }

        #endregion

        #region Content Handling

        /// <summary>
        /// This function is called after the box is initialized
        /// </summary>
        protected virtual void OnInitialized()
        {}

        /// <summary>
        /// Update the displaying content on the ListBox
        /// </summary>
        /// <param name="content">The content to be displayed</param>
        protected virtual void UpdateDisplayContent(IListContent content)
        {
            Debug.Log(content);
        }

        #endregion
    }
}
