using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace LevelManagement
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        /// <summary>
        /// The event will be invoked when a level is loaded<para />
        /// The first argument is the level id, and the second argument
        /// is the level data provided by the level data manager.
        /// </summary>
        public event Action<int, object> OnLevelLoaded;
        /// <summary>
        /// The event will be invoked when all the levels are passed
        /// no matter the <c>_loadSceneMode</c>.
        /// </summary>
        public event Action OnAllLevelsPassed;

        [SerializeField]
        [Tooltip("The data manager that provides the level data")]
        private AbstractLevelDataManager _levelDataManager = null;
        [SerializeField]
        [Tooltip(
            "Whether to start from the first level when all levels are passed or not")]
        private bool _loopLevel = false;
        [SerializeField]
        [Tooltip("The mode of loading level scene")]
        private LoadSceneMode _loadSceneMode = LoadSceneMode.Single;
        [SerializeField]
        [Tooltip("The scene where the LevelManager is initially at")]
        private AssetReference _homeScene = null;
        [SerializeField]
        [Tooltip(
            "The scene that keeps existing through levels. " +
            "This is used when the loading mode is Additive")]
        private AssetReference _topScene = null;

        public int NumOfLevel => _levelDataManager.Length;

        private int _curLevelID = 0;
        private AsyncOperationHandle<SceneInstance> _topSceneHandle;
        private AsyncOperationHandle<SceneInstance> _levelSceneHandle;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else if (Instance != this) {
                Destroy(gameObject);
            }
        }

        #region Data Getter

        /// <summary>
        /// Check if the level is valid
        /// </summary>
        /// <param name="id">The target level id</param>
        /// <exception cref="IndexOutOfRangeException">
        /// The exception is raised if the level id is invalid
        /// </exception>
        private void ValidateLevelID(int id)
        {
            if (id < 0 || id >= _levelDataManager.Length)
                throw new IndexOutOfRangeException(
                    "The specified level id is out of range");
        }

        /// <summary>
        /// Get the level data of current loaded level
        /// </summary>
        public object GetCurLevelData()
        {
            ValidateLevelID(_curLevelID);
            return _levelDataManager.GetLevelData(_curLevelID);
        }

        /// <summary>
        /// Get the preview data of a level
        /// </summary>
        /// <param name="id">The level id</param>
        public object GetLevelPreviewData(int id)
        {
            ValidateLevelID(id);
            return _levelDataManager.GetPreviewData(id);
        }

        #endregion

        #region Level Operation

        /// <summary>
        /// Load the specified level
        /// </summary>
        /// <param name="id">The level ID</param>
        public void LoadLevel(int id)
        {
            ValidateLevelID(id);
            _curLevelID = id;
            LoadLevelScene();
        }

        /// <summary>
        /// Load the next level
        /// </summary>
        public void NextLevel()
        {
            ++_curLevelID;
            if (_curLevelID >= _levelDataManager.Length) {
                OnAllLevelsPassed?.Invoke();
                if (!_loopLevel)
                    return;

                _curLevelID = 0;
            }

            LoadLevelScene();
        }

        /// <summary>
        /// Reload the current level
        /// </summary>
        public void ReloadLevel()
        {
            LoadLevelScene();
        }

        /// <summary>
        /// Load the home scene<para />
        /// The scene will be loaded in the mode of Single.
        /// </summary>
        public void LoadHomeScene()
        {
            SceneLoader.LoadScene(_homeScene, LoadSceneMode.Single, null);
        }

        #endregion

        #region Scene Loader

        /// <summary>
        /// Load the level scene
        /// </summary>
        private void LoadLevelScene()
        {
            if (_loadSceneMode == LoadSceneMode.Additive)
                AdditiveLoadLevelScene();
            else
                LoadCurrentIDLevel();
        }

        /// <summary>
        /// Load level scene additively
        /// </summary>
        private void AdditiveLoadLevelScene()
        {
            // Check if the top scene is loaded, if not, load it first
            if (!_topSceneHandle.IsValid()) {
                SceneLoader.LoadScene(
                    _topScene, LoadSceneMode.Single,
                    handle =>
                    {
                        _topSceneHandle = handle;
                        AdditiveLoadLevelScene();
                    });
                return;
            }

            // If the level scene is loaded before, unload it.
            if (_levelSceneHandle.IsValid()) {
                SceneLoader.UnloadScene(
                    _levelSceneHandle,
                    handle => { AdditiveLoadLevelScene(); });
                return;
            }

            LoadCurrentIDLevel();
        }

        /// <summary>
        /// Load the level scene according to the current level ID
        /// </summary>
        private void LoadCurrentIDLevel()
        {
            SceneLoader.LoadScene(
                _levelDataManager.GetLevelScene(_curLevelID),
                _loadSceneMode,
                OnLevelSceneLoaded);
        }

        /// <summary>
        /// The callback to be invoked when the SceneLoader loaded the level scene
        /// </summary>
        private void OnLevelSceneLoaded(AsyncOperationHandle<SceneInstance> handle)
        {
            OnLevelLoaded?.Invoke(
                _curLevelID, _levelDataManager.GetLevelData(_curLevelID));
            _levelSceneHandle = handle;
        }

        #endregion
    }
}
