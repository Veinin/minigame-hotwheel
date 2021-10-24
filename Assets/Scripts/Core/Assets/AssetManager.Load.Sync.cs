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
        public static T LoadAsset<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return default;
            }

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T assetT)
                    return assetT;

                if (!SuppressWarningLogs)
                    Debug.LogWarning(AssetExceptions.AssetKeyNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                var result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, false);
                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        private static void ActivateSceneSync(in SceneInstance instance, int priority)
        {
            var operation = instance.ActivateAsync();
            operation.priority = priority;
            operation.WaitForCompletion();
        }

        public static SceneInstance LoadScene(string key,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return default;
            }

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                return scene;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                var result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        public static SceneInstance UnloadScene(string key, bool autoReleaseHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return default;
            }

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(AssetExceptions.NoSceneKeyLoaded(key));

                return default;
            }

            _scenes.Remove(key);

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                var result = operation.WaitForCompletion();
                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return scene;
            }
        }

        public static GameObject Instantiate(string key,
            Transform parent = null,
            bool inWorldSpace = false,
            bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return default;
            }

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                var result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, false);

                return result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }
    }

    internal static partial class AsyncOperationExtensions
    {
        public static void WaitForCompletion(this AsyncOperation operation)
        {
            new SyncOperationAwaiter(operation).WaitForCompletion();
        }
    }

    internal readonly struct SyncOperationAwaiter
    {
        private readonly AsyncOperation operation;

        public SyncOperationAwaiter(AsyncOperation operation)
        {
            this.operation = operation;
        }

        public bool IsCompleted
        {
            get
            {
                if (this.operation == null)
                    return true;

                return this.operation.isDone;
            }
        }

        public void WaitForCompletion()
        {
            while (!this.IsCompleted) { }
        }
    }
}