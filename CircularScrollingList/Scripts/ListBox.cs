using AirFishLab.ScrollingList.ContentManagement;
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

        public Transform Transform => transform;
        public int ListBoxID { get; private set; }
        public int ContentID { get; private set; }
        public IListBox LastListBox { get; private set; }
        public IListBox NextListBox { get; private set; }
        public ListBoxIntEvent OnBoxClick { get; } = new ListBoxIntEvent();
        public bool IsActivated
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        #endregion

        #region Exposed Properties

        /// <summary>
        /// The list which this box belongs to
        /// </summary>
        public CircularScrollingList scrollingList { get; private set; }

        #endregion

        #region IListBox

        public void Initialize(
            int listBoxID, IListBox lastListBox, IListBox nextListBox)
        {
            ListBoxID = listBoxID;
            LastListBox = lastListBox;
            NextListBox = nextListBox;
            RegisterClickEvent();
        }

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
            OnBoxClick?.Invoke(ContentID);
        }

        #endregion

        #region Content Handling

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
