﻿using System.Collections.Generic;

namespace AirFishLab.ScrollingList.ListBoxManagement
{
    public class ListBoxManager : IListBoxManager
    {
        #region Private Members

        /// <summary>
        /// The managed boxes
        /// </summary>
        private readonly List<ListBox> _boxes = new List<ListBox>();

        /// <summary>
        /// The number of boxes
        /// </summary>
        private int _numOfBoxes;

        #endregion

        public void SetBoxes(IEnumerable<ListBox> boxes)
        {
            _boxes.Clear();
            _boxes.AddRange(boxes);
        }

        public void UpdateBoxes()
        {
            throw new System.NotImplementedException();
        }
    }
}
