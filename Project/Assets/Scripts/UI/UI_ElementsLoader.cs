using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 프리팹 로드 담당
/// 2. 추후 Addressable로 변경 예정
/// </summary>
public class UI_ElementsLoader : Ex_MonoBehaviour
{
    public enum UI_Elements
    {
     UI_SideMenu,
     UI_Intro,
     UI_Instruction,
     OptionalTools,
     //Optional Tools
     //Optional
     UI_MediaArtInScene,
     UI_ReadyAndStart
    }


    private GameObject parent;
    public void LoadUIElements(GameObject parent)
    {
        this.parent = parent;
        InstantiateUI(UI_Elements.UI_SideMenu);
        InstantiateUI(UI_Elements.UI_Intro);
        InstantiateUI(UI_Elements.UI_Instruction);
        
        
        
        var MediaArtUI = InstantiateUI(UI_Elements.UI_MediaArtInScene);
        if (MediaArtUI != null)
            MediaArtUI.transform.localScale = Vector3.zero;
        
        
        var obj = InstantiateUI(UI_Elements.UI_ReadyAndStart);
 
        base.Init();
    }
    
    private GameObject InstantiateUI(UI_Elements element)
    {
        string prefabPath = $"UI/Elements/{element}";
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Logger.LogError($"❌ 프리팹 로드 실패: {prefabPath}");
            return null;
        }

        GameObject instance = Instantiate(prefab, parent.transform);
        instance.name = element.ToString(); // (Clone) 제거
        Logger.CoreClassLog("프리팹 로드 성공: " + element);
        return instance;
    }
}
