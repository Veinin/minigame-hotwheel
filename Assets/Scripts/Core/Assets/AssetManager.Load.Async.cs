using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace Game.Core.Assets
{
    public static partial class AssetManager
    {
        public static void LoadAssetAsync<T>(string key,
            Action<string, T> onSucceeded,
            Action<string> onFailed = null)
            where T : Object
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                {
                    onSucceeded?.Invoke(key, asset);
                    return;
                }

                if (!SuppressWarningLogs)
                    Debug.LogWarning(AssetExceptions.AssetKeyNotInstanceOf<T>(key));

                onFailed?.Invoke(key);
                return;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, false, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        private static void ActivateScene(SceneInstance scene, int priority, Action<SceneInstance> onSucceeded)
        {
            var operation = scene.ActivateAsync();
            operation.priority = priority;
            operation.completed += _ => onSucceeded?.Invoke(scene);
        }

        public static void LoadSceneAsync(string key,
            Action<SceneInstance> onSucceeded,
            Action<string> onFailed = null,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateScene(scene, priority, onSucceeded);
                else
                    onSucceeded?.Invoke(scene);
                return;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                operation.Completed += handle => OnLoadSceneCompleted(handle, key, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void UnloadSceneAsync(string key,
            Action<string> onSucceeded = null,
            Action<string> onFailed = null,
            bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                onFailed?.Invoke(key);
                return;
            }

            _scenes.Remove(key);

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                operation.Completed += handle => OnUnloadSceneCompleted(handle, key, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        public static void InstantiateAsync(string key,
            Action<string, GameObject> onSucceeded,
            Action<string> onFailed = null,
            Transform parent = null,
            bool inWorldSpace = false,
            bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                operation.Completed += handle => OnInstantiateCompleted(handle, key, false, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }
    }

}