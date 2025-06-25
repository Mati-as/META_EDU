
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// ui_Scene 으로, 항시 켜져있는 UI 표출중 
/// </summary>
public class UI_MetaEduLauncherMaster : UI_Scene
{
    private static bool _isLoadFinished;
    private Vector3 screenPosition;
    private PointerEventData _launcherPED;
    private List<RaycastResult> _results;
    private GraphicRaycaster _launcherGR;
    private RaySynchronizer _raySynchronizer;


    public static string GameNameWaitingForConfirmation
    {
        get;
        private set; 
    }
    public static string GameKoreanName
    {
        get;
        private set; 
    }
    private enum UI
    {
        Btn_Home,
        //Btn_Setting,
        Btn_Quit,
        Btn_Back
    }

    private readonly bool _isClikcable = true;
    private const int DEFAULT_UI_COUNT = 2; // UI_Home, 

    public override bool InitEssentialUI()
    {
        BindObject(typeof(UI));

        _raySynchronizer = GameObject.FindWithTag("RaySynchronizer").GetComponent<RaySynchronizer>();
        SetUIEssentials();

        // 널방지를 위한 딜레이 입니다.
        DOVirtual.Float(0, 0, 1f, _ =>
        {
        }).OnComplete(() =>
        {
            _isLoadFinished = true;
        });

        DOVirtual.DelayedCall(3.5f, () =>
        {
            Managers.isGameStopped = false;
        });
        GetObject((int)UI.Btn_Home).BindEvent(()=>{
            Managers.UI.CloseAllPopupUI();
            Managers.UI.ShowPopupUI<UI_Home>();
        });
        // GetObject((int)UI.Btn_Setting).BindEvent(()=>
        // {
        //     Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
        // });
        GetObject((int)UI.Btn_Quit).BindEvent(()=>{
            Application.Quit();
        });
        GetObject((int)UI.Btn_Back).BindEvent(() =>
        {
            Logger.ContentTestLog($"{Managers.UI.GetUICounts()}");

         
            if (!(Managers.UI.currentPopupClass is UI_Home))
            {
                Managers.UI.ClosePopupUI();
                if (Managers.UI.GetUICounts() <= DEFAULT_UI_COUNT)
                {
                    Managers.UI.ShowPopupUI<UI_Home>();
                }

                return;
            }
            
        });
        
        
       
        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
        RaySynchronizer.OnGetInputFromUser += OnRaySynced;
        return base.InitEssentialUI();
    }


  
    private void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;


        Managers.Sound.Stop(SoundManager.Sound.Bgm);
    }

    private void SetUIEssentials()
    {
        _launcherGR = GameObject.FindWithTag("UIManager").GetComponent<GraphicRaycaster>();
        _launcherPED =  new PointerEventData(EventSystem.current);
    }

    public void OnRaySynced()
    {
       // Logger.CoreClassLog("Raysync On Launcher -----------------");
        //마우스 및 포인터 위치를 기반으로 하고싶은경우.
        screenPosition = Mouse.current.position.ReadValue();
        if (_launcherPED != null) _launcherPED.position = screenPosition;
    //    else Logger.CoreClassLog("PointerEventData is null -----------------");

        _results = new List<RaycastResult>();
        if (_launcherGR != null) _launcherGR.Raycast(_launcherPED, _results);
      //  else Logger.CoreClassLog("laucnherGR is null -----------------");

        if (_results.Count <= 0) return;
        {
           // Logger.CoreClassLog("Raysync On Scene Selection -----------------");
            //OnSceneBtnClicked(_results);
        }
    }

    private string SetButtonString(string input)
    {
        if (!input.Contains("Button")) return null;

        string originalName = input;
        string modifiedName = originalName.Substring(0, originalName.Length - 6);
        return modifiedName;
    }


    
    public void OnSceneBtnClicked(string sceneId,string title)
    {
        DOVirtual.Float(0, 0, 0.1f, _ =>
            {
            })
            .OnComplete(() =>
            {
                MetaEduLauncher.UIType clickedUI = 0;



                GameNameWaitingForConfirmation = sceneId;
                GameKoreanName = title;
                        // ** 씬 로드** ---------------------------------------------------------
            //    Logger.Log($"{result.gameObject.name}게임 씬 버튼 클릭 됨--------------------------------------");
                 
                Managers.UI.ShowPopupUI<UI_Confirmation>();
                        return;
                        


                 
                    
            });
    }
    
    public override void OnPopupUI()
    {
        //첫실행에서는 리턴 
        if(GetObject((int)UI.Btn_Home) ==null)return;
        RefreshUI();
    }

    private void RefreshUI()
    {
     if(Managers.UI.FindPopup<UI_Home>())
     {
         GetObject((int)UI.Btn_Home).SetActive(false);
         GetObject((int)UI.Btn_Quit).SetActive(true);
         GetObject((int)UI.Btn_Back).SetActive(false);
       //  GetObject((int)UI.Btn_Setting).SetActive(true);
       Logger.CoreClassLog("UI_MetaEduLauncherMaster RefreshUI() - Home UI 활성화");
         
     }
     else
     {
         GetObject((int)UI.Btn_Home).SetActive(true);
         GetObject((int)UI.Btn_Quit).SetActive(true);
         GetObject((int)UI.Btn_Back).SetActive(true);
     //    GetObject((int)UI.Btn_Setting).SetActive(true);
     }
    }
    
    
    public void UIOff()
    {
       GetObject((int)UI.Btn_Home).SetActive(false);
            GetObject((int)UI.Btn_Quit).SetActive(false);
            GetObject((int)UI.Btn_Back).SetActive(false);
          //  GetObject((int)UI.Btn_Setting).SetActive(false);

    }
    
    
}