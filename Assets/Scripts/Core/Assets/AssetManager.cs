using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace Game.Core.Assets
{
    public static partial class AssetManager
    {
        public enum ExceptionHandleType
        {
            Log, Throw, Suppress
        }

        const string _baseDir = "Assets/";

        private static readonly List<object> _keys;
        private static readonly Dictionary<string, Object> _assets;
        private static readonly Dictionary<string, SceneInstance> _scenes;
        private static readonly Dictionary<string, List<GameObject>> _instances;
        private static readonly List<GameObject> _noInstanceList;

        public static ExceptionHandleType ExceptionHandle { get; set; }
        public static bool SuppressWarningLogs { get; set; }
        public static bool SuppressErrorLogs { get; set; }

        static AssetManager()
        {
            _keys = new List<object>();
            _assets = new Dictionary<string, Object>();
            _scenes = new Dictionary<string, SceneInstance>();
            _instances = new Dictionary<string, List<GameObject>>();
            _noInstanceList = new List<GameObject>(0);
        }

        private static void Clear()
        {
            _keys.Clear();
            _assets.Clear();
            _scenes.Clear();
            _instances.Clear();
        }

        private static bool GuardKey(string key, out string result)
        {
            result = key ?? string.Empty;
            return !string.IsNullOrEmpty(key);
        }

        private static void ThrowInvalidKeyException(string key)
        {
            if (ExceptionHandle == ExceptionHandleType.Throw)
                throw new InvalidKeyException(key);
            else if (ExceptionHandle == ExceptionHandleType.Log)
                Debug.LogException(new InvalidKeyException(key));
        }

        public static bool ContainsKey(object key) => _keys.Contains(key);
        public static bool ContainsAsset(string key) => _assets.ContainsKey(key) && _assets[key];

        public static bool TryGetScene(string key, out SceneInstance scene)
        {
            scene = default;

            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return false;
            }

            if (_scenes.TryGetValue(key, out var value))
            {
                scene = value;
                return true;
            }

            if (!SuppressWarningLogs)
                Debug.LogWarning($"No scene with key={key} has been loaded through {nameof(AssetManager)}.");

            return false;
        }

        public static T GetAsset<T>(string key) where T : Object
        {
            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return default;
            }

            if (!_assets.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(AssetExceptions.CannotFindAssetByKey(key));

                return default;
            }

            if (_assets[key] is T asset)
                return asset;

            if (!SuppressWarningLogs)
                Debug.LogWarning(AssetExceptions.AssetKeyNotInstanceOf<T>(key));

            return default;
        }

        public static bool TryGetAsset<T>(string key, out T asset) where T : Object
        {
            asset = default;

            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return false;
            }

            if (!_assets.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(AssetExceptions.CannotFindAssetByKey(key));

                return false;
            }

            if (_assets[key] is T assetT)
            {
                asset = assetT;
                return true;
            }

            if (!SuppressWarningLogs)
                Debug.LogWarning(AssetExceptions.AssetKeyNotInstanceOf<T>(key));

            return false;
        }

        public static void ReleaseAsset(string key)
        {
            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return;
            }

            if (!_assets.TryGetValue(key, out var asset))
                return;

            _assets.Remove(key);
            Addressables.Release(asset);
        }

        public static IReadOnlyList<GameObject> GetInstances(string key)
        {
            if (_instances.TryGetValue(key, out var instanceList))
                return instanceList;

            return _noInstanceList;
        }

        public static void ReleaseInstances(string key)
        {
            if (!_instances.TryGetValue(key, out var instanceList))
                return;

            _instances.Remove(key);

            foreach (var instance in instanceList)
            {
                Addressables.ReleaseInstance(instance);
            }
        }

        public static void ReleaseInstance(string key, GameObject instance)
        {
            if (!GuardKey(key, out key))
            {
                ThrowInvalidKeyException(key);
                return;
            }

            if (!instance)
                return;

            if (!_instances.TryGetValue(key, out var instanceList))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(AssetExceptions.NoInstanceKeyInitialized(key), instance);

                return;
            }

            var index = instanceList.FindIndex(x => x.GetInstanceID() == instance.GetInstanceID());
            if (index < 0)
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(AssetExceptions.NoInstanceKeyInitialized(key), instance);

                return;
            }

            instanceList.RemoveAt(index);
            Addressables.ReleaseInstance(instance);

            if (instanceList.Count > 0)
                return;

            _instances.Remove(key);
        }
    }
}