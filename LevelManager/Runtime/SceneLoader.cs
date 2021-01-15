using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace LevelManagement
{
    public static class SceneLoader
    {
        public static void LoadScene(
            AssetReference scene, LoadSceneMode mode,
            Action<AsyncOperationHandle<SceneInstance>> onLoadCompleted)
        {
            Addressables.LoadSceneAsync(scene, mode).Completed += onLoadCompleted;
        }

        public static void UnloadScene(
            AsyncOperationHandle<SceneInstance> handle,
            Action<AsyncOperationHandle<SceneInstance>> onUnloadCompleted)
        {
            Addressables.UnloadSceneAsync(handle).Completed += onUnloadCompleted;
        }
    }
}
