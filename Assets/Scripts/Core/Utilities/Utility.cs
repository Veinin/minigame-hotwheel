﻿using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Game.Core
{
    public class Utility
    {
        public struct MathRelate
        {
            /// <summary>
            /// 计算抛物线位置
            /// </summary>
            /// <param name="from">起点</param>
            /// <param name="to">终点</param>
            /// <param name="height">高度</param>
            /// <param name="t">比例</param>
            /// <returns></returns>
            public static Vector3 Parabola(Vector3 from, Vector3 to, float height, float t)
            {
                var py = -4*height*t*t + 4*height*t;
                var pos = Vector3.Lerp(from, to, t);
                return new Vector3(pos.x, py + Mathf.Lerp(from.y, to.y, t), pos.z);
            }
        }

        public struct CameraRelate
        {
            public static Vector2 GetScreenBounds() 
            {
                return new Vector3(Screen.width/100f, Screen.height/100f);
            }
        }

        public struct AssetRelate
        {
            /// <summary>
            /// Resources.Load 並檢查是否null
            /// </summary>
            public static T ResourcesLoadCheckNull<T>(string name) where T : UnityEngine.Object
            {
                T loadGo = Resources.Load<T>(name);

                if (loadGo == null)
                {
                    Debug.LogError("Resources.Load [ " + name + " ] is Null !!");
                    return default(T);
                }

                return loadGo;
            }

            /// <summary>
            /// Resources.Load Sprite
            /// </summary>
            public static Sprite ResourcesLoadSprite(string name)
            {
                return ResourcesLoadCheckNull<Sprite>("Sprites/" + name);
            }

            /// <summary>
            /// 讀TXT檔
            /// </summary>
            public static void LoadFile(string path)
            {
                string strTemp;
                TextAsset data = null;
                TextReader reader = null;

                data = Resources.Load(path, typeof(TextAsset)) as TextAsset;

                if (data != null)
                    reader = new StringReader(data.text);

                if (reader != null)
                {
                    while ((strTemp = reader.ReadLine()) != null)
                    {
                        Debug.Log(strTemp);
                    }

                    reader.Close();
                }
            }
        }


        public struct GameObjectRelate
        {
            /// <summary>
            /// 清除此父物件下子物件
            /// </summary>
            public static void ClearChildren(Transform Obj)
            {
                for (int i = Obj.childCount - 1; i >= 0; --i)
                {
                    Transform Item = Obj.GetChild(i);
                    Item.SetParent(null);
                    MonoBehaviour.DestroyImmediate(Item.gameObject);
                }
            }

            /// <summary>
            /// 在父物件下建立子物件(使用名稱建立一個新物件)
            /// </summary>
            public static GameObject InstantiateGameObject(GameObject parent, string name)
            {
                GameObject go = new GameObject(name);

                if (parent != null)
                {
                    Transform t = go.transform;
                    t.SetParent(parent.transform);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                    t.localScale = Vector3.one;

                    RectTransform rect = go.transform as RectTransform;
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector3.zero;
                        rect.localRotation = Quaternion.identity;
                        rect.localScale = Vector3.one;

                        //判斷anchor是否在同一點
                        if (rect.anchorMin.x != rect.anchorMax.x && rect.anchorMin.y != rect.anchorMax.y)
                        {
                            rect.offsetMin = Vector2.zero;
                            rect.offsetMax = Vector2.zero;
                        }
                    }

                    go.layer = parent.layer;
                }
                return go;
            }

            /// <summary>
            /// 在父物件下建立子物件
            /// </summary>
            public static GameObject InstantiateGameObject(GameObject parent, GameObject prefab)
            {

                GameObject go = GameObject.Instantiate(prefab) as GameObject;

                if (go != null && parent != null)
                {
                    Transform t = go.transform;
                    t.SetParent(parent.transform);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                    t.localScale = Vector3.one;

                    RectTransform rect = go.transform as RectTransform;
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector3.zero;
                        rect.localRotation = Quaternion.identity;
                        rect.localScale = Vector3.one;

                        //判斷anchor是否在同一點
                        if (rect.anchorMin.x != rect.anchorMax.x && rect.anchorMin.y != rect.anchorMax.y)
                        {
                            rect.offsetMin = Vector2.zero;
                            rect.offsetMax = Vector2.zero;
                        }
                    }

                    go.layer = parent.layer;
                }
                return go;
            }

            /// <summary>
            /// 查詢子物件
            /// </summary>
            public static Transform SearchChild(Transform target, string name)
            {
                if (target.name == name) return target;

                for (int i = 0; i < target.childCount; ++i)
                {
                    var result = SearchChild(target.GetChild(i), name);

                    if (result != null) return result;
                }

                return null;
            }
            /// <summary>
            /// 查詢多個子物件
            /// </summary>
            public static List<Transform> SearchChildsPartName(Transform target, string name)
            {
                List<Transform> objs = new List<Transform>();
                Transform child = null;

                for (int i = 0; i < target.childCount; ++i)
                {
                    child = target.GetChild(i);

                    if (child != null)
                    {
                        if (child.name.IndexOf(name, 0) >= 0)
                            objs.Add(child);
                    }
                }

                return objs;
            }

            /// <summary>
            /// 使用GetInstance比較GameObject
            /// </summary>
            public static bool CompareGameObject(GameObject A, GameObject B)
            {
                return A.GetInstanceID() == B.GetInstanceID() ? true : false;
            }

            /// <summary>
            /// GameObject Array 全開/全關
            /// </summary>
            public static void SetObjectArrayActive(GameObject[] gos, bool isActive)
            {
                for (int i = 0; i < gos.Length; i++)
                    gos[i].SetActive(isActive);
            }

            /// <summary>
            /// GameObject 開關
            /// </summary>
            public static void SetObjectActiveToggle(GameObject go)
            {
                go.SetActive(!go.activeSelf);
            }


            public delegate void SmallTabHandler();

            /// <summary>
            /// GameObject Array 中一個Active，其他InActive
            /// </summary>
            /// <param name="gos">GameObject Array<</param>
            /// <param name="id">第幾個index Active</param>
            /// <param name="callback">執行完callback</param>
            public static GameObject SetObjectArrayOneActive(GameObject[] gos, int id, SmallTabHandler callback = null)
            {
                foreach (GameObject go in gos)
                {
                    if (go != null)
                        go.SetActive(false);
                }


                if (callback != null)
                {
                    callback();
                }


                if (id == -1)
                    return null;

                gos[id].SetActive(true);

                return gos[id];
            }

            /// <summary>
            /// GameObject Array 排序
            /// </summary>
            public static void SortGameObjectArray(ref GameObject[] gos)
            {
                System.Array.Sort(gos, (a, b) => a.name.CompareTo(b.name));
            }

            /// <summary>
            /// GameObject Child 排序
            /// </summary>
            public static void SortHierarchyObjectChildByName(Transform parent)
            {
                List<Transform> children = new List<Transform>();
                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    {
                        Transform child = parent.GetChild(i);
                        children.Add(child);
                        child.parent = null;
                    }
                }

                children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
                foreach (Transform child in children)
                {
                    child.parent = parent;
                }
            }

            /// <summary>
            /// 使用已存在的 Compoent 加入 GameObject
            /// </summary>
            public static T AddComponent<T>(GameObject go, T toAdd) where T : Component
            {
                return GetCopyOf(go.AddComponent<T>(), toAdd) as T;
            }

            public static T GetCopyOf<T>(Component comp, T other) where T : Component
            {
                Type type = comp.GetType();
                if (type != other.GetType()) return null; // type mis-match
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
                PropertyInfo[] pinfos = type.GetProperties(flags);
                foreach (var pinfo in pinfos)
                {
                    if (pinfo.CanWrite)
                    {
                        try
                        {
                            pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                        }
                        catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                    }
                }
                FieldInfo[] finfos = type.GetFields(flags);
                foreach (var finfo in finfos)
                {
                    finfo.SetValue(comp, finfo.GetValue(other));
                }
                return comp as T;
            }

            /// <summary>
            /// 改變物體layer (包含所有子物體)
            /// </summary>
            public static void SetLayerRecursively(GameObject go, int layerNumber)
            {
                if (go == null) return;
                foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = layerNumber;
                }
            }
        }

    }

}