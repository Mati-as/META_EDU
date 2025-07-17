using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Confirmation : UI_PopUp
{
    public enum UI
    {
        Btn_Yes,
        Btn_No,
    }

    public enum TMP
    {
        tmpConfirm
    }

    public override bool InitOnLoad()
    {
        Debug.Assert(UI_Master.GameNameWaitingForConfirmation!=null);
        BindObject(typeof(UI));
        BindTMP(typeof(TMP));

        GetTMP((int)TMP.tmpConfirm).text = $"해당 놀이를 시작 할까요?\n<color=#FDF06D>- {UI_Master.GameKoreanName} -</color>";

        Logger.CoreClassLog($"로드할 게임이름 : {UI_Master.GameNameWaitingForConfirmation}");
        
        GetObject((int)UI.Btn_Yes).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI(); //Confirmation 팝업 닫기
            Managers.UI.SavePreviousPopup();
            // UIManager.UISelectionOnGameExit = Managers.UI.currentPopupClass;
            // Logger.CoreClassLog($"뒤로가기시 실행버튼 -------{UIManager.UISelectionOnGameExit}");
            LoadScene(UI_Master.GameNameWaitingForConfirmation);
        });
        GetObject((int)UI.Btn_No).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
        });
        
        return base.InitOnLoad();
        
    }
    
    public void LoadScene(string sceneName)
    {
        // string originalName = sceneName;
        // string modifiedName = originalName.Substring("SceneName_".Length);
       
        gameObject.SetActive(false);
        SceneManager.LoadScene(sceneName);
    }

}
