using System;
using UnityEngine;


    public class Util :MonoBehaviour
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
    }
