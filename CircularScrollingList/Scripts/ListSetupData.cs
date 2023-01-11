using System.Collections.Generic;
using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The data for setting up the list
    /// </summary>
    public class ListSetupData
    {
        #region Public Members

        /// <summary>
        /// The scrolling list
        /// </summary>
        public readonly CircularScrollingList ScrollingList;

        /// <summary>
        /// The setting of the list
        /// </summary>
        public readonly ListSetting ListSetting;

        /// <summary>
        /// The root rect transform of the list
        /// </summary>
        public readonly RectTransform RectTransform;

        /// <summary>
        /// The camera that the root canvas is referenced
        /// </summary>
        /// If the canvas is in "Screen Space - Overlay" mode, it will be null.
        public readonly Camera CanvasRefCamera;

        /// <summary>
        /// The boxes in the list
        /// </summary>
        public readonly List<IListBox> ListBoxes;

        /// <summary>
        /// The components for providing the list contents
        /// </summary>
        public readonly ListContentProvider ListContentProvider;

        #endregion

        public ListSetupData(
            CircularScrollingList scrollingList,
            ListSetting listSetting, RectTransform rectTransform,
            Camera canvasRefCamera, List<IListBox> listBoxes,
            ListContentProvider listContentProvider)
        {
            ScrollingList = scrollingList;
            ListSetting = listSetting;
            RectTransform = rectTransform;
            CanvasRefCamera = canvasRefCamera;
            ListBoxes = listBoxes;
            ListContentProvider = listContentProvider;
        }
    }
}
