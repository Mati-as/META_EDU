using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class UI_Scene : UI_Base
{
	public override bool InitEssentialUI()
	{
		if (base.InitEssentialUI() == false)
			return false;

		//Managers.UI.SetCanvas(gameObject, false);
        SetUIManager();
		
		return true;
	}

    public virtual void OnPopupUI()
    {
        
    }
    
    private void SetUIManager()
    {
        Logger.CoreClassLog("Scene UI Init -------------------------");
        Debug.Assert(SceneManager.GetActiveScene().name.Contains("LAUNCHER"));
        
        //var isUIManagerLoadedOnRuntime = Managers.UI.ShowCurrentSceneUIManager<GameObject>(SceneManager.GetActiveScene().name);
  //      if (!isUIManagerLoadedOnRuntime) return; 
  
  
        var mainCamera = Camera.main;
        
        // UIManager가 로드된 경우, UICamera를 MainCamera의 Stack에 추가
        var uiCameraObj = GameObject.FindGameObjectWithTag("UICamera");

        Managers.UI.SetCanvas(uiCameraObj, true);
        
        Canvas canvas = uiCameraObj.GetComponentInChildren<Canvas>();
        if (canvas != null && uiCameraObj != null)
        {
            // Set the render mode and assign the camera
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCameraObj.GetComponent<Camera>();

            Logger.CoreClassLog("UICamera assigned to Canvas successfully.");
        }
        else
        {
            Logger.LogError("Canvas or Camera not found on UICamera object.");
        }
        if (uiCameraObj != null)
        {
            var uiCamera = uiCameraObj.GetComponent<Camera>();
            if (uiCamera != null)
            {
                uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            
                if (mainCamera != null && mainCamera.cameraType == CameraType.Game)
                {
                    if (!mainCamera.GetUniversalAdditionalCameraData().cameraStack.Contains(uiCamera))
                    {
                        mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(uiCamera);
                        Logger.ContentTestLog("UICamera가 MainCamera의 Stack에 추가됨.");
                    }
                }
                else
                    Logger.LogError("MainCamera가 없거나 올바르지 않은 타입입니다.");
            }
            else
                Logger.LogError("UICamera 오브젝트에 Camera 컴포넌트가 없습니다.");
        }
        else
            Logger.LogError("UICamera 태그를 가진 오브젝트를 찾을 수 없습니다.");
    }
}
