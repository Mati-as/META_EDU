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


     
     /// <summary>
     /// 게임실행중 나갔을때 보여줄 해당 UI선택화면
     /// </summary>
     private static UI_PopUp _uiSelectionOnGameExit = null;
     public static UI_PopUp UISelectionOnGameExit
     {
         get
         {
             return _uiSelectionOnGameExit;}
         set
         {
             _uiSelectionOnGameExit = value;
             Logger.CoreClassLog("뒤로가기 _uiSelectionOnGameExit set to: " + _uiSelectionOnGameExit.GetType().Name);
         }
     } 
     public UI_PopUp currentPopupClass
     {
         get;
         set;
     }
     public string CurrentPopup
     {
         get;
         private set;
     }
     public string PreviousPopup
     {
         get;
         private set;
     }
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


    public int GetUICounts()
    {
        
        foreach (var popUP in _popupStack)
        {
            Logger.CoreClassLog($"[UI] Popup: {popUP.name}, Type: {popUP.GetType().Name}");
        }
        return _popupStack.Count;
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

		GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
		T sceneUI = Utils.GetOrAddComponent<T>(go);
		SceneUI = sceneUI;

		go.transform.SetParent(Root.transform);

		return sceneUI;
	}

    public T ShowPopupUI<T>(string name = null, Transform parent = null) where T : UI_PopUp
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        // 🔒 중복 팝업 검사
        foreach (var popup in _popupStack)
        {
            if (popup != null && popup.GetType() == typeof(T))
            {
                Debug.LogWarning($"[UI] Popup '{name}' is already open. Duplicate not allowed.");
                return popup as T;
            }
        }

        // 프리팹 로드 및 인스턴스화
        var prefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/Popup/{name}");
        var go = Managers.Resource.Instantiate($"UI/Popup/{name}");

        var popupInstance = Utils.GetOrAddComponent<T>(go);
        _popupStack.Push(popupInstance);

        if (parent != null)
            go.transform.SetParent(parent);
        else if (SceneUI != null)
            go.transform.SetParent(SceneUI.transform);
        else
            go.transform.SetParent(Root.transform);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = prefab.transform.position;

        Managers.UI.SceneUI.OnPopupUI();

        PreviousPopup = CurrentPopup;
        CurrentPopup = name;
        

      if (popupInstance.GetType() != typeof(UI_Confirmation)
          && popupInstance.GetType() !=typeof(UI_LoadInitialScene))currentPopupClass = popupInstance;

      Logger.CoreClassLog($"[UI] Popup '{name}' opened. Current Popup: {CurrentPopup}, Previous Popup: {PreviousPopup}");
        return popupInstance;
    }
    
    public UI_PopUp ShowPopupUI(Type type, string name = null, Transform parent = null)
    {
        if (!typeof(UI_PopUp).IsAssignableFrom(type))
        {
            Debug.LogError($"Type {type} is not a UI_PopUp");
            return null;
        }

        if (string.IsNullOrEmpty(name))
            name = type.Name;

        var prefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/Popup/{name}");
        var go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        var popupInstance = go.AddComponent(type) as UI_PopUp;

        _popupStack.Push(popupInstance);

        if (parent != null)
            go.transform.SetParent(parent);
        else if (SceneUI != null)
            go.transform.SetParent(SceneUI.transform);
        else
            go.transform.SetParent(Root.transform);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = prefab.transform.position;

        Managers.UI.SceneUI.OnPopupUI();

        PreviousPopup = CurrentPopup;
        CurrentPopup = name;
        
        
        
        if (popupInstance.GetType() != typeof(UI_Confirmation))currentPopupClass = popupInstance;

        return popupInstance;
    }


    private void SetPreviousClass()
    {
        
    }
    
    public void InitOnLauncherLoad(){
        _popupStack.Clear();
       // CurrentPopup = null;
        //PreviousPopup = null;
       // currentPopupClass = null;
    }
    
    /// <summary>
    /// ** 런쳐에서 사용 금지 -----------------------------각 씬별 GameManager용 입니다----------------------
    /// 개별컨트롤할 필요가 없는경우, Original UIManager를 사용합니다. 개별 UI가 필요없는경우 Original Prefab만 사용해도 무방합니다. 
    /// </summary>
    /// <returns></returns>
    public bool ShowCurrentSceneUIManager<T>(out GameObject uiGamobj,string sceneName = null, Transform parent = null) 
    {
        if (string.IsNullOrEmpty(sceneName))
            sceneName = typeof(T).Name;

        GameObject uiManagerOnScene = GameObject.Find($"{sceneName}_UIManager");
        if (uiManagerOnScene != null)
        {
            Logger.Log("UIManager 이미 씬에 있음");
            uiGamobj = uiManagerOnScene;
            return false;
        }
        
        var UIManagerPrefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/UIManagers/{sceneName}_UIManager");
        
        bool isUIManagerOnScene = false;
        

        
        
        if (UIManagerPrefab == null)
        {
            UIManagerPrefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/UIManagers/UIManager");
            
            if(UIManagerPrefab == null)
            {
                uiGamobj = null;
                Debug.LogError($"Prefab for {sceneName}_UIManager not found.");
                return false;
            }
            
            
            var go = Managers.Resource.Instantiate($"UI/UIManagers/UIManager");
            uiGamobj = go;
            Logger.CoreClassLog("Using Original UIManager Prefab.....");
        }
        else
        {
            var go = Managers.Resource.Instantiate($"UI/UIManagers/{sceneName}_UIManager");
            uiGamobj = go;
        }



       
        // if (parent != null)
        //     go.transform.SetParent(parent);
        // else if (SceneUI != null)
        //     go.transform.SetParent(SceneUI.transform);
        // else
        //     go.transform.SetParent(Root.transform);

        // go.transform.localScale = Vector3.one;
        // go.transform.localPosition = prefab.transform.position;

        return true;
    }
    

  
    public UI_PopUp ShowPopupUI(string className, Transform parent = null)
    {
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogError("className is null or empty.");
            return null;
        }

        // 프리팹 로드
        var prefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/Popup/{className}");
        if (prefab == null)
        {
            Debug.LogError($"Prefab for {className} not found.");
            return null;
        }

        var go = Managers.Resource.Instantiate($"UI/Popup/{className}");

        // 문자열로 타입 가져오기
        Type popupType = Type.GetType(className);
        if (popupType == null)
        {
            Debug.LogError($"Type {className} could not be found.");
            return null;
        }

        // Component 붙이기
        UI_PopUp popup = go.AddComponent(popupType) as UI_PopUp;
        if (popup == null)
        {
            Debug.LogError($"Type {className} is not a UI_PopUp.");
            return null;
        }

        _popupStack.Push(popup);

        // 부모 설정
        if (parent != null)
            go.transform.SetParent(parent);
        else if (SceneUI != null)
            go.transform.SetParent(SceneUI.transform);
        else
            go.transform.SetParent(Root.transform);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = prefab.transform.position;

        Managers.UI.SceneUI.OnPopupUI();

        PreviousPopup = CurrentPopup;
        CurrentPopup = className;

        return popup;
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