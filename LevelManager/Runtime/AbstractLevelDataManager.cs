using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LevelManagement
{
    /// <summary>
    /// The abstract class for defining the necessary functions of LevelDataManager
    /// </summary>
    public abstract class AbstractLevelDataManager : MonoBehaviour
    {
        /// <summary>
        /// The number of levels
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Get the scene asset of the specified level
        /// </summary>
        /// <param name="id">The id of the level</param>
        public abstract AssetReference GetLevelScene(int id);

        /// <summary>
        /// Get the data to be passed in <c>LevelManager.OnLevelLoaded</c>
        /// when the level is loaded
        /// </summary>
        /// <param name="id">The id of the level</param>
        public abstract object GetLevelData(int id);

        /// <summary>
        /// Get the preview data to be displayed in the level selection UI
        /// </summary>
        /// <param name="id">The id of the level</param>
        public abstract object GetPreviewData(int id);
    }
}
