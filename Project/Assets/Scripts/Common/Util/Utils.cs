using System;
using UnityEngine;
using System.Xml;


    public class Utils :MonoBehaviour
    {
        
        public static T FindComponentInSiblings<T>(Transform  transform) where T : Component
        {
            // 부모 게임 오브젝트를 얻습니다.
            Transform parent = transform.parent;

            // 부모 게임 오브젝트가 없다면, 형제가 없음을 의미하므로 null을 반환합니다.
            if (parent == null)
                return null;

            // 부모의 모든 자식을 순회합니다.
            foreach (Transform sibling in parent)
            {
                // 현재 게임 오브젝트를 제외한 모든 형제에서 컴포넌트를 찾습니다.
                if (sibling != transform)
                {
                    T component = sibling.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }

            // 원하는 컴포넌트를 찾지 못한 경우 null을 반환합니다.
            return null;
        }

        public static void LoadXML(ref TextAsset xmlAsset,ref XmlDocument xmlDoc, string path)
        {
            xmlAsset = Resources.Load<TextAsset>(path); 
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlAsset.text);
        }
        
        public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
                component = go.AddComponent<T>();
            return component;
        }

        
        public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
        {
            if (go == null)
                return null;

            if (recursive == false)
            {
                Transform transform = go.transform.Find(name);
                if (transform != null)
                    return transform.GetComponent<T>();
            }
            else
            {
                foreach (T component in go.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                        return component;
                }
            }

            return null;
        }

        public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
        {
            Transform transform = FindChild<Transform>(go, name, recursive);
            if (transform != null)
                return transform.gameObject;
            return null;
        }
        
        
        public static void Shuffle<T>(T[] array) where T : UnityEngine.Object
        {
            for (int i = 0; i < array.Length; i++)
            {
                T temp = array[i];
                int randomIndex = UnityEngine.Random.Range(i, array.Length);
                array[i] = array[randomIndex];
                array[randomIndex] = temp;
            }
        }

    }
