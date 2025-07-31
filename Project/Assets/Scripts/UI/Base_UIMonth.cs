using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class Base_UIMonth : UI_PopUp
{
    public override bool IsBackBtnClickable => true;

    public override bool InitOnLoad()
    {
        LoadUIElements();
        return base.InitOnLoad();
    }
    
    public enum UI_Elements
    {
        UI_SeasonTopNavi,

    }
  
    public void LoadUIElements()
    {
       
        InstantiateUI(UI_Elements.UI_SeasonTopNavi);
    }
    
    private GameObject InstantiateUI(UI_Elements element)
    {
        string prefabPath = $"Prefabs/UI/Popup/ETC/{element}";
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Logger.LogError($"❌ 프리팹 로드 실패: {prefabPath}");
            return null;
        }
 
        GameObject instance = Instantiate(prefab, gameObject.transform);
        instance.name = element.ToString(); // (Clone) 제거
        Logger.CoreClassLog("프리팹 로드 성공: " + element);
        return instance;
    }
}
