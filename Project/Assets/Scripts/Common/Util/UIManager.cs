using System.Collections.Generic;
using System.Linq;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using System;
using System.Reflection;

public class UIManager
{
    private int _order = -20;

     Stack<UI_PopUp> _popupStack = new Stack<UI_PopUp>();
    
     public UI_Scene SceneUI { get; private set; }

    public GameObject Root
    {
        get
        {
            var root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };

            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        var canvas = Utils.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }


    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        var prefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/SubItem/{name}");

        var go = Managers.Resource.Instantiate(prefab);
        if (parent != null)
            go.transform.SetParent(parent);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = prefab.transform.position;

        return Utils.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        var go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        var sceneUI = Utils.GetOrAddComponent<T>(go);
        SceneUI = sceneUI;

        go.transform.SetParent(Root.transform);

        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null, Transform parent = null) where T : UI_PopUp
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        var prefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/Popup/{name}");

        var go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        var popup = Utils.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        if (parent != null)
            go.transform.SetParent(parent);
        else if (SceneUI != null)
            go.transform.SetParent(SceneUI.transform);
        else
            go.transform.SetParent(Root.transform);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = prefab.transform.position;

        return popup;
    }
    
    public bool ShowCurrentSceneUIManager<T>(string sceneName = null, Transform parent = null) 
    {
        if (string.IsNullOrEmpty(sceneName))
            sceneName = typeof(T).Name;

        var prefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/UIManagers/{sceneName}_UIManager");

        if (prefab == null)
        {
            Logger.ContentTestLog("UIManager prefab is null");
            return false;
        }

        var go = Managers.Resource.Instantiate($"UI/UIManagers/{sceneName}_UIManager");

        if (parent != null)
            go.transform.SetParent(parent);
        else if (SceneUI != null)
            go.transform.SetParent(SceneUI.transform);
        else
            go.transform.SetParent(Root.transform);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = prefab.transform.position;

        return true;
    }
    

  
    public void ShowPopupUI(string className)
    {
        // 클래스 타입 가져오기
        // Type uiType = Type.GetType(className);
        //
        // if (uiType == null)
        // {
        //     Debug.LogError($"UI 클래스 {className}를 찾을 수 없습니다.");
        //     return;
        // }
        //
        // // Managers.UI.ShowPopupUI<T>() 호출
        // MethodInfo method = typeof(UI_PopUp).GetMethod("ShowPopupUI");
        // MethodInfo genericMethod = method?.MakeGenericMethod(uiType);
        // genericMethod.Invoke(Managers.UI, null);
    }





    public T FindPopup<T>() where T : UI_PopUp
    {
        return _popupStack.Where(x => x.GetType() == typeof(T)).FirstOrDefault() as T;
    }

    public T PeekPopupUI<T>() where T : UI_PopUp
    {
        if (_popupStack.Count == 0)
            return null;

        return _popupStack.Peek() as T;
    }

    public void ClosePopupUI(UI_PopUp popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_PopUp popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        SceneUI = null;
    }
}