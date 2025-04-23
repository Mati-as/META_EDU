using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Confirmation : UI_PopUp
{
    public enum UI
    {
        Button_Yes,
        Button_No,
    }

    public override bool Init()
    {
        BindObject(typeof(UI));
        GetObject((int)UI.Button_Yes).BindEvent(() =>
        {
            Debug.Assert(UI_MetaEduLauncherMaster.GamenNameWaitingForConfirmation!=null);
            LoadScene(UI_MetaEduLauncherMaster.GamenNameWaitingForConfirmation);
        });
        GetObject((int)UI.Button_No).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI(this);
        });
        return base.Init();
        
    }
    
    public void LoadScene(string sceneName)
    {
        string originalName = sceneName;
        string modifiedName = originalName.Substring("SceneName_".Length);

        gameObject.SetActive(false);
        SceneManager.LoadScene(modifiedName);
    }

}
