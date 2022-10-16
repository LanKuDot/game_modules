using AirFishLab.ScrollingList.ListStateProcessing;
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
        /// The component for managing the boxes
        /// </summary>
        public readonly IListBoxManager ListBoxManager;

        #endregion

        public ListSetupData(
            CircularScrollingListSetting setting, RectTransform rectTransform,
            Camera canvasRefCamera, IListBoxManager listBoxManager)
        {
            Setting = setting;
            RectTransform = rectTransform;
            CanvasRefCamera = canvasRefCamera;
            ListBoxManager = listBoxManager;
        }
    }
}
