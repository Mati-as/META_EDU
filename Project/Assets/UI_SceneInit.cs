using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_SceneInit : MonoBehaviour
{
    void Start()
    {
        Managers.UI.ShowSceneUI<UI_MetaEduLauncherMaster>();
        Managers.UI.ShowPopupUI<UI_LoadInitialScene>();
    }


}
