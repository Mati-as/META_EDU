using System;
using UnityEngine;
using System.Xml;
using UnityEngine.SocialPlatforms.Impl;


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

        //XML 관련 ----------------------------------------------------------------------------
        public static void LoadXML(ref TextAsset xmlAsset,ref XmlDocument xmlDoc, string path, ref string savePath)
        {
            xmlAsset = Resources.Load<TextAsset>(path); 
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlAsset.text);
            // Get the path to save the file later
           // savePath = System.IO.Path.Combine(Application.dataPath, savePath);
        }
        public static void AddUser(ref XmlDocument xmlDoc,string username,string score,string iconNumber)
        {
            XmlNode root = xmlDoc.DocumentElement;

            // Find the highest userID
            int highestUserID = -1;
            foreach (XmlNode node in root.SelectNodes("StringData"))
            {
                int userID = int.Parse(node.Attributes["userID"].Value);
                if (userID > highestUserID)
                {
                    highestUserID = userID;
                }
            }

            // Create a new user with the next available userID
            int newUserID = highestUserID + 1;

            
          
            XmlElement newUser = xmlDoc.CreateElement("StringData");
            newUser.SetAttribute("userID", newUserID.ToString());
            newUser.SetAttribute(nameof(username), username);
            newUser.SetAttribute((nameof(score)), score);
            newUser.SetAttribute("iconnumber", iconNumber);

            root.AppendChild(newUser);
        }

        // Save the XML file
        public static void SaveXML(ref XmlDocument xmlDoc,string xmlFilePath)
        {
            xmlDoc.Save(xmlFilePath);
            Debug.Log("XML file saved to: " + xmlFilePath);
        }
        //------------------------------------------------------------------------------------------
        
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
        
        public static T FindSomething<T>(GameObject go,string name) where T : UnityEngine.Object
        {
            if (go == null)
                return null;
            
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
