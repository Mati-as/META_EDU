using System;
using System.Collections.Generic;
using System.Linq;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UIManager
{
    private int _order = -20;

    private static readonly Stack<UI_PopUp> _popupStack = new();
    

    private readonly Stack<Type> _previousPopupTypeStack = new();

    public void SavePreviousPopup()
    {
        _previousPopupTypeStack.Clear(); // 기존 저장 내용 초기화

        foreach (var popup in _popupStack) // 스택 순서 유지 위해 뒤집어서 push
            if (popup != null)
            {
                var popupType = popup.GetType();

                _previousPopupTypeStack.Push(popupType);
                Logger.CoreClassLog($"[UI] Saved popup TYPE: {popupType.Name}");
            }
    }

    /// <summary>
    ///     초기 UI (로딩,게임매니저) 등은 중복로드하지 않도록 설계중, 단순화 필요. 2025/07/04
    /// </summary>
    /// <returns></returns>
    public void LoadPopUp()
    {
        if (Managers.IsAlreadyFirstTimeHomeOpened)
        {
            Logger.CoreClassLog("저장된 씬");

            foreach (var uiType in _previousPopupTypeStack)
            {
                Logger.CoreClassLog($"[UI] Loading popup TYPE: {uiType.Name}");
                Managers.UI.ShowPopupUI(uiType);
            }
        }
        else
        {
            Logger.CoreClassLog("첫 로드씬 ---- 첫 로드씬인지 반드시 확인해야함 ");
            ShowPopupUI<UI_Home>();
        }

        Managers.IsAlreadyFirstTimeHomeOpened = true;
        Cursor.visible = true;  
    }

    /// <summary>
    ///     게임실행중 나갔을때 보여줄 해당 UI선택화면
    /// </summary>
    private static UI_PopUp _uiSelectionOnGameExit;

    public static UI_PopUp UISelectionOnGameExit
    {
        get
        {
            return _uiSelectionOnGameExit;
        }
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

    public UI_Scene SceneUI
    {
        get;
        private set;
    }

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
            canvas.sortingOrder = 0;
    }


    public int GetUICounts()
    {
        foreach (var popUP in _popupStack)
            Logger.CoreClassLog($"[UI] Popup: {popUP.name}, Type: {popUP.GetType().Name}");
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

    
    public UI_Master Master;
    
  
    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        var go = Managers.Resource.Instantiate($"UI/Scene/{name}"); //Awake()실행보장
        var sceneUI = Utils.GetOrAddComponent<T>(go);
        SceneUI = sceneUI;

        
        go.transform.SetParent(Root.transform);
        Master = Utils.GetOrAddComponent<UI_Master>(Root);
        
        Debug.Assert(Master!=null,"Master UI Can't be null");
        
        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null, Transform parent = null,string path = null) where T : UI_PopUp
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        // 🔒 중복 팝업 검사
        foreach (var popup in _popupStack)
            if (popup != null && popup.GetType() == typeof(T))
            {
                Debug.LogWarning($"[UI] Popup '{name}' is already open. Duplicate not allowed.");
                return null;
                return popup as T;
            }

        GameObject UIPrefab = null;
        GameObject go = null;
        // 프리팹 로드 및 인스턴스화
        if (string.IsNullOrEmpty(path))
        {
             UIPrefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/Popup/{name}");
             go= Managers.Resource.Instantiate($"UI/Popup/{name}");//Awake()실행보장

        }
        else
        {
            string manualPath = "UI/Popup/" + path + $"/{name}";
            Logger.Log($"[UI] Loading Popup from manual path: {manualPath}");
            
             UIPrefab = Managers.Resource.Load<GameObject>("Prefabs/"+manualPath);
             go= Managers.Resource.Instantiate(manualPath);//Awake()실행보장
        }
      
     
        var popupInstance = Utils.GetOrAddComponent<T>(go);
        _popupStack.Push(popupInstance);

        if (parent != null)
            go.transform.SetParent(parent);
        else if (SceneUI != null)
            go.transform.SetParent(SceneUI.transform);
        else
            go.transform.SetParent(Root.transform);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = UIPrefab.transform.position;


        PreviousPopup = CurrentPopup;
        CurrentPopup = name;


        if (popupInstance.GetType() != typeof(UI_Confirmation)
            && popupInstance.GetType() != typeof(UI_LoadInitialScene)) currentPopupClass = popupInstance;

        Managers.UI.SceneUI.OnPopupUI();
        
        // Logger.CoreClassLog(
        //     $"[UI] Popup '{name}' opened. Current Popup: {CurrentPopup}, Previous Popup: {PreviousPopup}");
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

        if (popupInstance!=null)
        {
            Master.SetBtnStatus(UI_Master.UI.Btn_Back,popupInstance.IsBackBtnClickable);
        }

        if (popupInstance.GetType() != typeof(UI_Confirmation)) currentPopupClass = popupInstance;

        return popupInstance;
    }


  
    public void InitOnLauncherLoad()
    {
        _popupStack.Clear();
        // CurrentPopup = null;
        //PreviousPopup = null;
        // currentPopupClass = null;
    }

    /// <summary>
    ///     ** 런쳐에서 사용 금지 -----------------------------각 씬별 GameManager용 입니다----------------------
    ///     개별컨트롤할 필요가 없는경우, Original UIManager를 사용합니다. 개별 UI가 필요없는경우 Original Prefab만 사용해도 무방합니다.
    /// </summary>
    /// <returns></returns>
    public bool ShowCurrentSceneUIManager<T>(out GameObject uiGamobj, string sceneName = null, Transform parent = null)
    {
        if (string.IsNullOrEmpty(sceneName))
            sceneName = typeof(T).Name;

        // 1.씬에 UIManager 배치여부 확인
        var uiManagerOnScene = GameObject.FindWithTag("UIManager");
        if (uiManagerOnScene != null)
        {
            Logger.Log("UIManager 이미 씬에 있음");
            uiGamobj = uiManagerOnScene;
            return false;
        }
        
        
        

        // 2.씬에 없는경우, Custom UIManager인지 확인
        var customUIManagerPrefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/UIManagers/{sceneName}_UIManager");
        
        
        
        // 3.Custom 아닌경우 Original UIManager를 사용
        if (customUIManagerPrefab == null)
        {
            var originalUIManager = Managers.Resource.Load<GameObject>("Prefabs/UI/UIManagers/UIManager");

            Debug.Assert(originalUIManager != null, $"Original UIManager prefab은 누락될 수 없음.");


            var go = Managers.Resource.Instantiate("UI/UIManagers/UIManager");
            uiGamobj = go;
            Logger.CoreClassLog("Using Original UIManager Prefab.....");
            return true;
        }
        else// 4.커스텀인경우 로드 
        {
            var go = Managers.Resource.Instantiate($"UI/UIManagers/{sceneName}_UIManager");
            uiGamobj = go;
            return true;
        }
        
    }


    // public UI_PopUp ShowPopupUI(string className, Transform parent = null)
    // {
    //     if (string.IsNullOrEmpty(className))
    //     {
    //         Debug.LogError("className is null or empty.");
    //         return null;
    //     }
    //
    //     // 프리팹 로드
    //     var prefab = Managers.Resource.Load<GameObject>($"Prefabs/UI/Popup/{className}");
    //     if (prefab == null)
    //     {
    //         Debug.LogError($"Prefab for {className} not found.");
    //         return null;
    //     }
    //
    //     var go = Managers.Resource.Instantiate($"UI/Popup/{className}");
    //
    //     // 문자열로 타입 가져오기
    //     var popupType = Type.GetType(className);
    //     if (popupType == null)
    //     {
    //         Debug.LogError($"Type {className} could not be found.");
    //         return null;
    //     }
    //
    //     // Component 붙이기
    //     var popup = go.AddComponent(popupType) as UI_PopUp;
    //     if (popup == null)
    //     {
    //         Debug.LogError($"Type {className} is not a UI_PopUp.");
    //         return null;
    //     }
    //
    //     _popupStack.Push(popup);
    //
    //     // 부모 설정
    //     if (parent != null)
    //         go.transform.SetParent(parent);
    //     else if (SceneUI != null)
    //         go.transform.SetParent(SceneUI.transform);
    //     else
    //         go.transform.SetParent(Root.transform);
    //
    //     go.transform.localScale = Vector3.one;
    //     go.transform.localPosition = prefab.transform.position;
    //
    //     Managers.UI.SceneUI.OnPopupUI();
    //
    //     PreviousPopup = CurrentPopup;
    //     CurrentPopup = className;
    //
    //     return popup;
    // }


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

        var type = _popupStack.Peek().GetType();


        var popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        int _closeCount = 0;
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
            _closeCount++;
        }
    }

    public void Clear()
    {
        CloseAllPopupUI();
        SceneUI = null;
    }
}