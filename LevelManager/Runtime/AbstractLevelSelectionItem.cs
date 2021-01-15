using UnityEngine;

namespace LevelManagement
{
    /// <summary>
    /// The abstract class for defining the necessary functions of
    /// level selection item in UI
    /// </summary>
    public abstract class AbstractLevelSelectionItem : MonoBehaviour
    {
        /// <summary>
        /// Set the preview of the level selection item
        /// </summary>
        /// <param name="levelID">The level id of the item</param>
        /// <param name="previewData">The preview data of the item</param>
        public abstract void SetPreview(int levelID, object previewData);
    }
}
