﻿using UnityEngine;

namespace AirFishLab.ScrollingList
{
    /// <summary>
    /// The data of the input information
    /// </summary>
    public struct InputInfo
    {
        /// <summary>
        /// The phase of the input timing
        /// </summary>
        public InputPhase Phase;

        /// <summary>
        /// The local delta position of the input
        /// </summary>
        /// <remarks>
        /// If the <c>Phase</c> is Began, the value will be 0.
        /// If the <c>Phase</c> is Scrolled, the value will be scroll delta.
        /// </remarks>
        public Vector2 DeltaLocalPos;

        /// <summary>
        /// The normalized delta position whose value is [-1, 1]
        /// </summary>
        public Vector2 DeltaLocalPosNormalized;

        public override string ToString()
        {
            return $"Phase: {Phase}, " +
                   $"DeltaLocalPos: {DeltaLocalPos}, " +
                   $"DeltaLocalPosNormalized: {DeltaLocalPosNormalized}";
        }
    }

    /// <summary>
    /// The phase of the input timing
    /// </summary>
    public enum InputPhase
    {
        None,
        Began,
        Moved,
        Ended,
        Scrolled
    }
}
