using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_SceneInit : MonoBehaviour
{
    void Start()
    {
        //순서주의------------------------
        // -PreInit : 이전 UI 선택정보, UI Pool 초기화 
        Managers.UI.InitOnLauncherLoad();
        
        
        //1. 씬 마스터 컨트롤용 UI 생성 
        var sceneUI = Managers.UI.ShowSceneUI<UI_MetaEduLauncherMaster>();
        
        //2. 로드 씬 생성 및 초기화 
        Managers.UI.ShowPopupUI<UI_LoadInitialScene>();
        //3. 카메라 관련 설정등을 위한 GameManager 로드용 팝업생성
        Managers.UI.ShowPopupUI<UI_GameManagerOnLauncher>();
        
        //4. 로드 씬 초기화
        sceneUI.UIOff();

    }



}
