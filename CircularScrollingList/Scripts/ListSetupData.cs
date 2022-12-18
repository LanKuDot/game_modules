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
        /// The setting of the list
        /// </summary>
        public readonly CircularScrollingListSetting Setting;

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

        #endregion

        public ListSetupData(
            CircularScrollingListSetting setting, RectTransform rectTransform,
            Camera canvasRefCamera, List<IListBox> listBoxes)
        {
            Setting = setting;
            RectTransform = rectTransform;
            CanvasRefCamera = canvasRefCamera;
            ListBoxes = listBoxes;
        }
    }
}
