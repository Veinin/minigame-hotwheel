using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace Game.Core.Assets
{
    public static partial class AssetManager
    {
        private static void OnLoadAssetCompleted<T>(AsyncOperationHandle<T> handle,
            string key,
            bool useReference,
            Action<string, T> onSucceeded = null,
            Action<string> onFailed = null) where T : Object
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!handle.Result)
            {
                if (!SuppressErrorLogs)
                    Debug.LogError(useReference ? AssetExceptions.CannotLoadAssetReference<T>(key) : AssetExceptions.CannotLoadAssetKey<T>(key));

                onFailed?.Invoke(key);
                return;
            }

            if (_assets.ContainsKey(key))
            {
                if (!(_assets[key] is T))
                {
                    if (!SuppressErrorLogs)
                    {
                        if (useReference)
                            Debug.LogError(AssetExceptions.AssetReferenceExist(_assets[key].GetType(), key));
                        else
                            Debug.LogError(AssetExceptions.AssetKeyExist(_assets[key].GetType(), key));
                    }

                    onFailed?.Invoke(key);
                    return;
                }
            }
            else
            {
                _assets.Add(key, handle.Result);
            }

            onSucceeded?.Invoke(key, handle.Result);
        }

        private static void OnLoadSceneCompleted(AsyncOperationHandle<SceneInstance> handle,
            string key,
            Action<SceneInstance> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _scenes.Add(key, handle.Result);
                onSucceeded?.Invoke(handle.Result);
            }
            else if (handle.Status == AsyncOperationStatus.Failed)
            {
                onFailed?.Invoke(key);
            }
        }

        private static void OnUnloadSceneCompleted(AsyncOperationHandle<SceneInstance> handle,
            string key,
            Action<string> onSucceeded,
            Action<string> onFailed)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onSucceeded?.Invoke(key);
            }
            else if (handle.Status == AsyncOperationStatus.Failed)
            {
                onFailed?.Invoke(key);
            }
        }

        private static void OnInstantiateCompleted(AsyncOperationHandle<GameObject> handle,
            string key,
            bool useReference,
            Action<string, GameObject> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!handle.Result)
            {
                if (!SuppressErrorLogs)
                {
                    if (useReference)
                        Debug.LogError(AssetExceptions.CannotInstantiateReference(key));
                    else
                        Debug.LogError(AssetExceptions.CannotInstantiateKey(key));
                }

                onFailed?.Invoke(key);
                return;
            }

            var result = handle.Result;

            //RectTransform rect = result.transform as RectTransform;
            //if (rect != null)
            //{
            //    rect.anchoredPosition = Vector3.zero;
            //    rect.localRotation = Quaternion.identity;
            //    rect.localScale = Vector3.one;

            //    if (rect.anchorMin.x != rect.anchorMax.x && rect.anchorMin.y != rect.anchorMax.y)
            //    {
            //        rect.offsetMin = Vector2.zero;
            //        rect.offsetMax = Vector2.zero;
            //    }
            //}

            if (!_instances.ContainsKey(key))
                _instances.Add(key, new List<GameObject>());

            _instances[key].Add(result);
            onSucceeded?.Invoke(key, result);
        }
    }

}